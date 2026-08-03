module ExpScopeAnalysis

open Exp

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
    | _ -> ()

// Referential/unbound roles are traversed only through interpolated name holes.
and private collectDefinitionScopesFromRole (acc: ResizeArray<Scope>) = function
    | Declarative _ -> ()
    | Resolved _ -> ()
    | Unbound nameParts
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
    | _ -> ()

// Walk the analyzed Exp tree and collect relevant scopes into acc.
and private collectDefinitionScopes (acc: ResizeArray<Scope>) (node: Exp) : unit =
    match node with
    | Local (_, s, e) ->
        addDefinitionScope acc s
        collectDefinitionScopes acc e
    | Apply (_, f, a) ->
        collectDefinitionScopes acc f
        collectDefinitionScopes acc a
    | Tuple (_, xs)
    | Set (_, xs)
    | List (_, xs) ->
        for x in xs do
            collectDefinitionScopes acc x
    | Product (_, positional, _, s, named) ->
        addDefinitionScope acc s
        for x in positional do
            collectDefinitionScopes acc x
        for x in named do
            collectDefinitionScopes acc x
    | Record (_, s, e) ->
        addDefinitionScope acc s
        collectDefinitionScopes acc e
    | Publish (_, s, _, e) ->
        addDefinitionScope acc s
        collectDefinitionScopes acc e
    | ScopeExp (_, _, e) ->
        collectDefinitionScopes acc e
    | Identifier (_, role)
    | This (_, role) ->
        collectDefinitionScopesFromRole acc role
    | Literal _
    | Void _ -> ()
    | StringLiteral (_, parts) ->
        for p in parts do
            collectDefinitionScopesFromPart acc p
    | Projection (_, s, e, m)
    | MemberProjection (_, s, e, m) ->
        addDefinitionScope acc s
        collectDefinitionScopes acc e
        collectDefinitionScopes acc m
    | Equals (_, a, b)
    | Catch (_, a, b) ->
        collectDefinitionScopes acc a
        collectDefinitionScopes acc b
    | Throw (_, e)
    | Primary (_, e)
    | Candidate (_, e)
    | IsDefined (_, e) ->
        collectDefinitionScopes acc e
    | Unknown _ -> ()
    // Surface markers: eliminated by the Binder before analysis.
    | Reference _
    | DeclareLeft _
    | DeclareRight _
    | ReferenceLeft _
    | ReferenceRight _
    | PushScope _
    | PushMemberScope _ -> ()

// Public helper to collect scopes from an analyzed Exp.
let collectAllScopes (node: Exp) : Scope list =
    let acc = ResizeArray<Scope>()
    collectDefinitionScopes acc node
    acc |> Seq.toList
