module ExpDiagnostics

open System
open Exp
open ExpScopeAnalysis

// Minimal name formatting for scope diagnostics.
let private formatInterpolatedPartForScope = function
    | TextPart s -> s
    | HolePart _ -> "{...}"

let private formatNameParts (parts: InterpolatedParts) =
    parts
    |> List.map formatInterpolatedPartForScope
    |> String.concat ""

let private nameOfRole = function
    | Unbound p
    | Pending (_, p)
    | Bound (_, _, p) -> p

let formatScope (s: Scope) =
    $"{s.Id}"

let private scopeTypeName (s: Scope) =
    match s with
    | :? LocalScope -> "Local"
    | :? ExpressionScope -> "Expression"
    | :? ExpressionMemberScope -> "ExpressionMember"
    | _ -> "Unknown"

let private publishUsageName = function
    | Exp.AccessModifier.Public -> "publish-public"
    | Exp.AccessModifier.Private -> "publish-private"
    | Exp.AccessModifier.Internal -> "publish-internal"
    | Exp.AccessModifier.Protected -> "publish-protected"

let private inferScopeUsages (exp: Exp) =
    let usages = System.Collections.Generic.Dictionary<Guid, System.Collections.Generic.HashSet<string>>()

    let addUsage (s: Scope) (usage: string) =
        let set =
            match usages.TryGetValue s.Id with
            | true, existing -> existing
            | _ ->
                let created = System.Collections.Generic.HashSet<string>()
                usages[s.Id] <- created
                created
        set.Add usage |> ignore

    let rec walk (node: Exp) =
        match node with
        | Local (_, s, e) ->
            addUsage s "local"
            walk e
        | Record (_, s, e) ->
            addUsage s "record"
            walk e
        | Publish (_, s, access, e) ->
            addUsage s (publishUsageName access)
            walk e
        | Product (_, positional, _, s, named) ->
            addUsage s "product"
            List.iter walk positional
            List.iter walk named
        | Projection (_, s, e, m) ->
            addUsage s "projection"
            walk e
            walk m
        | MemberProjection (_, s, e, m) ->
            addUsage s "member-projection"
            walk e
            walk m
        | ScopeExp (_, s, e) ->
            addUsage s "scope-exp"
            walk e
        | Apply (_, f, a)
        | Catch (_, f, a)
        | Equals (_, f, a)
        | DeclareLeft (_, f, a, _)
        | DeclareRight (_, f, a, _)
        | ReferenceLeft (_, f, a, _)
        | ReferenceRight (_, f, a, _)
        | PushScope (_, f, a)
        | PushMemberScope (_, f, a) ->
            walk f
            walk a
        | Tuple (_, xs)
        | Set (_, xs)
        | List (_, xs) -> List.iter walk xs
        | StringLiteral (_, parts) ->
            parts
            |> List.iter (function
                | TextPart _ -> ()
                | HolePart e -> walk e)
        | Throw (_, e)
        | Primary (_, e)
        | Candidate (_, e)
        | IsDefined (_, e)
        | Reference (_, e) -> walk e
        | Identifier _
        | This _
        | Literal _
        | Void _
        | Unknown _ -> ()

    walk exp
    usages

// Fixed reporting order, so a scope carrying several access modifiers reads as
// `Export(protected internal)` rather than in hash order.
let private accessModifierOrder = [ "public"; "protected"; "internal"; "private" ]

let private usageDisplayName = function
    | "local" -> "Local"
    | "record" -> "Record"
    | "product" -> "Product"
    | "projection" -> "Projection"
    | "member-projection" -> "MemberProjection"
    | "scope-exp" -> "Scope"
    | other -> other

let private formatScopeKind (usages: string list) (s: Scope) =
    let published =
        accessModifierOrder
        |> List.filter (fun a -> List.contains $"publish-{a}" usages)

    let plain =
        usages
        |> List.filter (fun u -> not (u.StartsWith "publish-"))
        |> List.sort
        |> List.map usageDisplayName

    let exported =
        if List.isEmpty published then
            []
        else
            let joined = String.concat " " published
            [ $"Export({joined})" ]

    match plain @ exported with
    | [] -> scopeTypeName s
    | parts -> String.concat ", " parts

let private formatScopeNames (s: Scope) =
    s.Names
    |> Seq.map (fun kvp -> formatNameParts kvp.Value.NameParts)
    |> String.concat ", "

