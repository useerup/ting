module CoreScopeAnalysis

open Exp
open Core

// Collect the scopes reachable from a core tree, in the order they are met.
// The counterpart of ExpScopeAnalysis for the post-lowering tree: the scopes
// themselves are the same objects, since lowering carries them across untouched.

let rec private collectFromPart (acc: ResizeArray<Scope>) = function
    | CoreText _ -> ()
    | CoreHole e -> collectScopes acc e

// If a scope carries an expression, traverse it as well. Those expressions are
// surface nodes - a scope is built during binding, before lowering exists - so
// the surface traversal handles them.
and private collectFromScope (acc: ResizeArray<Scope>) (s: Scope) =
    match s with
    | :? ExpressionScope as k -> ExpScopeAnalysis.collectAllScopes k.Expression |> List.iter acc.Add
    | :? ExpressionMemberScope as k -> ExpScopeAnalysis.collectAllScopes k.Expression |> List.iter acc.Add
    | _ -> ()

// Referential/unbound roles are traversed only through interpolated name holes,
// which are surface expressions for the same reason.
and private collectFromRole (acc: ResizeArray<Scope>) = function
    | Declarative _
    | Resolved _ -> ()
    | Unbound nameParts
    | Referential (_, nameParts) ->
        for p in nameParts do
            match p with
            | TextPart _ -> ()
            | HolePart e -> ExpScopeAnalysis.collectAllScopes e |> List.iter acc.Add

and private addScope (acc: ResizeArray<Scope>) (s: Scope) =
    match s with
    | :? LocalScope
    | :? ExpressionScope
    | :? ExpressionMemberScope ->
        acc.Add s
        collectFromScope acc s
    | _ -> ()

and private collectScopes (acc: ResizeArray<Scope>) (node: CoreNode) : unit =
    match node.Core with
    | CoreExp.Apply (f, a) ->
        collectScopes acc f
        collectScopes acc a
    | CoreExp.Tuple xs
    | CoreExp.Set xs
    | CoreExp.List xs ->
        for x in xs do
            collectScopes acc x
    | CoreExp.Product (positional, _, s, named) ->
        addScope acc s
        for x in positional do
            collectScopes acc x
        for x in named do
            collectScopes acc x
    | CoreExp.StringLiteral parts ->
        for p in parts do
            collectFromPart acc p
    | CoreExp.Literal _
    | CoreExp.Void
    | CoreExp.Unknown -> ()
    | CoreExp.Identifier role
    | CoreExp.This role -> collectFromRole acc role
    | CoreExp.Local (s, e)
    | CoreExp.Scoped (s, e)
    | CoreExp.Record (s, e)
    | CoreExp.Publish (_, s, e) ->
        addScope acc s
        collectScopes acc e
    | CoreExp.Project (r, m)
    | CoreExp.ProjectMembers (r, m)
    | CoreExp.Equals (r, m)
    | CoreExp.Catch (r, m) ->
        collectScopes acc r
        collectScopes acc m
    | CoreExp.Primary e
    | CoreExp.Candidate e
    | CoreExp.IsDefined e
    | CoreExp.Throw e -> collectScopes acc e

/// Public helper to collect scopes from a core tree.
let collectAllScopes (node: CoreNode) : Scope list =
    let acc = ResizeArray<Scope>()
    collectScopes acc node
    acc |> Seq.toList
