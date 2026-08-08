module Resolver

open System
open System.Collections.Generic
open Exp
open ExpScopeAnalysis

// Name resolution pass.
//
// markDeclarations (Binder, run as part of parsing) produces an analyzed Exp
// where every referential occurrence
// carries the ordered list of candidate scope ids (nearest enclosing scope first,
// root/published scope last). This pass walks that list, finds the nearest scope
// that declares a matching name, and rewrites the occurrence to BindingRole.Resolved
// carrying the concrete declaration id.
//
// It also surfaces two diagnostics derived directly from the design model:
//   - UnresolvedName: a non-interpolated name that no candidate scope declares.
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

// Map every scope id reachable from the program (plus the root and base scopes)
// to its Scope. Base scopes are ambient scopes that no node points at but that
// candidate lists refer to — the parser's operator scope is one.
let private buildRegistry (rootScope: Scope) (baseScopes: Scope list) (exp: Exp) : Dictionary<Guid, Scope> =
    let registry = Dictionary<Guid, Scope>()
    registry[rootScope.Id] <- rootScope
    for s in baseScopes do
        if not (registry.ContainsKey s.Id) then
            registry[s.Id] <- s
    for s in collectAllScopes exp do
        if not (registry.ContainsKey s.Id) then
            registry[s.Id] <- s
    registry

let private nameOfRole (role: BindingRole) : InterpolatedParts =
    match role with
    | Unbound p
    | Declarative (_, p)
    | Referential (_, p)
    | Resolved (_, p) -> p

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
        | Apply (_, op, Tuple (_, [ left; right ])) ->
            if isEqOp op then
                match left with
                | Identifier (_, Declarative (id, _)) ->
                    if not (map.ContainsKey id) then
                        map.Add(id, right)
                | _ -> ()
            walk op
            walk left
            walk right
        | Apply (_, f, a) ->
            walk f
            walk a
        | Local (_, _, e)
        | Record (_, _, e)
        | Publish (_, _, _, e)
        | ScopeExp (_, _, e)
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
        | Apply (_, typeExpr, This (_, Declarative (thisId, _))) ->
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
        | ScopeExp (_, _, e)
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
      BindingMap: Dictionary<Guid, Exp>
      ThisTypes: Dictionary<Guid, Exp>
      Diags: ResizeArray<Diagnostic> }

