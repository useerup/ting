module Binder

open Exp
open ExpScopes

// Scope-annotation pass over the unified AST. Runs as part of parsing (see
// Parser.parse): it eliminates the parser's binding-intent marker nodes,
// assigns declaration ids to declarative identifiers (recording them in their
// declarative scopes), and attaches the ordered candidate scope-id list to
// every referential identifier.

module private ScopeStackOps =
    let addDeclarative (s: Scope) (scopes: ScopeStack) : ScopeStack =
        let (decls, refs) = scopes
        (s :: decls, s :: refs)

    let addReferential (s: Scope) (scopes: ScopeStack) : ScopeStack =
        let (decls, refs) = scopes
        (decls, s :: refs)

    let makeReferential (scopes: ScopeStack) : ScopeStack =
        let (_, refs) = scopes
        ([], refs)

module private Binding =
    let bindingRole ((decls, refs): ScopeStack) (name: InterpolatedParts) : BindingRole =
        if decls.IsEmpty then
            Referential (refs |> List.map scopeId, name)
        else
            let id = System.Guid.NewGuid()
            declareNameInScopes id name decls
            Declarative (id, name)

module private Marking =
    open ScopeStackOps
    open Binding

    // Collect the declaration ids introduced by a (member) expression so their
    // host can be recorded after binding. Used for `int.Zero = ...` (a single
    // declarative member) and destructuring members like `int.(a, b) = ...`.
    let private collectDeclarativeIds (exp: Node) : System.Guid list =
        let acc = ResizeArray<System.Guid>()

        let rec walk (n: Node) =
            match n.Exp with
            | Identifier (Declarative (id, _)) -> acc.Add id
            | Apply (f, a) ->
                walk f
                walk a
            | Tuple xs
            | Set xs
            | List xs -> List.iter walk xs
            | Local (_, x)
            | Record (_, x)
            | Publish (_, _, x)
            | ScopeExp (_, x)
            | Throw x
            | IsDefined x -> walk x
            | Projection (_, _, m)
            | MemberProjection (_, _, m) -> walk m
            | Catch (a, b)
            | Equals (a, b)
            | ValueEquals (a, b) ->
                walk a
                walk b
            | StringLiteral parts ->
                parts
                |> List.iter (function
                    | HolePart e -> walk e
                    | TextPart _ -> ())
            | _ -> ()

        walk exp
        acc |> List.ofSeq

    // Record where a projection-declared name lives: e.g. `int.Zero` hosts `Zero`
    // directly on `int`, and `int..Double` hosts `Double` on the members of `int`.
    let private attachHost (declScopes: Scope list) (host: NameHost) (memberExp: Node) : unit =
        for id in collectDeclarativeIds memberExp do
            for s in declScopes do
                match s.Names.TryGetValue id with
                | true, entry -> s.Names.[id] <- { entry with Host = Some host }
                | _ -> ()

    // Lowering preserves source spans: a node produced from a source construct
    // reports that construct's range (Node.like), and a node synthesized to group
    // operands reports the span of those operands (Node.spanning).
    let rec markDeclarations (rootScope: Scope) (node: Node) (scopes: ScopeStack) : Node =
        let recur e s = markDeclarations rootScope e s

        match node.Exp with
        | Declare e ->
            let s = createNamesScope()
            Node.like node (Local(s, recur e (addDeclarative s scopes)))
        | Reference e ->
            recur e (makeReferential scopes)
        | DeclareLeft (f, left, right) ->
            let s = createNamesScope()
            let head = recur f (makeReferential scopes)
            let left = recur left (addDeclarative s scopes)
            let right = recur right (addReferential s scopes)
            let args = Node.spanning left right (Tuple [ left; right ])
            Node.like node (Local(s, Node.like node (Apply(head, args))))
        | DeclareRight (f, left, right) ->
            let s = createNamesScope()
            let head = recur f (makeReferential scopes)
            let left = recur left (addReferential s scopes)
            let right = recur right (addDeclarative s scopes)
            let args = Node.spanning left right (Tuple [ left; right ])
            Node.like node (Local(s, Node.like node (Apply(head, args))))
        | ReferenceLeft (f, left, right) ->
            let refscopes = makeReferential scopes
            let head = recur f refscopes
            let left = recur left refscopes
            let right = recur right scopes
            let args = Node.spanning left right (Tuple [ left; right ])
            Node.like node (Apply(head, args))
        | ReferenceRight (f, left, right) ->
            let refscopes = makeReferential scopes
            let head = recur f refscopes
            let left = recur left scopes
            let right = recur right refscopes
            let args = Node.spanning left right (Tuple [ left; right ])
            Node.like node (Apply(head, args))
        | Apply (f, a) ->
            Node.like node (Apply(recur f scopes, recur a scopes))
        | Publish (s, access, e) ->
            Node.like node (Publish(s, access, recur e (addDeclarative s scopes)))
        | PushScope (se, e) ->
            let s = createExpressionScope(recur se (makeReferential scopes))
            recur e (addReferential s scopes)
        | PushMemberScope (se, e) ->
            let s = createExpressionMemberScope(recur se (makeReferential scopes))
            recur e (addReferential s scopes)
        | Tuple items ->
            Node.like node (Tuple(List.map (fun e -> recur e scopes) items))
        | Record (s, e) ->
            Node.like node (Record(s, recur e (addDeclarative s scopes)))
        | Identifier (Unbound parts) ->
            let parts = List.map (fun p -> markDeclarationsPart rootScope p (makeReferential scopes)) parts
            Node.like node (Identifier(bindingRole scopes parts))
        | Identifier _ ->
            node
        | BuiltIn name ->
            // The parser gives an operator's BuiltIn head the operator token's own
            // range, so the lowered identifier still points at the operator.
            Node.like node (Identifier(Referential([ rootScope.Id ], [ TextPart name ])))
        | Literal _ ->
            node
        | Set items ->
            Node.like node (Set(List.map (fun e -> recur e scopes) items))
        | List items ->
            Node.like node (List(List.map (fun e -> recur e scopes) items))
        | StringLiteral parts ->
            Node.like node (StringLiteral(List.map (fun p -> markDeclarationsPart rootScope p scopes) parts))
        | Projection (s, e, m) ->
            let receiver = recur e (makeReferential scopes)
            let memberExp = recur m scopes
            attachHost (fst scopes) (DirectPropertyHost receiver) memberExp
            Node.like node (Projection(s, receiver, memberExp))
        | MemberProjection (s, e, m) ->
            let receiver = recur e (makeReferential scopes)
            let memberExp = recur m scopes
            attachHost (fst scopes) (MemberPropertyHost receiver) memberExp
            Node.like node (MemberProjection(s, receiver, memberExp))
        | This (Unbound _) ->
            Node.like node (This(bindingRole scopes []))
        | This _ ->
            node
        | Void ->
            node
        // Analyzed-only nodes: not produced by the parser; pass through untouched.
        | Local _
        | ScopeExp _
        | Unknown
        | Throw _
        | Catch _
        | IsDefined _
        | Equals _
        | ValueEquals _ ->
            node

    and markDeclarationsPart (rootScope: Scope) (part: InterpolatedPart) (scopes: ScopeStack) : InterpolatedPart =
        match part with
        | TextPart text -> TextPart text
        | HolePart exp -> HolePart(markDeclarations rootScope exp scopes)

let markDeclarations rootScope node scopes = Marking.markDeclarations rootScope node scopes
