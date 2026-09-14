module Resolver

open System
open System.Collections.Generic
open Exp
open ExpScopeAnalysis

// Name classification and resolution pass.
//
// markDeclarations (Binder, run as part of parsing) produces an analyzed Exp
// where identifiers whose scope stack starts declaratively are already Bound
// (Declared), while every other identifier is Pending, carrying the ordered
// moded frame stack (nearest frame first) of the scopes covering it.
//
// This pass finishes the binding walk the Binder deferred. It runs declaration
// sweeps to a fixpoint - a deferred declaration can make another Pending
// occurrence declarable - and only then resolves references, so no reference
// resolves before every reachable declaration has been recorded. A referential
// frame that is an expression scope answers membership from the record
// structure of its defining expression; where that structure is not derivable
// the occurrence stays Pending (deferred, not an error).
//
// It also surfaces two diagnostics derived directly from the design model:
//   - UnresolvedName: a non-interpolated name whose walk misses every frame.
//   - IllegalShadowing: the same name is declared in two or more distinct enclosing
//     local scopes (local scopes may shadow published names but not each other).

type DiagnosticKind =
    | UnresolvedName of string
    | UnresolvedMember of string
    | IllegalShadowing of string

/// A diagnostic together with the source span that provoked it. Every
/// diagnostic carries a span: a message without a location is not actionable,
/// and tooling downstream of the compiler has nothing else to anchor to.
type Diagnostic = {
    Kind: DiagnosticKind
    Span: Range.Range
}

// Compare two name patterns. Only fully textual names participate in static
// resolution; any interpolation hole makes the name dynamic and unresolved here.
let private namePartsEqual (a: InterpolatedParts) (b: InterpolatedParts) =
    a.Length = b.Length
    && List.forall2
        (fun x y ->
            match x, y with
            | TextPart s1, TextPart s2 -> s1 = s2
            | _ -> false)
        a
        b

let private nameHasHole (parts: InterpolatedParts) =
    parts
    |> List.exists (function
        | HolePart _ -> true
        | _ -> false)

let private formatName (parts: InterpolatedParts) =
    parts
    |> List.map (function
        | TextPart s -> s
        | HolePart _ -> "{...}")
    |> String.concat ""

// The first declaration id in a scope whose name matches.
let private lookupInScope (scope: Scope) (name: InterpolatedParts) : Guid option =
    scope.Names
    |> Seq.tryPick (fun kvp ->
        if namePartsEqual kvp.Value.NameParts name then
            Some kvp.Key
        else
            None)

// As lookupInScope, but ignores hosted names. Hosted names (extension
// properties/members declared via `int.Zero` / `int..Double`) are only reachable
// through a projection on their host, never as bare lexical names.
let private lookupUnhostedInScope (scope: Scope) (name: InterpolatedParts) : Guid option =
    scope.Names
    |> Seq.tryPick (fun kvp ->
        if kvp.Value.Host.IsNone && namePartsEqual kvp.Value.NameParts name then
            Some kvp.Key
        else
            None)

// Every scope the program can reach, in a stable order: the root scope, the
// ambient base scopes, then the scopes the tree points at. Base scopes are
// ambient scopes that no node points at but that candidate lists refer to -
// the parser's operator scope is one.
let private buildScopeList (rootScope: Scope) (baseScopes: Scope list) (exp: Exp) : Scope list =
    let seen = HashSet<Guid>()
    (rootScope :: baseScopes) @ collectAllScopes exp
    |> List.filter (fun s -> seen.Add s.Id)

let private buildRegistry (scopes: Scope list) : Dictionary<Guid, Scope> =
    let registry = Dictionary<Guid, Scope>()
    for s in scopes do
        registry[s.Id] <- s
    registry

let private nameOfRole (role: BindingRole) : InterpolatedParts =
    match role with
    | Unbound p
    | Pending (_, p)
    | Bound (_, _, p) -> p

