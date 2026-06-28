module Binder

open Exp
open CoreExp
open CoreExpScopes

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
    let bindingRole ((decls, refs): ScopeStack) (name: CoreInterpolatedPart list) : BindingRole =
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
    let private collectDeclarativeIds (exp: CoreExp) : System.Guid list =
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
            | InterpolatedString parts ->
                parts
                |> List.iter (function
                    | HolePart e -> walk e
                    | TextPart _ -> ())
            | Identifier _
            | This _
            | Literal _
            | Void
            | Unknown -> ()

        walk exp
        acc |> List.ofSeq

    // Record where a projection-declared name lives: e.g. `int.Zero` hosts `Zero`
    // directly on `int`, and `int..Double` hosts `Double` on the members of `int`.
    let private attachHost (declScopes: Scope list) (host: NameHost) (memberCore: CoreExp) : unit =
        for id in collectDeclarativeIds memberCore do
            for s in declScopes do
                match s.Names.TryGetValue id with
                | true, entry -> s.Names.[id] <- { entry with Host = Some host }
                | _ -> ()

    let rec markDeclarations (rootScope: Scope) (exp: Exp) (scopes: ScopeStack) : CoreExp =
        match exp with
        | Exp.Declare e ->
            let s = createNamesScope()
            CoreExp.Local(s, markDeclarations rootScope e (addDeclarative s scopes))
        | Exp.Reference e ->
            markDeclarations rootScope e (makeReferential scopes)
        | Exp.DeclareLeft (f, left, right) ->
            let s = createNamesScope()
            Local(s, Apply(markDeclarations rootScope f (makeReferential scopes), Tuple [ markDeclarations rootScope left (addDeclarative s scopes); markDeclarations rootScope right (addReferential s scopes) ]))
        | Exp.DeclareRight (f, left, right) ->
            let s = createNamesScope()
            Local(s, Apply(markDeclarations rootScope f (makeReferential scopes), Tuple [ markDeclarations rootScope left (addReferential s scopes); markDeclarations rootScope right (addDeclarative s scopes) ]))
        | Exp.ReferenceLeft (f, left, right) ->
            let refscopes = makeReferential scopes
            Apply(markDeclarations rootScope f refscopes, Tuple [ markDeclarations rootScope left refscopes; markDeclarations rootScope right scopes ])
        | Exp.ReferenceRight (f, left, right) ->
            let refscopes = makeReferential scopes
            Apply(markDeclarations rootScope f refscopes, Tuple [ markDeclarations rootScope left scopes; markDeclarations rootScope right refscopes ])
        | Exp.Apply (f, a) ->
            CoreExp.Apply(markDeclarations rootScope f scopes, markDeclarations rootScope a scopes)
        | Exp.Publish (access, e) ->
            let s = createNamesScope()
            CoreExp.Publish(s, access, markDeclarations rootScope e (addDeclarative s scopes))
        | Exp.PushScope (se, e) ->
            let s = createExpressionScope(markDeclarations rootScope se (makeReferential scopes))
            markDeclarations rootScope e (addReferential s scopes)
        | Exp.PushMemberScope (se, e) ->
            let s = createExpressionMemberScope(markDeclarations rootScope se (makeReferential scopes))
            markDeclarations rootScope e (addReferential s scopes)
        | Exp.Tuple items ->
            CoreExp.Tuple(List.map (fun e -> markDeclarations rootScope e scopes) items)
        | Exp.Record e ->
            let s = createNamesScope() in CoreExp.Record(s, markDeclarations rootScope e (addDeclarative s scopes))
        | Exp.Identifier parts ->
            let parts = List.map (fun p -> markDeclarationsPart rootScope p (makeReferential scopes)) parts
            CoreExp.Identifier(bindingRole scopes parts)
        | Exp.BuiltIn name ->
            CoreExp.Identifier(BindingRole.Referential([ rootScope.Id ], [ CoreInterpolatedPart.TextPart name ]))
        | Exp.Literal lit ->
            CoreExp.Literal lit
        | Exp.Set items ->
            CoreExp.Set(List.map (fun e -> markDeclarations rootScope e scopes) items)
        | Exp.List items ->
            CoreExp.List(List.map (fun e -> markDeclarations rootScope e scopes) items)
        | Exp.InterpolatedString parts ->
            CoreExp.InterpolatedString(List.map (fun p -> markDeclarationsPart rootScope p scopes) parts)
        | Exp.Projection (e, m) ->
            let s = createNamesScope() in CoreExp.Projection(s, markDeclarations rootScope e (makeReferential scopes), markDeclarations rootScope m scopes)
        | Exp.MemberProjection (e, m) ->
            let s = createNamesScope() in CoreExp.MemberProjection(s, markDeclarations rootScope e (makeReferential scopes), markDeclarations rootScope m scopes)
        | Exp.This ->
            CoreExp.This(bindingRole scopes [])
        | Exp.Void ->
            CoreExp.Void

    and markDeclarationsPart (rootScope: Scope) (part: InterpolatedPart) (scopes: ScopeStack) : CoreInterpolatedPart =
        match part with
        | InterpolatedPart.TextPart text -> CoreInterpolatedPart.TextPart text
        | InterpolatedPart.HolePart exp -> CoreInterpolatedPart.HolePart(markDeclarations rootScope exp scopes)

let markDeclarations rootScope exp scopes = Marking.markDeclarations rootScope exp scopes