// Scopes the tree does not point at (the root scope, the parser's operator
// scope) followed by every scope reachable from the tree, without duplicates.
let private allReportedScopes (ambient: Scope list) (exp: Exp) : Scope list =
    let seen = System.Collections.Generic.HashSet<Guid>()
    ambient @ collectAllScopes exp
    |> List.filter (fun s -> seen.Add s.Id)

// Build a textual scope report used by console and file output. Scopes are
// numbered from 1 in the order they are first met; the identifier report refers
// back to those numbers.
let renderScopeReportWith (ambient: Scope list) (exp: Exp) : string =
    let usagesByScope = inferScopeUsages exp
    allReportedScopes ambient exp
    |> List.mapi (fun i s ->
        let usages =
            match usagesByScope.TryGetValue s.Id with
            | true, entries -> List.ofSeq entries
            | _ -> []
        let namesText = formatScopeNames s
        if String.IsNullOrEmpty namesText then
            $"[{i + 1}] {formatScopeKind usages s} :"
        else
            $"[{i + 1}] {formatScopeKind usages s} : {namesText}")
    |> String.concat Environment.NewLine

let renderScopeReport (exp: Exp) : string = renderScopeReportWith [] exp

// Print each collected scope, its kind, and the names currently present in that scope.
let printScopeReport (exp: Exp) : unit =
    printfn "%s" (renderScopeReport exp)

// Render each identifier occurrence together with source position and binding
// metadata from the Binder/Resolver roles.
let renderIdentifierReportWith (ambient: Scope list) (exp: Exp) : string =
    let scopes = allReportedScopes ambient exp

    let scopeNumbers = scopes |> List.mapi (fun i s -> s.Id, i + 1) |> dict

    // A declaration may sit in several scopes at once: a contiguous declarative
    // segment, or a combined publish such as `protected internal`.
    let scopesOfDeclaration (declId: Guid) =
        scopes
        |> List.filter (fun s -> s.Names.ContainsKey declId)
        |> List.choose (fun s ->
            match scopeNumbers.TryGetValue s.Id with
            | true, number -> Some(string number)
            | _ -> None)

    let rec collect (node: Exp) (acc: (Range.Range * string * string) list) =
        let emit name role =
            let kindText, declaringScopes =
                match role with
                | Bound (Declared, declId, _) -> "decl", scopesOfDeclaration declId
                | Bound (Referenced, declId, _) -> "ref", scopesOfDeclaration declId
                | Pending _
                | Unbound _ -> "ref", []

            let target =
                match declaringScopes with
                | [] -> "unresolved"
                | numbers -> "scope " + String.concat ", " numbers

            (node.Range, name, $"{kindText} - {target}") :: acc

        match node with
        | Identifier (_, role) ->
            let name = nameOfRole role |> formatNameParts
            emit name role
        | This (_, role) -> emit "this" role
        | Local (_, _, e)
        | Record (_, _, e)
        | ScopeExp (_, _, e)
        | Publish (_, _, _, e)
        | Throw (_, e)
        | Primary (_, e)
        | Candidate (_, e)
        | IsDefined (_, e)
        | Reference (_, e) -> collect e acc
        | Apply (_, f, a)
        | Catch (_, f, a)
        | Equals (_, f, a)
        | DeclareLeft (_, f, a, _)
        | DeclareRight (_, f, a, _)
        | ReferenceLeft (_, f, a, _)
        | ReferenceRight (_, f, a, _)
        | PushScope (_, f, a)
        | PushMemberScope (_, f, a)
        | Projection (_, _, f, a)
        | MemberProjection (_, _, f, a) ->
            acc |> collect f |> collect a
        | Tuple (_, xs)
        | Set (_, xs)
        | List (_, xs) -> List.foldBack collect xs acc
        | Product (_, positional, _, _, named) ->
            acc |> List.foldBack collect positional |> List.foldBack collect named
        | StringLiteral (_, parts) ->
            List.foldBack
                (fun p st ->
                    match p with
                    | TextPart _ -> st
                    | HolePart e -> collect e st)
                parts
                acc
        | Literal _
        | Void _
        | Unknown _ -> acc

    let rows =
        collect exp []
        |> List.rev
        |> List.sortBy (fun (span, _, _) -> span.Start.Line, span.Start.Column, span.End.Line, span.End.Column)
        |> List.map (fun (span, name, roleText) ->
            $"{name}[{span.Start.Line},{span.Start.Column}] {roleText}")

    if List.isEmpty rows then
        "No identifier occurrences found."
    else
        String.concat Environment.NewLine rows