// Map a declaration id to the expression it is bound to by `=` (e.g. the record
// value in `let person = record(Name:string)`). Only `_=_` applications whose
// left operand is a declarative identifier introduce a binding; comparison uses
// of `=` (where the left operand is, say, a projection) are ignored. This is the
// minimal structural knowledge needed to resolve member projections.
let private buildBindingMap (exp: Exp) : Dictionary<Guid, Exp> =
    let map = Dictionary<Guid, Exp>()

    let isEqOp (e: Exp) =
        match e with
        | Identifier (_, role) -> namePartsEqual (nameOfRole role) [ TextPart "_=_" ]
        | _ -> false

    let rec walk (n: Exp) =
        match n with
        | ScopeExp (_, scope, body) ->
            scopeExpression scope |> Option.iter walk
            walk body
        | Apply (_, op, Tuple (_, [ left; right ])) ->
            if isEqOp op then
                constrain left right
                constrain right left
            walk op
            walk left
            walk right
        | Apply (_, f, a) ->
            walk f
            walk a
        | Local (_, _, e)
        | Record (_, _, e)
        | Publish (_, _, _, e)
        | Throw (_, e)
        | Primary (_, e)
        | Candidate (_, e)
        | IsDefined (_, e) -> walk e
        | Tuple (_, xs)
        | Set (_, xs)
        | List (_, xs) -> List.iter walk xs
        | Product (_, positional, _, _, named) ->
            List.iter walk positional
            List.iter walk named
        | Projection (_, _, a, b)
        | MemberProjection (_, _, a, b)
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
    map

// Map the scope that introduces a `this` parameter to the parameter's set
// expression. A parameter written as `S this` appears as `Apply(S, This id)`;
// the members of the set `S` (e.g. the fields of the record `S` denotes) are then
// implicitly in scope in the lambda body as members of `this`.
// (Ting has no types of its own; `S` denotes a set, e.g. `DateTime`.)
let private buildThisTypes (registry: Dictionary<Guid, Scope>) (exp: Exp) : Dictionary<Guid, Exp> =
    // declaration id -> id of the scope that declares it.
    let declToScope = Dictionary<Guid, Guid>()

    for kvp in registry do
        let scope = kvp.Value
        for n in scope.Names do
            if not (declToScope.ContainsKey n.Key) then
                declToScope[n.Key] <- scope.Id

    let map = Dictionary<Guid, Exp>()

    let rec walk (n: Exp) =
        match n with
        | ScopeExp (_, scope, body) ->
            scopeExpression scope |> Option.iter walk
            walk body
        | Apply (_, typeExpr, This (_, Bound (Declared, thisId, _))) ->
            match declToScope.TryGetValue thisId with
            | true, sid when not (map.ContainsKey sid) -> map[sid] <- typeExpr
            | _ -> ()
            walk typeExpr
        | Apply (_, f, a) ->
            walk f
            walk a
        | Local (_, _, e)
        | Record (_, _, e)
        | Publish (_, _, _, e)
        | Throw (_, e)
        | Primary (_, e)
        | Candidate (_, e)
        | IsDefined (_, e) -> walk e
        | Tuple (_, xs)
        | Set (_, xs)
        | List (_, xs) -> List.iter walk xs
        | Product (_, positional, _, _, named) ->
            List.iter walk positional
            List.iter walk named
        | Projection (_, _, a, b)
        | MemberProjection (_, _, a, b)
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
    map

// Shared state threaded through the resolution pass.
type private Context =
    { Registry: Dictionary<Guid, Scope>
      Scopes: Scope list
      BindingMap: Dictionary<Guid, Exp>
      ThisTypes: Dictionary<Guid, Exp>
      Diags: ResizeArray<Diagnostic> }

// Resolve a binding role to a single declaration id, where possible. Used when
// following a receiver (e.g. `person`) to the value it denotes.
let private tryResolveRoleToDecl (ctx: Context) (role: BindingRole) : Guid option =
    match role with
    | Bound (_, id, _) -> Some id
    | Pending (frames, name) when not (nameHasHole name) ->
        frames
        |> List.tryPick (fun f ->
            match f.Mode, f.Scope with
            | ReferentialScope, (:? LocalScope as scope) -> lookupInScope scope name
            | _ -> None)
    | _ -> None

