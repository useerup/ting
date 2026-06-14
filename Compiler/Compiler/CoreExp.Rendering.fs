module CoreExpDiagnostics

open System
open CoreExp
open CoreExpScopeAnalysis

// Minimal name formatting for scope diagnostics.
let private formatInterpolatedPartForScope = function
    | TextPart s -> s
    | HolePart _ -> "{...}"

let private formatNameParts (parts: CoreInterpolatedPart list) =
    parts
    |> List.map formatInterpolatedPartForScope
    |> String.concat ""

let private formatScope (s: Scope) =
    $"{s.Id}"

let private scopeKindName (s: Scope) =
    match s with
    | :? LocalScope -> "NamesScope"
    | :? ExpressionScope -> "ExpressionScope"
    | :? ExpressionMemberScope -> "ExpressionMemberScope"
    | :? RootScope -> "RootScope"
    | _ -> "UnknownScope"

let private formatScopeNames (s: Scope) =
    s.Names
    |> Seq.map (fun kvp -> formatNameParts kvp.Value)
    |> String.concat ", "

// Build a textual scope report used by console and file output.
let renderScopeReport (exp: CoreExp) : string =
    collectAllScopes exp
    |> List.mapi (fun i s ->
        let namesText = formatScopeNames s
        [
            $"[{i}] {scopeKindName s, -20} {formatScope s}"
            $"    names=[{namesText}]"
        ])
    |> List.concat
    |> String.concat Environment.NewLine

// Print each collected scope, its kind, and the names currently present in that scope.
let printScopeReport (exp: CoreExp) : unit =
    printfn "%s" (renderScopeReport exp)

// Render CoreExp in a cycle-safe way for diagnostics/output files.
let rec private renderInterpolatedPart = function
    | TextPart s -> s
    | HolePart e -> $"{{{renderExpression e}}}"

and private renderBindingRole = function
    | Declarative (id, _) -> $"decl:{id}"
    | Referential (scopeIds, _) ->
        let scopesText = scopeIds |> List.map string |> String.concat ", "
        $"ref:[{scopesText}]"

and renderExpression (exp: CoreExp) : string =
    match exp with
    | Local (s, e) -> $"local[{formatScope s}] {renderExpression e}"
    | Apply (f, a) -> $"({renderExpression f} {renderExpression a})"
    | Tuple xs -> xs |> List.map renderExpression |> String.concat ", " |> fun s -> $"({s})"
    | Record (s, e) -> $"record[{formatScope s}] {renderExpression e}"
    | Publish (s, access, e) ->
        let accessText =
            match access with
            | Exp.AccessModifier.Public -> "public"
            | Exp.AccessModifier.Private -> "private"
            | Exp.AccessModifier.Internal -> "internal"
            | Exp.AccessModifier.Protected -> "protected"
        $"{accessText}[{formatScope s}] {renderExpression e}"
    | Identifier role -> $"id({renderBindingRole role})"
    | ScopeExp (s, e) -> $"scope[{formatScope s}] {renderExpression e}"
    | Literal lit ->
        match lit with
        | Exp.Int32 i -> string i
        | Exp.Boolean b -> string b
        | Exp.Float32 f -> string f
        | Exp.Float64 d -> string d
        | Exp.Decimal m -> string m
        | Exp.String s -> $"\"{s}\""
    | Set xs -> xs |> List.map renderExpression |> String.concat ", " |> fun s -> $"{{{s}}}"
    | List xs -> xs |> List.map renderExpression |> String.concat ", " |> fun s -> $"[{s}]"
    | InterpolatedString parts ->
        parts |> List.map renderInterpolatedPart |> String.concat "" |> fun s -> $"$\"{s}\""
    | Projection (s, e, m) -> $"({renderExpression e}).[{formatScope s}]({renderExpression m})"
    | MemberProjection (s, e, m) -> $"({renderExpression e})..[{formatScope s}]({renderExpression m})"
    | StructuralEquals (a, b) -> $"({renderExpression a} = {renderExpression b})"
    | IdentityEquals (a, b) -> $"({renderExpression a} == {renderExpression b})"
    | This role -> $"this[{renderBindingRole role}]"
    | Void -> "void"
