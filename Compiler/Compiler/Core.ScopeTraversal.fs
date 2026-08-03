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

and private collectScopes (acc: ResizeArray<Scope>) (node: CoreExp) : unit =
    match node with
    | CoreExp.Apply (_, f, a) ->
        collectScopes acc f
        collectScopes acc a
    | CoreExp.Tuple (_, xs)
    | CoreExp.Set (_, xs)
    | CoreExp.List (_, xs) ->
        for x in xs do
            collectScopes acc x
    | CoreExp.Product (_, positional, _, s, named) ->
        addScope acc s
        for x in positional do
            collectScopes acc x
        for x in named do
            collectScopes acc x
    | CoreExp.StringLiteral (_, parts) ->
        for p in parts do
            collectFromPart acc p
    | CoreExp.Literal _
    | CoreExp.Void _
    | CoreExp.Unknown _ -> ()
    | CoreExp.Identifier (_, role)
    | CoreExp.This (_, role) -> collectFromRole acc role
    | CoreExp.Local (_, s, e)
    | CoreExp.Scoped (_, s, e)
    | CoreExp.Record (_, s, e)
    | CoreExp.Publish (_, _, s, e) ->
        addScope acc s
        collectScopes acc e
    | CoreExp.Project (_, r, m)
    | CoreExp.ProjectMembers (_, r, m)
    | CoreExp.Equals (_, r, m)
    | CoreExp.Catch (_, r, m) ->
        collectScopes acc r
        collectScopes acc m
    | CoreExp.Primary (_, e)
    | CoreExp.Candidate (_, e)
    | CoreExp.IsDefined (_, e)
    | CoreExp.Throw (_, e) -> collectScopes acc e

/// Public helper to collect scopes from a core tree.
let collectAllScopes (node: CoreExp) : Scope list =
    let acc = ResizeArray<Scope>()
    collectScopes acc node
    acc |> Seq.toList