// The declaration id an expression denotes, for host matching. Follows the
// identifier of the host/receiver (e.g. `int` -> int's declaration id).
let rec private exprIdentity (ctx: Context) (exp: Exp) : Guid option =
    match exp with
    | Identifier (_, role) -> tryResolveRoleToDecl ctx role
    | Local (_, _, e)
    | Publish (_, _, _, e)
    | ScopeExp (_, _, e) -> exprIdentity ctx e
    | _ -> None

// The set a receiver is a member of, for `..` (member-property) host matching.
// Currently known for primitive literals (e.g. `21` is a member of `int`).
let private receiverTypeIdentity (ctx: Context) (receiver: Exp) : Guid option =
    match receiver with
    | Literal (_, lit) ->
        let typeName =
            match lit with
            | Exp.Int32 _ -> Some "int"
            | Exp.Boolean _ -> Some "bool"
            | Exp.Float32 _ -> Some "float"
            | Exp.Float64 _ -> Some "double"
            | Exp.Decimal _ -> Some "decimal"
            | Exp.String _ -> Some "string"
        typeName
        |> Option.bind (fun n ->
            match ctx.Registry.TryGetValue Guid.Empty with
            | true, root -> lookupInScope root [ TextPart n ]
            | _ -> None)
    | _ -> None

// Find a hosted name in a scope whose host matches the receiver:
//   DirectPropertyHost h  matches when the receiver *is* h        (e.g. int.Zero)
//   MemberPropertyHost h  matches when the receiver is a member of h (e.g. 21.Double)
// `excluded` is the occurrence's own declaration, which a projection always
// contributes to its own scope and which must therefore never match itself.
let private lookupHostedInScope
    (ctx: Context)
    (scope: Scope)
    (name: InterpolatedParts)
    (receiverIdentity: Guid option)
    (receiverType: Guid option)
    (excluded: Guid)
    : Guid option =
    scope.Names
    |> Seq.tryPick (fun kvp ->
        let entry = kvp.Value
        if kvp.Key <> excluded && namePartsEqual entry.NameParts name then
            match entry.Host with
            | Some (DirectPropertyHost h) ->
                match exprIdentity ctx h, receiverIdentity with
                | Some hostId, Some rid when hostId = rid -> Some kvp.Key
                | _ -> None
            | Some (MemberPropertyHost h) ->
                match exprIdentity ctx h, receiverType with
                | Some hostId, Some rt when hostId = rt -> Some kvp.Key
                | _ -> None
            | None -> None
        else
            None)

// Find the record scope denoted by an expression, following identifier bindings
// (e.g. `person` -> `record(Name:string)`) with cycle protection.
let rec private recordScopeOf (ctx: Context) (visited: HashSet<Guid>) (exp: Exp) : Scope option =
    match exp with
    | Record (_, recScope, _) -> Some recScope
    // A product literal's named section carries its field names, so a product
    // with named fields answers member lookups exactly as a record does.
    | Product (_, _, _, productScope, _) -> Some productScope
    | Set (_, [ single ]) -> recordScopeOf ctx visited single
    | Local (_, _, e)
    | Publish (_, _, _, e)
    | ScopeExp (_, _, e) -> recordScopeOf ctx visited e
    | Identifier (_, role) ->
        match tryResolveRoleToDecl ctx role with
        | Some id when visited.Add id ->
            match ctx.BindingMap.TryGetValue id with
            | true, def -> recordScopeOf ctx visited def
            | _ -> None
        | _ -> None
    | _ -> None

// How a referential frame answers "does this scope provide the name?".
type private Membership =
    | Contains of Guid
    | Lacks
    | UnknownMembers

let private frameMembership (ctx: Context) (frame: ScopeFrame) (name: InterpolatedParts) : Membership =
    match frame.Scope with
    | :? LocalScope as scope ->
        match lookupUnhostedInScope scope name with
        | Some id -> Contains id
        | None -> Lacks
    | :? ExpressionScope as scope ->
        // The names an expression scope provides are the members of the set its
        // defining expression denotes.
        match recordScopeOf ctx (HashSet<Guid>()) scope.Expression with
        | Some recScope ->
            match lookupInScope recScope name with
            | Some id -> Contains id
            | None -> Lacks
        | None -> UnknownMembers
    | _ ->
        // ExpressionMemberScope: member-of-member structure is not derivable yet.
        UnknownMembers

