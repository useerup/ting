module CoreExp

open System
open Exp


// Interpolated fragments used in identifiers and strings after analysis.
type CoreInterpolatedPart =
    | TextPart of string
    | HolePart of CoreExp

// Name table for a scope: declaration id -> analyzed name parts.
and Names = System.Collections.Generic.Dictionary<Guid,CoreInterpolatedPart list>

// Core, scope-annotated expression tree produced by Analyzer.markDeclarations.
and CoreExp =
    | Local of Scope * CoreExp
    | Apply of CoreExp * CoreExp
    | Tuple of CoreExp list
    | Record of Scope * CoreExp
    | Publish of Scope * Exp.AccessModifier * CoreExp
    | Identifier of BindingRole
    | ScopeExp of Scope * CoreExp
    | Literal of Exp.Literal
    | Set of CoreExp list
    | List of CoreExp list
    | InterpolatedString of CoreInterpolatedPart list
    | Projection of Scope * CoreExp * CoreExp
    | MemberProjection of Scope * CoreExp * CoreExp
    | Equals of CoreExp * CoreExp
    | This of BindingRole
    | Void

// Scope hierarchy.
and [<AbstractClass>] Scope(id: Guid, names: Names) =
    member _.Id = id
    member _.Names = names

and RootScope() =
    inherit Scope(Guid.Empty, new Names())

and [<AbstractClass>] ChildScope(id: Guid) =
    inherit Scope(id, new Names())

and LocalScope(id: Guid) =
    inherit ChildScope(id)

and ExpressionScope(id: Guid, expression: CoreExp) =
    inherit ChildScope(id)
    member _.Expression = expression

and ExpressionMemberScope(id: Guid, expression: CoreExp) =
    inherit ChildScope(id)
    member _.Expression = expression

// Declarative carries declaration identity; referential carries candidate scope ids + name pattern.
and BindingRole =
    | Declarative of Guid * CoreInterpolatedPart list
    | Referential of Guid list * CoreInterpolatedPart list

type ScopeStack = Scope list * Scope list


let scopeId (s: Scope) = s.Id

let newNamesScope () = LocalScope(Guid.NewGuid()) :> Scope

let newExpressionScope (e: CoreExp) = ExpressionScope(Guid.NewGuid(), e) :> Scope

let newExpressionMemberScope (e: CoreExp) = ExpressionMemberScope(Guid.NewGuid(), e) :> Scope

let rootScope = RootScope() :> Scope

// Add the same declaration id/name into each declarative scope.
let declareName (id: Guid) (name: CoreInterpolatedPart list) (decls: Scope list) : unit =
    for s in decls do
        s.Names.Add(id, name) |> ignore

// Minimal name formatting for scope diagnostics.
let private formatInterpolatedPartForScope = function
    | TextPart s -> s
    | HolePart _ -> "{...}"

let private formatNameParts (parts: CoreInterpolatedPart list) =
    parts
    |> List.map formatInterpolatedPartForScope
    |> String.concat ""

let private formatScopeKind (s: Scope) =
    match s with
    | :? LocalScope -> "names"
    | :? ExpressionScope -> "expr"
    | :? ExpressionMemberScope -> "member-expr"
    | :? RootScope -> "root"
    | _ -> "unknown"

let private formatScope (s: Scope) =
    $"{s.Id}"

// Traverse interpolation holes to find nested scope-defining expressions.
let rec private collectDefinitionScopesFromPart (acc: ResizeArray<Scope>) = function
    | TextPart _ -> ()
    | HolePart e -> collectDefinitionScopes acc e

// If a scope carries an expression, traverse it as well.
and private collectDefinitionScopesFromScope (acc: ResizeArray<Scope>) (s: Scope) =
    match s with
    | :? LocalScope -> ()
    | :? ExpressionScope as k -> collectDefinitionScopes acc k.Expression
    | :? ExpressionMemberScope as k -> collectDefinitionScopes acc k.Expression
    | :? RootScope -> ()
    | _ -> ()

// Referential roles are traversed only through interpolated name holes.
and private collectDefinitionScopesFromRole (acc: ResizeArray<Scope>) = function
    | Declarative _ -> ()
    | Referential (_, nameParts) ->
        for p in nameParts do
            collectDefinitionScopesFromPart acc p

// Collect only scope kinds requested for reporting.
and private addDefinitionScope (acc: ResizeArray<Scope>) (s: Scope) =
    match s with
    | :? LocalScope
    | :? ExpressionScope
    | :? ExpressionMemberScope ->
        acc.Add s
        collectDefinitionScopesFromScope acc s
    | :? RootScope -> ()
    | _ -> ()

