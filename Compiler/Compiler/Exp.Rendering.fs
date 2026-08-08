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

let private formatIdentifierRole (role: BindingRole) =
    match role with
    | Unbound _ -> "undefined"
    | Bound (Declared, _, _) -> "declarative"
    | Pending _ -> "referential"
    | Bound (Referenced, _, _) -> "referential(resolved)"

let private nameOfRole = function
    | Unbound p
    | Pending (_, p)
    | Bound (_, _, p) -> p

let formatScope (s: Scope) =
    $"{s.Id}"

let private scopeKindName (s: Scope) =
    match s with
    | :? LocalScope -> "NamesScope"
    | :? ExpressionScope -> "ExpressionScope"
    | :? ExpressionMemberScope -> "ExpressionMemberScope"
    | _ -> "UnknownScope"

let private declarationKindText (entry: NameEntry) =
    match entry.Host with
    | None -> "name"
    | Some (DirectPropertyHost _) -> "property"
    | Some (MemberPropertyHost _) -> "member-property"

// Short display of a host expression: the host's name plus the access marker
// (`.` for direct properties, `..` for member properties). Proves the host was
// recorded in the scope, e.g. `host=int.` or `host=int..`.
let private formatHostSuffix (entry: NameEntry) =
    let rec hostName (n: Exp) =
        match n with
        | Identifier (_, Bound (_, _, parts))
        | Identifier (_, Pending (_, parts)) -> formatNameParts parts
        | Local (_, _, x)
        | Record (_, _, x)
        | ScopeExp (_, _, x)
        | Publish (_, _, _, x) -> hostName x
        | _ -> "?"

    match entry.Host with
    | None -> ""
    | Some (DirectPropertyHost e) -> $" host={hostName e}."
    | Some (MemberPropertyHost e) -> $" host={hostName e}.."

let private collectProjectedDeclarationKinds (exp: Exp) =
    let kinds = System.Collections.Generic.Dictionary<Guid, string>()

    let setKind id kind =
        if not (kinds.ContainsKey id) then
            kinds.Add(id, kind)

    let rec collectFromMember isMemberProjection (memberExp: Exp) =
        match memberExp with
        | Identifier (_, Bound (Declared, id, _)) ->
            setKind id (if isMemberProjection then "extension member-property" else "extension property")
        | Tuple (_, xs)
        | Set (_, xs)
        | List (_, xs) -> xs |> List.iter (collectFromMember isMemberProjection)
        | Product (_, positional, _, _, named) ->
            positional |> List.iter (collectFromMember isMemberProjection)
            named |> List.iter (collectFromMember isMemberProjection)
        | Local (_, _, e)
        | Record (_, _, e)
        | ScopeExp (_, _, e)
        | Publish (_, _, _, e)
        | Throw (_, e)
        | Primary (_, e)
        | Candidate (_, e)
        | IsDefined (_, e) -> collectFromMember isMemberProjection e
        | Apply (_, f, a)
        | Catch (_, f, a)
        | Equals (_, f, a) ->
            collectFromMember isMemberProjection f
            collectFromMember isMemberProjection a
        | Projection (_, _, _, m) -> collectFromMember isMemberProjection m
        | MemberProjection (_, _, _, m) -> collectFromMember true m
        | _ -> ()

    let rec walk (n: Exp) =
        match n with
        | Local (_, _, x)
        | Record (_, _, x)
        | ScopeExp (_, _, x)
        | Publish (_, _, _, x)
        | Throw (_, x)
        | Primary (_, x)
        | Candidate (_, x)
        | IsDefined (_, x) -> walk x
        | Apply (_, f, a)
        | Catch (_, f, a)
        | Equals (_, f, a) ->
            walk f
            walk a
        | Tuple (_, xs)
        | Set (_, xs)
        | List (_, xs) -> xs |> List.iter walk
        | Product (_, positional, _, _, named) ->
            positional |> List.iter walk
            named |> List.iter walk
        | Projection (_, _, receiver, m) ->
            collectFromMember false m
            walk receiver
            walk m
        | MemberProjection (_, _, receiver, m) ->
            collectFromMember true m
            walk receiver
            walk m
        | _ -> ()

    walk exp
    kinds

let private formatScopeNames (projectedKinds: System.Collections.Generic.Dictionary<Guid, string>) (s: Scope) =
    s.Names
    |> Seq.map (fun kvp ->
        let entry = kvp.Value
        let kindText =
            match projectedKinds.TryGetValue kvp.Key with
            | true, projectedKind -> projectedKind
            | _ -> declarationKindText entry
        $"{formatNameParts entry.NameParts}<{kindText}>{formatHostSuffix entry}")
    |> String.concat ", "

// Build a textual scope report used by console and file output.
let renderScopeReport (exp: Exp) : string =
    let projectedKinds = collectProjectedDeclarationKinds exp
    collectAllScopes exp
    |> List.mapi (fun i s ->
        let namesText = formatScopeNames projectedKinds s
        [
            $"[{i}] {scopeKindName s, -20} {formatScope s}"
            $"    names=[{namesText}]"
        ])
    |> List.concat
    |> String.concat Environment.NewLine

// Print each collected scope, its kind, and the names currently present in that scope.
let printScopeReport (exp: Exp) : unit =
    printfn "%s" (renderScopeReport exp)

// Render each identifier occurrence together with source position and binding
// metadata from the Binder/Resolver roles.
let renderIdentifierReport (exp: Exp) : string =
    let scopes = collectAllScopes exp

    let declarativeScopes (declId: Guid) =
        scopes
        |> List.filter (fun s -> s.Names.ContainsKey declId)
        |> List.map formatScope
        |> String.concat ", "

    let rec collect (node: Exp) (acc: (Range.Range * string) list) =
        let emit role =
            let name = nameOfRole role |> formatNameParts
            let roleText = formatIdentifierRole role

            let details =
                match role with
                | Bound (Declared, declId, _) ->
                    let scopesText = declarativeScopes declId
                    $"bound-scopes=[{scopesText}]"
                | Pending (frames, _) ->
                    let candidateText =
                        frames
                        |> List.choose (fun f ->
                            match f.Mode with
                            | ReferentialScope -> Some(string f.Scope.Id)
                            | DeclarativeScope -> None)
                        |> String.concat ", "
                    $"candidate-scopes=[{candidateText}]"
                | Bound (Referenced, declId, _) ->
                    $"resolved-declaration={declId}"
                | Unbound _ -> ""

            let line =
                if String.IsNullOrWhiteSpace details then
                    $"name='{name}' role={roleText}"
                else
                    $"name='{name}' role={roleText} {details}"

            (node.Range, line) :: acc

        match node with
        | Identifier (_, role) -> emit role
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
        | This _
        | Literal _
        | Void _
        | Unknown _ -> acc

    let rows =
        collect exp []
        |> List.rev
        |> List.sortBy (fun (span, _) -> span.Start.Line, span.Start.Column, span.End.Line, span.End.Column)
        |> List.mapi (fun i (span, line) -> $"[{i}] ({span.Start.Line},{span.Start.Column}) {line}")

    if List.isEmpty rows then
        "No identifier occurrences found."
    else
        String.concat Environment.NewLine rows

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