// What the deferred binding walk would do for a Pending occurrence.
type private WalkOutcome =
    | WouldReference of Guid
    | WouldDeclare of Scope list
    | Undecidable
    | Missing

let private walkPending (ctx: Context) (frames: ScopeFrame list) (name: InterpolatedParts) : WalkOutcome =
    let rec go (fs: ScopeFrame list) =
        match fs with
        | [] -> Missing
        | f :: rest ->
            match f.Mode with
            | DeclarativeScope ->
                // Contiguous declarative segment; the first referential frame is
                // a barrier per the declaration composition rules.
                let targets =
                    fs
                    |> List.takeWhile (fun fr -> fr.Mode = DeclarativeScope)
                    |> List.map (fun fr -> fr.Scope)
                WouldDeclare targets
            | ReferentialScope ->
                match frameMembership ctx f name with
                | Contains id -> WouldReference id
                | Lacks -> go rest
                | UnknownMembers -> Undecidable
    go frames

let private applyDeclare (targets: Scope list) (name: InterpolatedParts) : Guid =
    let declId =
        targets
        |> List.tryPick (fun s -> lookupUnhostedInScope s name)
        |> Option.defaultValue (Guid.NewGuid())
    for s in targets do
        if (lookupUnhostedInScope s name).IsNone then
            s.defineName(declId, name, Exp.unknown)
    declId

// Distinct declaration identities for the name across the non-root local
// referential frames. Two or more means nested local scopes shadow each other.
let private shadowingIds (frames: ScopeFrame list) (name: InterpolatedParts) : HashSet<Guid> =
    let ids = HashSet<Guid>()
    for f in frames do
        if f.Mode = ReferentialScope then
            match f.Scope with
            | :? LocalScope as s when s.Id <> Guid.Empty ->
                match lookupUnhostedInScope s name with
                | Some id -> ids.Add id |> ignore
                | None -> ()
            | _ -> ()
    ids

// Fallback: a bare name may be an implicit member of a `this` parameter whose
// set is known in scope (e.g. `S this => ...Year...` where `Year` is a field of
// the record `S`).
let private tryThisMember (ctx: Context) (frames: ScopeFrame list) (name: InterpolatedParts) : Guid option =
    frames
    |> List.tryPick (fun f ->
        match f.Mode with
        | ReferentialScope ->
            match ctx.ThisTypes.TryGetValue f.Scope.Id with
            | true, typeExpr ->
                recordScopeOf ctx (HashSet<Guid>()) typeExpr
                |> Option.bind (fun recScope -> lookupInScope recScope name)
            | _ -> None
        | _ -> None)

