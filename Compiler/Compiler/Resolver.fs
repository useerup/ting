module Resolver

open System
open System.Collections.Generic
open CoreExp
open CoreExpScopeAnalysis

// Name resolution pass.
//
// markDeclarations (Binder) produces a CoreExp where every referential occurrence
// carries the ordered list of candidate scope ids (nearest enclosing scope first,
// root/published scope last). This pass walks that list, finds the nearest scope
// that declares a matching name, and rewrites the occurrence to BindingRole.Resolved
// carrying the concrete declaration id.
//
// It also surfaces two diagnostics derived directly from the design model:
//   - UnresolvedName: a non-interpolated name that no candidate scope declares.
//   - IllegalShadowing: the same name is declared in two or more distinct enclosing
//     local scopes (local scopes may shadow published names but not each other).

type Diagnostic =
    | UnresolvedName of string
    | UnresolvedMember of string
    | IllegalShadowing of string

// Compare two name patterns. Only fully textual names participate in static
// resolution; any interpolation hole makes the name dynamic and unresolved here.
let private namePartsEqual (a: CoreInterpolatedPart list) (b: CoreInterpolatedPart list) =
    a.Length = b.Length
    && List.forall2
        (fun x y ->
            match x, y with
            | TextPart s1, TextPart s2 -> s1 = s2
            | _ -> false)
        a
        b

let private nameHasHole (parts: CoreInterpolatedPart list) =
    parts
    |> List.exists (function
        | HolePart _ -> true
        | _ -> false)

let private formatName (parts: CoreInterpolatedPart list) =
    parts
    |> List.map (function
        | TextPart s -> s
        | HolePart _ -> "{...}")
    |> String.concat ""

// The first declaration id in a scope whose name matches.
let private lookupInScope (scope: Scope) (name: CoreInterpolatedPart list) : Guid option =
    scope.Names
    |> Seq.tryPick (fun kvp ->
        if namePartsEqual kvp.Value.NameParts name then
            Some kvp.Key
        else
            None)

// As lookupInScope, but ignores hosted names. Hosted names (extension
// properties/members declared via `int.Zero` / `int..Double`) are only reachable
// through a projection on their host, never as bare lexical names.
let private lookupUnhostedInScope (scope: Scope) (name: CoreInterpolatedPart list) : Guid option =
    scope.Names
    |> Seq.tryPick (fun kvp ->
        if kvp.Value.Host.IsNone && namePartsEqual kvp.Value.NameParts name then
            Some kvp.Key
        else
            None)

// Map every scope id reachable from the program (plus the root scope) to its Scope.
let private buildRegistry (rootScope: Scope) (exp: CoreExp) : Dictionary<Guid, Scope> =
    let registry = Dictionary<Guid, Scope>()
    registry[rootScope.Id] <- rootScope
    for s in collectAllScopes exp do
        if not (registry.ContainsKey s.Id) then
            registry[s.Id] <- s
    registry

let private nameOfRole (role: BindingRole) : CoreInterpolatedPart list =
    match role with
    | Declarative (_, p)
    | Referential (_, p)
    | Resolved (_, p) -> p

// Map a declaration id to the expression it is bound to by `=` (e.g. the record
// value in `let person = record(Name:string)`). Only `_=_` applications whose
// left operand is a declarative identifier introduce a binding; comparison uses
// of `=` (where the left operand is, say, a projection) are ignored. This is the
// minimal structural knowledge needed to resolve member projections.
let private buildBindingMap (exp: CoreExp) : Dictionary<Guid, CoreExp> =
    let map = Dictionary<Guid, CoreExp>()

    let isEqOp e =
        match e with
        | Identifier role -> namePartsEqual (nameOfRole role) [ TextPart "_=_" ]
        | _ -> false

    let rec walk e =
        match e with
        | Apply (op, Tuple [ left; right ]) ->
            if isEqOp op then
                match left with
                | Identifier (Declarative (id, _)) ->
                    if not (map.ContainsKey id) then
                        map.Add(id, right)
                | _ -> ()
            walk op
            walk left
            walk right
        | Apply (f, a) ->
            walk f
            walk a
        | Local (_, e)
        | Record (_, e)
        | Publish (_, _, e)
        | ScopeExp (_, e)
        | Throw e
        | IsDefined e -> walk e
        | Tuple xs
        | Set xs
        | List xs -> List.iter walk xs
        | Projection (_, a, b)
        | MemberProjection (_, a, b)
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
    map

