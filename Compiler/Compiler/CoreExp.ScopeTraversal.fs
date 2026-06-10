module CoreExpScopeAnalysis

open CoreExp

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
let collectAllScopes (exp: CoreExp) : Scope list =
    let acc = ResizeArray<Scope>()
    collectDefinitionScopes acc exp
    acc |> Seq.toList
