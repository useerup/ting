module Binder

open System
open Exp
open ExpScopes

// Scope-annotation pass over the unified AST. Runs as part of parsing (see
// Parser.parse): it eliminates the parser's binding-intent markers, assigns
// declaration ids to declarative identifiers (recording them in their
// declarative scopes), and attaches the ordered candidate scope-id list to
// every referential identifier.
//
// Operator heads are not its business: the parser knows the operators it parses
// and emits their heads already bound to its own operator scope, so they arrive
// here as ordinary Referential identifiers and pass through untouched.

module private ScopeStackOps =
    let addDeclarative (s: Scope) (scopes: ScopeStack) : ScopeStack =
        { Scope = s; Mode = DeclarativeScope } :: scopes

    let addReferential (s: Scope) (scopes: ScopeStack) : ScopeStack =
        { Scope = s; Mode = ReferentialScope } :: scopes

    let makeReferential (scopes: ScopeStack) : ScopeStack =
        scopes |> List.map (fun f -> { f with Mode = ReferentialScope })

    let referentialCandidateIds (scopes: ScopeStack) : Guid list =
        scopes
        |> List.choose (fun f ->
            match f.Mode with
            | ReferentialScope -> Some(scopeId f.Scope)
            | DeclarativeScope -> None)

    let declarativeScopes (scopes: ScopeStack) : Scope list =
        scopes
        |> List.choose (fun f ->
            match f.Mode with
            | DeclarativeScope -> Some f.Scope
            | ReferentialScope -> None)

    let contiguousDeclarativeScopes (frames: ScopeStack) : Scope list =
        let rec takeAdjacentDeclarative (acc: Scope list) (remaining: ScopeStack) : Scope list =
            match remaining with
            | { Scope = s; Mode = DeclarativeScope } :: rest -> takeAdjacentDeclarative (s :: acc) rest
            | _ -> List.rev acc

        match frames with
        | { Scope = s; Mode = DeclarativeScope } :: rest -> takeAdjacentDeclarative [ s ] rest
        | _ -> []

module private NameLookup =
    let hasHole (parts: InterpolatedParts) =
        parts
        |> List.exists (function
            | HolePart _ -> true
            | TextPart _ -> false)

    // Lexical lookup ignores hosted names; hosted names are resolved only via
    // projection/member-projection in Resolver.
    let tryLookupUnhostedByName (scope: Scope) (name: InterpolatedParts) : Guid option =
        if hasHole name then
            None
        else
            scope.Names
            |> Seq.tryPick (fun kv ->
                if kv.Value.Host.IsNone && kv.Value.NameParts = name then
                    Some kv.Key
                else
                    None)

module private Binding =
    open ScopeStackOps
    open NameLookup

    let private ensureDeclaredInScope (id: Guid) (name: InterpolatedParts) (scope: Scope) : unit =
        match tryLookupUnhostedByName scope name with
        | Some _ -> ()
        | None -> scope.defineName(id, name, Exp.unknown)

    let bindingRole (scopes: ScopeStack) (name: InterpolatedParts) : BindingRole =
        let refs = referentialCandidateIds scopes

        let rec walk (frames: ScopeStack) : BindingRole option =
            match frames with
            | [] -> None
            | frame :: rest ->
                match frame.Mode with
                | ReferentialScope ->
                    match tryLookupUnhostedByName frame.Scope name with
                    | Some _ -> Some(Referential(refs, name))
                    | None -> walk rest
                | DeclarativeScope ->
                    let targets = contiguousDeclarativeScopes frames

                    let declId =
                        targets
                        |> List.tryPick (fun s -> tryLookupUnhostedByName s name)
                        |> Option.defaultValue (Guid.NewGuid())

                    targets |> List.iter (ensureDeclaredInScope declId name)
                    Some(Declarative(declId, name))

        match walk scopes with
        | Some role -> role
        | None -> Referential(refs, name)