// Walk the CoreExp tree and collect relevant scopes into acc.
and private collectDefinitionScopes (acc: ResizeArray<Scope>) (exp: CoreExp) : unit =
    match exp with
    | Local (s, e) ->
        addDefinitionScope acc s
        collectDefinitionScopes acc e
    | Apply (f, a) ->
        collectDefinitionScopes acc f
        collectDefinitionScopes acc a
    | Tuple xs
    | Set xs
    | List xs ->
        for x in xs do
            collectDefinitionScopes acc x
    | Record (s, e) ->
        addDefinitionScope acc s
        collectDefinitionScopes acc e
    | Publish (s, _, e) ->
        addDefinitionScope acc s
        collectDefinitionScopes acc e
    | ScopeExp (_, e) ->
        collectDefinitionScopes acc e
    | Identifier role
    | This role ->
        collectDefinitionScopesFromRole acc role
    | Literal _
    | Void -> ()
    | InterpolatedString parts ->
        for p in parts do
            collectDefinitionScopesFromPart acc p
    | Projection (s, e, m)
    | MemberProjection (s, e, m) ->
        addDefinitionScope acc s
        collectDefinitionScopes acc e
        collectDefinitionScopes acc m
    | Equals (a, b) ->
        collectDefinitionScopes acc a
        collectDefinitionScopes acc b

// Public helper to collect scopes from a CoreExp.
let getAllScopes (exp: CoreExp) : Scope list =
    let acc = ResizeArray<Scope>()
    collectDefinitionScopes acc exp
    acc |> Seq.toList

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

//
// Build a textual scope report used by console and file output.
let renderAllScopes (exp: CoreExp) : string =
    getAllScopes exp
    |> List.mapi (fun i s ->
        let namesText = formatScopeNames s
        [
            $"[{i}] {scopeKindName s, -20} {formatScope s}"
            $"    names=[{namesText}]"
        ])
    |> List.concat
    |> String.concat Environment.NewLine

// Print each collected scope, its kind, and the names currently present in that scope.
let printAllScopes (exp: CoreExp) : unit =
    printfn "%s" (renderAllScopes exp)

// Render CoreExp in a cycle-safe way for diagnostics/output files.
let rec private renderInterpolatedPart = function
    | TextPart s -> s
    | HolePart e -> $"{{{renderCoreExp e}}}"

and private renderBindingRole = function
    | Declarative (id, _) -> $"decl:{id}"
    | Referential (scopeIds, _) ->
        let scopesText = scopeIds |> List.map string |> String.concat ", "
        $"ref:[{scopesText}]"

and renderCoreExp (exp: CoreExp) : string =
    match exp with
    | Local (s, e) -> $"local[{formatScope s}] {renderCoreExp e}"
    | Apply (f, a) -> $"({renderCoreExp f} {renderCoreExp a})"
    | Tuple xs -> xs |> List.map renderCoreExp |> String.concat ", " |> fun s -> $"({s})"
    | Record (s, e) -> $"record[{formatScope s}] {renderCoreExp e}"
    | Publish (s, access, e) ->
        let accessText =
            match access with
            | Exp.AccessModifier.Public -> "public"
            | Exp.AccessModifier.Private -> "private"
            | Exp.AccessModifier.Internal -> "internal"
            | Exp.AccessModifier.Protected -> "protected"
        $"{accessText}[{formatScope s}] {renderCoreExp e}"
    | Identifier role -> $"id({renderBindingRole role})"
    | ScopeExp (s, e) -> $"scope[{formatScope s}] {renderCoreExp e}"
    | Literal lit ->
        match lit with
        | Exp.Int32 i -> string i
        | Exp.Boolean b -> string b
        | Exp.Float32 f -> string f
        | Exp.Float64 d -> string d
        | Exp.Decimal m -> string m
        | Exp.String s -> $"\"{s}\""
    | Set xs -> xs |> List.map renderCoreExp |> String.concat ", " |> fun s -> $"{{{s}}}"
    | List xs -> xs |> List.map renderCoreExp |> String.concat ", " |> fun s -> $"[{s}]"
    | InterpolatedString parts ->
        parts |> List.map renderInterpolatedPart |> String.concat "" |> fun s -> $"$\"{s}\""
    | Projection (s, e, m) -> $"({renderCoreExp e}).[{formatScope s}]({renderCoreExp m})"
    | MemberProjection (s, e, m) -> $"({renderCoreExp e})..[{formatScope s}]({renderCoreExp m})"
    | Equals (a, b) -> $"({renderCoreExp a} == {renderCoreExp b})"
    | This role -> $"this[{renderBindingRole role}]"
    | Void -> "void"

