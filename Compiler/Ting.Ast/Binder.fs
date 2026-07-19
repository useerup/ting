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
    let private collectDeclarativeIds (exp: Exp) : System.Guid list =
        let acc = ResizeArray<System.Guid>()

        let rec walk e =
            match e with
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
    let private attachHost (declScopes: Scope list) (host: NameHost) (memberExp: Exp) : unit =
        for id in collectDeclarativeIds memberExp do
            for s in declScopes do
                match s.Names.TryGetValue id with
                | true, entry -> s.Names.[id] <- { entry with Host = Some host }
                | _ -> ()

    let rec markDeclarations (rootScope: Scope) (exp: Exp) (scopes: ScopeStack) : Exp =
        match exp with
        | Declare e ->
            let s = createNamesScope()
            Local(s, markDeclarations rootScope e (addDeclarative s scopes))
        | Reference e ->
            markDeclarations rootScope e (makeReferential scopes)
        | DeclareLeft (f, left, right) ->
            let s = createNamesScope()
            Local(s, Apply(markDeclarations rootScope f (makeReferential scopes), Tuple [ markDeclarations rootScope left (addDeclarative s scopes); markDeclarations rootScope right (addReferential s scopes) ]))
        | DeclareRight (f, left, right) ->
            let s = createNamesScope()
            Local(s, Apply(markDeclarations rootScope f (makeReferential scopes), Tuple [ markDeclarations rootScope left (addReferential s scopes); markDeclarations rootScope right (addDeclarative s scopes) ]))
        | ReferenceLeft (f, left, right) ->
            let refscopes = makeReferential scopes
            Apply(markDeclarations rootScope f refscopes, Tuple [ markDeclarations rootScope left refscopes; markDeclarations rootScope right scopes ])
        | ReferenceRight (f, left, right) ->
            let refscopes = makeReferential scopes
            Apply(markDeclarations rootScope f refscopes, Tuple [ markDeclarations rootScope left scopes; markDeclarations rootScope right refscopes ])
        | Apply (f, a) ->
            Apply(markDeclarations rootScope f scopes, markDeclarations rootScope a scopes)
        | Publish (s, access, e) ->
            Publish(s, access, markDeclarations rootScope e (addDeclarative s scopes))
        | PushScope (se, e) ->
            let s = createExpressionScope(markDeclarations rootScope se (makeReferential scopes))
            markDeclarations rootScope e (addReferential s scopes)
        | PushMemberScope (se, e) ->
            let s = createExpressionMemberScope(markDeclarations rootScope se (makeReferential scopes))
            markDeclarations rootScope e (addReferential s scopes)
        | Tuple items ->
            Tuple(List.map (fun e -> markDeclarations rootScope e scopes) items)
        | Record (s, e) ->
            Record(s, markDeclarations rootScope e (addDeclarative s scopes))
        | Identifier (Unbound parts) ->
            let parts = List.map (fun p -> markDeclarationsPart rootScope p (makeReferential scopes)) parts
            Identifier(bindingRole scopes parts)
        | Identifier _ ->
            exp
        | BuiltIn name ->
            Identifier(Referential([ rootScope.Id ], [ TextPart name ]))
        | Literal _ ->
            exp
        | Set items ->
            Set(List.map (fun e -> markDeclarations rootScope e scopes) items)
        | List items ->
            List(List.map (fun e -> markDeclarations rootScope e scopes) items)
        | StringLiteral parts ->
            StringLiteral(List.map (fun p -> markDeclarationsPart rootScope p scopes) parts)
        | Projection (s, e, m) ->
            let receiver = markDeclarations rootScope e (makeReferential scopes)
            let memberExp = markDeclarations rootScope m scopes
            attachHost (fst scopes) (DirectPropertyHost receiver) memberExp
            Projection(s, receiver, memberExp)
        | MemberProjection (s, e, m) ->
            let receiver = markDeclarations rootScope e (makeReferential scopes)
            let memberExp = markDeclarations rootScope m scopes
            attachHost (fst scopes) (MemberPropertyHost receiver) memberExp
            MemberProjection(s, receiver, memberExp)
        | This (Unbound _) ->
            This(bindingRole scopes [])
        | This _ ->
            exp
        | Void ->
            exp
        // Analyzed-only nodes: not produced by the parser; pass through untouched.
        | Local _
        | ScopeExp _
        | Unknown
        | Throw _
        | Catch _
        | IsDefined _
        | Equals _
        | ValueEquals _ ->
            exp

    and markDeclarationsPart (rootScope: Scope) (part: InterpolatedPart) (scopes: ScopeStack) : InterpolatedPart =
        match part with
        | TextPart text -> TextPart text
        | HolePart exp -> HolePart(markDeclarations rootScope exp scopes)

let markDeclarations rootScope exp scopes = Marking.markDeclarations rootScope exp scopes