// Resolve a binding role to a single declaration id, where possible. Used when
// following a receiver (e.g. `person`) to the value it denotes.
let private tryResolveRoleToDecl (ctx: Context) (role: BindingRole) : Guid option =
    match role with
    | Resolved (id, _)
    | Declarative (id, _) -> Some id
    | Referential (cands, name) when not (nameHasHole name) ->
        cands
        |> List.tryPick (fun sid ->
            match ctx.Registry.TryGetValue sid with
            | true, (:? LocalScope as scope) -> lookupInScope scope name
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
let private lookupHostedInScope
    (ctx: Context)
    (scope: Scope)
    (name: InterpolatedParts)
    (receiverIdentity: Guid option)
    (receiverType: Guid option)
    : Guid option =
    scope.Names
    |> Seq.tryPick (fun kvp ->
        let entry = kvp.Value
        if namePartsEqual entry.NameParts name then
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

let rec private resolveRole (ctx: Context) (span: Range.Range) (role: BindingRole) : BindingRole =
    let report kind = ctx.Diags.Add { Kind = kind; Span = span }

    match role with
    | Unbound _
    | Declarative _
    | Resolved _ -> role
    | Referential (candidates, name) ->
        if nameHasHole name then
            // Interpolated names cannot be resolved statically; leave as-is.
            role
        else
            let mutable resolved = None
            // Distinct declaration identities found in non-root local scopes.
            // The same declaration id may intentionally be recorded in several
            // local scopes by layered declarative contexts; that is not shadowing.
            let declaringLocalDeclIds = HashSet<Guid>()
            let mutable hasDeferredScope = false

            for sid in candidates do
                match ctx.Registry.TryGetValue sid with
                | true, scope ->
                    match scope with
                    | :? LocalScope ->
                        match lookupUnhostedInScope scope name with
                        | Some declId ->
                            if resolved.IsNone then
                                resolved <- Some declId
                            if sid <> Guid.Empty then
                                declaringLocalDeclIds.Add declId |> ignore
                        | None -> ()
                    | :? ExpressionScope
                    | :? ExpressionMemberScope ->
                        // Members of an expression scope depend on the expression's set,
                        // which is not computed yet. Defer rather than report unresolved.
                        hasDeferredScope <- true
                    | _ -> ()
                | _ ->
                    // Candidate scope not in the registry; be conservative and defer.
                    hasDeferredScope <- true

            if declaringLocalDeclIds.Count >= 2 then
                report (IllegalShadowing(formatName name))

            match resolved with
            | Some declId -> Resolved(declId, name)
            | None ->
                // Fallback: a bare name may be an implicit member of a `this`
                // parameter whose set is known in scope (e.g. `S this => ...Year...`
                // where `Year` is a field of the record `S`, such as `DateTime`).
                // Resolve it against that set's members.
                let thisHit =
                    candidates
                    |> List.tryPick (fun sid ->
                        match ctx.ThisTypes.TryGetValue sid with
                        | true, typeExpr ->
                            recordScopeOf ctx (HashSet<Guid>()) typeExpr
                            |> Option.bind (fun recScope -> lookupInScope recScope name)
                        | _ -> None)

                match thisHit with
                | Some declId -> Resolved(declId, name)
                | None ->
                    if not hasDeferredScope then
                        report (UnresolvedName(formatName name))
                    role

and private resolveExp (ctx: Context) (node: Exp) : Exp =
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
    | Identifier (r, role) -> Identifier(r, resolveRole ctx r role)
    | ScopeExp (r, s, e) -> ScopeExp(r, s, go e)
    | Literal _ -> node
    | Set (r, xs) -> Set(r, List.map go xs)
    | List (r, xs) -> List(r, List.map go xs)
    | StringLiteral (r, parts) -> StringLiteral(r, List.map goPart parts)
    | Projection (r, s, e, m) ->
        // The member of `receiver.member` is resolved against the record structure
        // of the receiver, not the enclosing lexical scopes.
        let receiver = go e
        Projection(r, s, receiver, resolveMember ctx receiver m)
    | MemberProjection (r, s, e, m) -> MemberProjection(r, s, go e, go m)
    | This (r, role) -> This(r, resolveRole ctx r role)
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

// Resolve `receiver.member`. The member is resolved against (a) the record
// structure of the receiver, then (b) hosted extension properties/members whose
// host matches the receiver. There is no lexical fallback: member names are not
// lexically scoped. If the receiver's structure is unknown, defer silently.
and private resolveMember (ctx: Context) (receiver: Exp) (memberExp: Exp) : Exp =
    match memberExp with
    | Identifier (r, Referential (cands, name)) when not (nameHasHole name) ->
        let recScopeOpt = recordScopeOf ctx (HashSet<Guid>()) receiver
        let fieldHit = recScopeOpt |> Option.bind (fun rs -> lookupInScope rs name)

        let hostedHit =
            match fieldHit with
            | Some _ -> None
            | None ->
                let receiverIdentity = exprIdentity ctx receiver
                let receiverType = receiverTypeIdentity ctx receiver
                cands
                |> List.tryPick (fun sid ->
                    match ctx.Registry.TryGetValue sid with
                    | true, scope -> lookupHostedInScope ctx scope name receiverIdentity receiverType
                    | _ -> None)

        match fieldHit |> Option.orElse hostedHit with
        | Some id -> Identifier(r, Resolved(id, name))
        | None ->
            // A field was sought on a known record but not found: a real error.
            // Otherwise the receiver structure is unknown, so defer.
            match recScopeOpt with
            | Some _ -> ctx.Diags.Add { Kind = UnresolvedMember(formatName name); Span = r }
            | None -> ()

            memberExp
    | _ -> resolveExp ctx memberExp

// Find the record scope denoted by an expression, following identifier bindings
// (e.g. `person` -> `record(Name:string)`) with cycle protection.
and private recordScopeOf (ctx: Context) (visited: HashSet<Guid>) (exp: Exp) : Scope option =
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

// Resolve referential occurrences against the program's scopes.
// `baseScopes` are scopes that referential candidate lists name but that are not
// reachable from the tree — currently the parser's operator scope, which
// declares the functions operators lower to.
// Returns the rewritten tree and any diagnostics collected along the way.
let resolve (rootScope: Scope) (baseScopes: Scope list) (exp: Exp) : Exp * Diagnostic list =
    let registry = buildRegistry rootScope baseScopes exp

    let ctx =
        { Registry = registry
          BindingMap = buildBindingMap exp
          ThisTypes = buildThisTypes registry exp
          Diags = ResizeArray<Diagnostic>() }

    let resolved = resolveExp ctx exp
    resolved, List.ofSeq ctx.Diags

let formatDiagnostic (d: Diagnostic) =
    let message =
        match d.Kind with
        | UnresolvedName name -> $"unresolved name '{name}'"
        | UnresolvedMember name -> $"unresolved member '{name}'"
        | IllegalShadowing name -> $"illegal shadowing of '{name}' in nested local scope"

    $"{Range.Range.format d.Span} {message}"