module private Marking =
    open ScopeStackOps
    open Binding

    // Collect the declaration ids introduced by a (member) expression so their
    // host can be recorded after binding. Used for `int.Zero = ...` (a single
    // declarative member) and destructuring members like `int.(a, b) = ...`.
    let private collectDeclarativeIds (exp: Exp) : System.Guid list =
        let acc = ResizeArray<System.Guid>()

        let rec walk (n: Exp) =
            match n with
            | Identifier (_, Declarative (id, _)) -> acc.Add id
            | Apply (_, f, a) ->
                walk f
                walk a
            | Tuple (_, xs)
            | Set (_, xs)
            | List (_, xs) -> List.iter walk xs
            | Product (_, positional, _, _, named) ->
                List.iter walk positional
                List.iter walk named
            | Local (_, _, x)
            | Record (_, _, x)
            | Publish (_, _, _, x)
            | ScopeExp (_, _, x)
            | Throw (_, x)
            | Primary (_, x)
            | Candidate (_, x)
            | IsDefined (_, x) -> walk x
            | Projection (_, _, _, m)
            | MemberProjection (_, _, _, m) -> walk m
            | Catch (_, a, b)
            | Equals (_, a, b) ->
                walk a
                walk b
            | StringLiteral (_, parts) ->
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

    // Lowering preserves source spans: an expression produced from a source
    // construct reports that construct's range (the `r` bound by each pattern),
    // and one synthesized to group operands reports `spanning` those operands.
    let rec markDeclarations (node: Exp) (scopes: ScopeStack) : Exp =
        let recur e s = markDeclarations e s

        match node with
        | Local (r, s, e) ->
            Local(r, s, recur e (addDeclarative s scopes))
        | Reference (_, e) ->
            recur e (makeReferential scopes)
        | DeclareLeft (r, f, left, right) ->
            let s = createNamesScope()
            let head = recur f (makeReferential scopes)
            let left = recur left (addDeclarative s scopes)
            let right = recur right (addReferential s scopes)
            let args = Tuple(spanning left right, [ left; right ])
            Local(r, s, Apply(r, head, args))
        | DeclareRight (r, f, left, right) ->
            let s = createNamesScope()
            let head = recur f (makeReferential scopes)
            let left = recur left (addReferential s scopes)
            let right = recur right (addDeclarative s scopes)
            let args = Tuple(spanning left right, [ left; right ])
            Local(r, s, Apply(r, head, args))
        | ReferenceLeft (r, f, left, right) ->
            let refscopes = makeReferential scopes
            let head = recur f refscopes
            let left = recur left refscopes
            let right = recur right scopes
            let args = Tuple(spanning left right, [ left; right ])
            Apply(r, head, args)
        | ReferenceRight (r, f, left, right) ->
            let refscopes = makeReferential scopes
            let head = recur f refscopes
            let left = recur left scopes
            let right = recur right refscopes
            let args = Tuple(spanning left right, [ left; right ])
            Apply(r, head, args)
        | Apply (r, f, a) ->
            Apply(r, recur f scopes, recur a scopes)
        | Publish (r, s, access, e) ->
            Publish(r, s, access, recur e (addDeclarative s scopes))
        | PushScope (_, se, e) ->
            let s = createExpressionScope(recur se (makeReferential scopes))
            recur e (addReferential s scopes)
        | PushMemberScope (_, se, e) ->
            let s = createExpressionMemberScope(recur se (makeReferential scopes))
            recur e (addReferential s scopes)
        | Tuple (r, items) ->
            Tuple(r, List.map (fun e -> recur e scopes) items)
        // A product literal's positional section is ambient, exactly as a
        // tuple's; its named section declares into the product's own scope, as
        // the operand of `record` does.
        | Product (r, positional, fusion, s, named) ->
            let positional = List.map (fun e -> recur e scopes) positional
            let named = List.map (fun e -> recur e (addDeclarative s scopes)) named
            Product(r, positional, fusion, s, named)
        | Record (r, s, e) ->
            Record(r, s, recur e (addDeclarative s scopes))
        | Identifier (r, Unbound parts) ->
            let parts = List.map (fun p -> markDeclarationsPart p (makeReferential scopes)) parts
            Identifier(r, bindingRole scopes parts)
        | Identifier _ ->
            // Already bound: identifiers the parser resolved itself, i.e. the
            // operator heads it emits Referential against its operator scope.
            node
        | Literal _ ->
            node
        | Set (r, items) ->
            Set(r, List.map (fun e -> recur e scopes) items)
        | List (r, items) ->
            List(r, List.map (fun e -> recur e scopes) items)
        | StringLiteral (r, parts) ->
            StringLiteral(r, List.map (fun p -> markDeclarationsPart p scopes) parts)
        | Projection (r, s, e, m) ->
            let receiver = recur e (makeReferential scopes)
            let memberExp = recur m scopes
            attachHost (declarativeScopes scopes) (DirectPropertyHost receiver) memberExp
            Projection(r, s, receiver, memberExp)
        | MemberProjection (r, s, e, m) ->
            let receiver = recur e (makeReferential scopes)
            let memberExp = recur m scopes
            attachHost (declarativeScopes scopes) (MemberPropertyHost receiver) memberExp
            MemberProjection(r, s, receiver, memberExp)
        | This (r, Unbound _) ->
            This(r, bindingRole scopes [])
        | This _ ->
            node
        | Void _ ->
            node
        // Erasure intrinsics. The operand keeps the ambient binding context, so
        // `primary a` declares `a` in a declarative scope exactly as a bare `a`
        // would - the binding is just weaker, constraining `a` only up to
        // attribute erasure.
        | Primary (r, e) ->
            Primary(r, recur e scopes)
        | Candidate (r, e) ->
            Candidate(r, recur e scopes)
        // `throw` is a semantic form rather than a function application, but its
        // operand binds like any other prefix operator's: ambient.
        | Throw (r, e) ->
            Throw(r, recur e scopes)
        // Analyzed-only forms: not produced by the parser; pass through untouched.
        | ScopeExp _
        | Unknown _
        | Catch _
        | IsDefined _
        | Equals _ ->
            node

    and markDeclarationsPart (part: InterpolatedPart) (scopes: ScopeStack) : InterpolatedPart =
        match part with
        | TextPart text -> TextPart text
        | HolePart exp -> HolePart(markDeclarations exp scopes)

let markDeclarations node scopes = Marking.markDeclarations node scopes
