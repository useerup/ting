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
and private collectDefinitionScopes (acc: ResizeArray<Scope>) (node: Node) : unit =
    match node.Exp with
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
    | StringLiteral parts ->
        for p in parts do
            collectDefinitionScopesFromPart acc p
    | Projection (s, e, m)
    | MemberProjection (s, e, m) ->
        addDefinitionScope acc s
        collectDefinitionScopes acc e
        collectDefinitionScopes acc m
    | Equals (a, b)
    | ValueEquals (a, b)
    | Catch (a, b) ->
        collectDefinitionScopes acc a
        collectDefinitionScopes acc b
    | Throw e
    | IsDefined e ->
        collectDefinitionScopes acc e
    | Unknown -> ()
    // Surface marker nodes: eliminated by the Binder before analysis.
    | Declare _
    | Reference _
    | DeclareLeft _
    | DeclareRight _
    | ReferenceLeft _
    | ReferenceRight _
    | BuiltIn _
    | PushScope _
    | PushMemberScope _ -> ()

// Public helper to collect scopes from an analyzed Exp.
let collectAllScopes (node: Node) : Scope list =
    let acc = ResizeArray<Scope>()
    collectDefinitionScopes acc node
    acc |> Seq.toList