// Shared state threaded through the resolution pass.
type private Context =
    { Registry: Dictionary<Guid, Scope>
      BindingMap: Dictionary<Guid, CoreExp>
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
let rec private exprIdentity (ctx: Context) (exp: CoreExp) : Guid option =
    match exp with
    | Identifier role -> tryResolveRoleToDecl ctx role
    | Local (_, e)
    | Publish (_, _, e)
    | ScopeExp (_, e) -> exprIdentity ctx e
    | _ -> None

// The type a receiver is a member of, for `..` (member-property) host matching.
// Currently known for primitive literals (e.g. `21 : int`).
let private receiverTypeIdentity (ctx: Context) (receiver: CoreExp) : Guid option =
    match receiver with
    | Literal lit ->
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
    (name: CoreInterpolatedPart list)
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

let private resolveRole (ctx: Context) (role: BindingRole) : BindingRole =
    match role with
    | Declarative _
    | Resolved _ -> role
    | Referential (candidates, name) ->
        if nameHasHole name then
            // Interpolated names cannot be resolved statically; leave as-is.
            role
        else
            let mutable resolved = None
            // Distinct non-root local scopes that declare the name (for shadow detection).
            let declaringLocalScopes = HashSet<Guid>()
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
                                declaringLocalScopes.Add sid |> ignore
                        | None -> ()
                    | :? ExpressionScope
                    | :? ExpressionMemberScope ->
                        // Members of an expression scope depend on the expression's type,
                        // which is not computed yet. Defer rather than report unresolved.
                        hasDeferredScope <- true
                    | _ -> ()
                | _ ->
                    // Candidate scope not in the registry; be conservative and defer.
                    hasDeferredScope <- true

            if declaringLocalScopes.Count >= 2 then
                ctx.Diags.Add(IllegalShadowing(formatName name))

            match resolved with
            | Some declId -> Resolved(declId, name)
            | None ->
                if not hasDeferredScope then
                    ctx.Diags.Add(UnresolvedName(formatName name))
                role

let rec private resolveExp (ctx: Context) (exp: CoreExp) : CoreExp =
    let go = resolveExp ctx

    let goPart =
        function
        | TextPart _ as p -> p
        | HolePart e -> HolePart(go e)

    match exp with
    | Local (s, e) -> Local(s, go e)
    | Apply (f, a) -> Apply(go f, go a)
    | Tuple xs -> Tuple(List.map go xs)
    | Record (s, e) -> Record(s, go e)
    | Publish (s, access, e) -> Publish(s, access, go e)
    | Identifier role -> Identifier(resolveRole ctx role)
    | ScopeExp (s, e) -> ScopeExp(s, go e)
    | Literal _ -> exp
    | Set xs -> Set(List.map go xs)
    | List xs -> List(List.map go xs)
    | InterpolatedString parts -> InterpolatedString(List.map goPart parts)
    | Projection (s, e, m) ->
        // The member of `receiver.member` is resolved against the record structure
        // of the receiver, not the enclosing lexical scopes.
        let receiver = go e
        Projection(s, receiver, resolveMember ctx receiver m)
    | MemberProjection (s, e, m) -> MemberProjection(s, go e, go m)
    | This role -> This(resolveRole ctx role)
    | Void -> exp
    | Unknown -> exp
    | Throw e -> Throw(go e)
    | Catch (a, b) -> Catch(go a, go b)
    | IsDefined e -> IsDefined(go e)
    | Equals (a, b) -> Equals(go a, go b)
    | ValueEquals (a, b) -> ValueEquals(go a, go b)

// Resolve `receiver.member`. The member is resolved against (a) the record
// structure of the receiver, then (b) hosted extension properties/members whose
// host matches the receiver. There is no lexical fallback: member names are not
// lexically scoped. If the receiver's structure is unknown, defer silently.
and private resolveMember (ctx: Context) (receiver: CoreExp) (memberExp: CoreExp) : CoreExp =
    match memberExp with
    | Identifier (Referential (cands, name)) when not (nameHasHole name) ->
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
        | Some id -> Identifier(Resolved(id, name))
        | None ->
            // A field was sought on a known record but not found: a real error.
            // Otherwise the receiver structure is unknown, so defer.
            match recScopeOpt with
            | Some _ -> ctx.Diags.Add(UnresolvedMember(formatName name))
            | None -> ()

            memberExp
    | _ -> resolveExp ctx memberExp

// Find the record scope denoted by an expression, following identifier bindings
// (e.g. `person` -> `record(Name:string)`) with cycle protection.
and private recordScopeOf (ctx: Context) (visited: HashSet<Guid>) (exp: CoreExp) : Scope option =
    match exp with
    | Record (recScope, _) -> Some recScope
    | Local (_, e)
    | Publish (_, _, e)
    | ScopeExp (_, e) -> recordScopeOf ctx visited e
    | Identifier role ->
        match tryResolveRoleToDecl ctx role with
        | Some id when visited.Add id ->
            match ctx.BindingMap.TryGetValue id with
            | true, def -> recordScopeOf ctx visited def
            | _ -> None
        | _ -> None
    | _ -> None

// Resolve referential occurrences against the program's scopes.
// Returns the rewritten tree and any diagnostics collected along the way.
let resolve (rootScope: Scope) (exp: CoreExp) : CoreExp * Diagnostic list =
    let ctx =
        { Registry = buildRegistry rootScope exp
          BindingMap = buildBindingMap exp
          Diags = ResizeArray<Diagnostic>() }

    let resolved = resolveExp ctx exp
    resolved, List.ofSeq ctx.Diags

let formatDiagnostic =
    function
    | UnresolvedName name -> $"unresolved name '{name}'"
    | UnresolvedMember name -> $"unresolved member '{name}'"
    | IllegalShadowing name -> $"illegal shadowing of '{name}' in nested local scope"