// One classification sweep. Declarations are always applied; references only
// when `final` is true, so no reference resolves before every reachable
// declaration has been recorded. Scope mutation during the sweep is
// intentional: later occurrences in the same sweep see earlier declarations.
// Returns the rewritten tree and the number of declarations applied.
let private classifyPass (ctx: Context) (final: bool) (exp: Exp) : Exp * int =
    let mutable applied = 0

    let classifyRole (span: Range.Range) (role: BindingRole) : BindingRole =
        match role with
        | Unbound _
        | Bound _ -> role
        | Pending (frames, name) ->
            match walkPending ctx frames name with
            | WouldDeclare targets ->
                applied <- applied + 1
                Bound(Declared, applyDeclare targets name, name)
            | WouldReference id when final ->
                if (shadowingIds frames name).Count >= 2 then
                    ctx.Diags.Add { Kind = IllegalShadowing(formatName name); Span = span }
                Bound(Referenced, id, name)
            | Missing when final ->
                match tryThisMember ctx frames name with
                | Some id -> Bound(Referenced, id, name)
                | None ->
                    if not (nameHasHole name) then
                        ctx.Diags.Add { Kind = UnresolvedName(formatName name); Span = span }
                    role
            | _ -> role

    let rec go (node: Exp) : Exp =
        let goPart =
            function
            | TextPart _ as p -> p
            | HolePart e -> HolePart(go e)

        match node with
        | Local (r, s, e) -> Local(r, s, go e)
        | Apply (r, f, a) -> Apply(r, go f, go a)
        | Tuple (r, xs) -> Tuple(r, List.map go xs)
        | Product (r, positional, fusion, s, named) ->
            Product(r, List.map go positional, fusion, s, List.map go named)
        | Record (r, s, e) -> Record(r, s, go e)
        | Publish (r, s, access, e) -> Publish(r, s, access, go e)
        | Identifier (r, role) -> Identifier(r, classifyRole r role)
        | ScopeExp (r, s, e) ->
            rewriteScopeExpression go s
            ScopeExp(r, s, go e)
        | Literal _ -> node
        | Set (r, xs) -> Set(r, List.map go xs)
        | List (r, xs) -> List(r, List.map go xs)
        | StringLiteral (r, parts) -> StringLiteral(r, List.map goPart parts)
        | Projection (r, s, e, m) ->
            // The member of `receiver.member` resolves against the receiver's
            // structure, not lexically; leave identifier members for
            // resolveMember.
            let m' =
                match m with
                | Identifier _ -> m
                | _ -> go m
            Projection(r, s, go e, m')
        | MemberProjection (r, s, e, m) ->
            let m' =
                match m with
                | Identifier _ -> m
                | _ -> go m
            MemberProjection(r, s, go e, m')
        | This (r, role) -> This(r, classifyRole r role)
        | Void _ -> node
        | Unknown _ -> node
        | Throw (r, e) -> Throw(r, go e)
        | Catch (r, a, b) -> Catch(r, go a, go b)
        | IsDefined (r, e) -> IsDefined(r, go e)
        | Equals (r, a, b) -> Equals(r, go a, go b)
        | Primary (r, e) -> Primary(r, go e)
        | Candidate (r, e) -> Candidate(r, go e)
        // Surface markers: eliminated by the Binder before resolution.
        | Reference _
        | DeclareLeft _
        | DeclareRight _
        | ReferenceLeft _
        | ReferenceRight _
        | PushScope _
        | PushMemberScope _ -> node

    let rewritten = go exp
    rewritten, applied

let rec private resolveExp (ctx: Context) (node: Exp) : Exp =
    let go = resolveExp ctx
    // Resolution only rewrites binding roles, so every rebuilt expression keeps
    // the range of the one it replaced.

    let goPart =
        function
        | TextPart _ as p -> p
        | HolePart e -> HolePart(go e)

    match node with
    | Local (r, s, e) -> Local(r, s, go e)
    | Apply (r, f, a) -> Apply(r, go f, go a)
    | Tuple (r, xs) -> Tuple(r, List.map go xs)
    | Product (r, positional, fusion, s, named) ->
        Product(r, List.map go positional, fusion, s, List.map go named)
    | Record (r, s, e) -> Record(r, s, go e)
    | Publish (r, s, access, e) -> Publish(r, s, access, go e)
    | Identifier _ -> node
    | ScopeExp (r, s, e) ->
        rewriteScopeExpression go s
        ScopeExp(r, s, go e)
    | Literal _ -> node
    | Set (r, xs) -> Set(r, List.map go xs)
    | List (r, xs) -> List(r, List.map go xs)
    | StringLiteral (r, parts) -> StringLiteral(r, List.map goPart parts)
    | Projection (r, s, e, m) ->
        // The member of `receiver.member` is resolved against the record structure
        // of the receiver, not the enclosing lexical scopes.
        let receiver = go e
        Projection(r, s, receiver, resolveMember ctx s false receiver m)
    | MemberProjection (r, s, e, m) ->
        let receiver = go e
        MemberProjection(r, s, receiver, resolveMember ctx s true receiver m)
    | This _ -> node
    | Void _ -> node
    | Unknown _ -> node
    | Throw (r, e) -> Throw(r, go e)
    | Catch (r, a, b) -> Catch(r, go a, go b)
    | IsDefined (r, e) -> IsDefined(r, go e)
    | Equals (r, a, b) -> Equals(r, go a, go b)
    | Primary (r, e) -> Primary(r, go e)
    | Candidate (r, e) -> Candidate(r, go e)
    // Surface markers: eliminated by the Binder before resolution.
    | Reference _
    | DeclareLeft _
    | DeclareRight _
    | ReferenceLeft _
    | ReferenceRight _
    | PushScope _
    | PushMemberScope _ -> node

// Resolve `receiver.member` (or `receiver..member` when `overMembers`). A
// projection declares its member into its own scope, so every member arrives
// here already declared. A member that was *also* contributed to a scope
// outside the projection stood in a declarative context: it introduces a hosted
// extension property and is left alone. Otherwise it is a read, resolved
// against (a) the record structure of the receiver, then (b) hosted names whose
// host matches the receiver. There is no lexical fallback: member names are not
// lexically scoped. If the receiver's structure is unknown, defer silently.
and private resolveMember (ctx: Context) (projectionScope: Scope) (overMembers: bool) (receiver: Exp) (memberExp: Exp) : Exp =
    match memberExp with
    | Tuple (r, xs) -> Tuple(r, xs |> List.map (resolveMember ctx projectionScope overMembers receiver))
    | Identifier (r, Bound (Declared, declId, name)) when not (nameHasHole name) ->
        let declaresExtension =
            ctx.Scopes
            |> List.exists (fun s -> s.Id <> projectionScope.Id && s.Names.ContainsKey declId)

        if declaresExtension then
            memberExp
        else
            // `..` selects from the members of the receiver, whose structure is
            // not derivable, so only the hosted path applies there.
            let recScopeOpt =
                if overMembers then None else recordScopeOf ctx (HashSet<Guid>()) receiver

            let fieldHit = recScopeOpt |> Option.bind (fun rs -> lookupInScope rs name)

            let hostedHit =
                match fieldHit with
                | Some _ -> None
                | None ->
                    let receiverIdentity, receiverType =
                        if overMembers then
                            None, exprIdentity ctx receiver
                        else
                            exprIdentity ctx receiver, receiverTypeIdentity ctx receiver

                    ctx.Scopes
                    |> List.tryPick (fun s ->
                        lookupHostedInScope ctx s name receiverIdentity receiverType declId)

            match fieldHit |> Option.orElse hostedHit with
            | Some id ->
                // The member only looked like a declaration until the receiver
                // supplied the name, so retract it.
                projectionScope.Names.Remove declId |> ignore
                Identifier(r, Bound(Referenced, id, name))
            | None ->
                // A field was sought on a known record but not found: a real error.
                // Otherwise the receiver structure is unknown, so defer.
                match recScopeOpt with
                | Some _ -> ctx.Diags.Add { Kind = UnresolvedMember(formatName name); Span = r }
                | None -> ()

                memberExp
    | _ -> resolveExp ctx memberExp

// Classify the deferred binding walks, then resolve member projections.
// `baseScopes` are scopes that Pending frames may name but that are not
// reachable from the tree — currently the parser's operator scope.
// Returns the rewritten tree and any diagnostics collected along the way.
let resolve (rootScope: Scope) (baseScopes: Scope list) (exp: Exp) : Exp * Diagnostic list =
    let diags = ResizeArray<Diagnostic>()

    // The binding map and this-types change as deferred declarations land, so
    // rebuild the context between sweeps. The registry maps ids to the same
    // mutable scope objects throughout.
    let mkCtx (e: Exp) =
        let scopes = buildScopeList rootScope baseScopes e
        let registry = buildRegistry scopes
        { Registry = registry
          Scopes = scopes
          BindingMap = buildBindingMap e
          ThisTypes = buildThisTypes registry e
          Diags = diags }

    // Declaration sweeps to a fixpoint: a deferred declaration can enable
    // another (or change a later reference), so repeat until none lands.
    let rec settle (e: Exp) =
        let e', applied = classifyPass (mkCtx e) false e
        if applied > 0 then settle e' else e'

    let settled = settle exp
    let classified, _ = classifyPass (mkCtx settled) true settled
    let resolved = resolveExp (mkCtx classified) classified
    resolved, List.ofSeq diags

let formatDiagnostic (d: Diagnostic) =
    let message =
        match d.Kind with
        | UnresolvedName name -> $"unresolved name '{name}'"
        | UnresolvedMember name -> $"unresolved member '{name}'"
        | IllegalShadowing name -> $"illegal shadowing of '{name}' in nested local scope"

    $"{Range.Range.format d.Span} {message}"