let renderIdentifierReport (exp: Exp) : string = renderIdentifierReportWith [] exp

// Render the analyzed Exp in a cycle-safe way for diagnostics/output files.
let rec private renderInterpolatedPart = function
    | TextPart s -> s
    | HolePart e -> $"{{{renderExpression e}}}"

and renderBindingRole = function
    | Unbound _ -> "unbound"
    | Bound (Declared, id, _) -> $"decl:{id}"
    | Bound (Referenced, id, _) -> $"resolved:{id}"
    | Pending (frames, _) ->
        let scopesText =
            frames
            |> List.choose (fun f ->
                match f.Mode with
                | ReferentialScope -> Some(string f.Scope.Id)
                | DeclarativeScope -> None)
            |> String.concat ", "
        $"ref:[{scopesText}]"

and renderExpression (node: Exp) : string =
    match node with
    | Local (_, s, e) -> $"local[{formatScope s}] {renderExpression e}"
    | Apply (_, f, a) -> $"({renderExpression f} {renderExpression a})"
    | Tuple (_, xs) -> xs |> List.map renderExpression |> String.concat ", " |> fun s -> $"({s})"
    | Product (_, positional, fusion, s, named) ->
        let mark =
            match fusion with
            | Fusing -> "@"
            | NonFusing -> "@@"
        let positionalText = positional |> List.map renderExpression |> String.concat ", "
        let namedText = named |> List.map renderExpression |> String.concat ", "
        let separator = if List.isEmpty positional then "" else " "
        $"({positionalText}{separator}{mark}[{formatScope s}] {namedText})"
    | Record (_, s, e) -> $"record[{formatScope s}] {renderExpression e}"
    | Publish (_, s, access, e) ->
        let accessText =
            match access with
            | Exp.AccessModifier.Public -> "public"
            | Exp.AccessModifier.Private -> "private"
            | Exp.AccessModifier.Internal -> "internal"
            | Exp.AccessModifier.Protected -> "protected"
        $"{accessText}[{formatScope s}] {renderExpression e}"
    | Identifier (_, role) -> $"id({renderBindingRole role})"
    | ScopeExp (_, s, e) -> $"scope[{formatScope s}] {renderExpression e}"
    | Literal (_, lit) ->
        match lit with
        | Exp.Int32 i -> string i
        | Exp.Boolean b -> string b
        | Exp.Float32 f -> string f
        | Exp.Float64 d -> string d
        | Exp.Decimal m -> string m
        | Exp.String s -> $"\"{s}\""
    | Set (_, xs) -> xs |> List.map renderExpression |> String.concat ", " |> fun s -> $"{{{s}}}"
    | List (_, xs) -> xs |> List.map renderExpression |> String.concat ", " |> fun s -> $"[{s}]"
    | StringLiteral (_, parts) ->
        parts |> List.map renderInterpolatedPart |> String.concat "" |> fun s -> $"$\"{s}\""
    | Projection (_, s, e, m) -> $"({renderExpression e}).[{formatScope s}]({renderExpression m})"
    | MemberProjection (_, s, e, m) -> $"({renderExpression e})..[{formatScope s}]({renderExpression m})"
    | Equals (_, a, b) -> $"({renderExpression a} = {renderExpression b})"
    | Primary (_, e) -> $"primary({renderExpression e})"
    | Candidate (_, e) -> $"candidate({renderExpression e})"
    | Throw (_, e) -> $"throw {renderExpression e}"
    | Catch (_, a, b) -> $"({renderExpression a} catch {renderExpression b})"
    | IsDefined (_, e) -> $"defined?({renderExpression e})"
    | This (_, role) -> $"this[{renderBindingRole role}]"
    | Void _ -> "void"
    | Unknown _ -> "???"
    // Surface markers: eliminated by the Binder before rendering.
    | Reference _
    | DeclareLeft _
    | DeclareRight _
    | ReferenceLeft _
    | ReferenceRight _
    | PushScope _
    | PushMemberScope _ -> "<surface>"
