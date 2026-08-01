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
///
/// The field is `Span` rather than `Range` for the same reason as `ParseError`'s:
/// `Node.Range` must stay the only `Range` field in scope, or F# will infer this
/// record wherever a `.Range` is read without annotation.
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
let private buildRegistry (rootScope: Scope) (baseScopes: Scope list) (exp: Node) : Dictionary<Guid, Scope> =
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
let private buildBindingMap (exp: Node) : Dictionary<Guid, Node> =
    let map = Dictionary<Guid, Node>()

    let isEqOp (e: Node) =
        match e.Exp with
        | Identifier role -> namePartsEqual (nameOfRole role) [ TextPart "_=_" ]
        | _ -> false

    let rec walk (n: Node) =
        match n.Exp with
        | Apply (op, E (Tuple [ left; right ])) ->
            if isEqOp op then
                match left.Exp with
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
        | Primary e
        | Candidate e
        | IsDefined e -> walk e
        | Tuple xs
        | Set xs
        | List xs -> List.iter walk xs
        | Product (positional, _, _, named) ->
            List.iter walk positional
            List.iter walk named
        | Projection (_, a, b)
        | MemberProjection (_, a, b)
        | Catch (a, b)
        | Equals (a, b) ->
            walk a
            walk b
        | StringLiteral parts ->
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
let private buildThisTypes (registry: Dictionary<Guid, Scope>) (exp: Node) : Dictionary<Guid, Node> =
    // declaration id -> id of the scope that declares it.
    let declToScope = Dictionary<Guid, Guid>()

    for kvp in registry do
        let scope = kvp.Value
        for n in scope.Names do
            if not (declToScope.ContainsKey n.Key) then
                declToScope[n.Key] <- scope.Id

    let map = Dictionary<Guid, Node>()

    let rec walk (n: Node) =
        match n.Exp with
        | Apply (typeExpr, E (This (Declarative (thisId, _)))) ->
            match declToScope.TryGetValue thisId with
            | true, sid when not (map.ContainsKey sid) -> map[sid] <- typeExpr
            | _ -> ()
            walk typeExpr
        | Apply (f, a) ->
            walk f
            walk a
        | Local (_, e)
        | Record (_, e)
        | Publish (_, _, e)
        | ScopeExp (_, e)
        | Throw e
        | Primary e
        | Candidate e
        | IsDefined e -> walk e
        | Tuple xs
        | Set xs
        | List xs -> List.iter walk xs
        | Product (positional, _, _, named) ->
            List.iter walk positional
            List.iter walk named
        | Projection (_, a, b)
        | MemberProjection (_, a, b)
        | Catch (a, b)
        | Equals (a, b) ->
            walk a
            walk b
        | StringLiteral parts ->
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
      BindingMap: Dictionary<Guid, Node>
      ThisTypes: Dictionary<Guid, Node>
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
let rec private exprIdentity (ctx: Context) (exp: Node) : Guid option =
    match exp.Exp with
    | Identifier role -> tryResolveRoleToDecl ctx role
    | Local (_, e)
    | Publish (_, _, e)
    | ScopeExp (_, e) -> exprIdentity ctx e
    | _ -> None

// The set a receiver is a member of, for `..` (member-property) host matching.
// Currently known for primitive literals (e.g. `21` is a member of `int`).
let private receiverTypeIdentity (ctx: Context) (receiver: Node) : Guid option =
    match receiver.Exp with
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
                        // Members of an expression scope depend on the expression's set,
                        // which is not computed yet. Defer rather than report unresolved.
                        hasDeferredScope <- true
                    | _ -> ()
                | _ ->
                    // Candidate scope not in the registry; be conservative and defer.
                    hasDeferredScope <- true

            if declaringLocalScopes.Count >= 2 then
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

and private resolveExp (ctx: Context) (node: Node) : Node =
    let go = resolveExp ctx
    // Resolution only rewrites binding roles, so every rebuilt node keeps the
    // range of the node it replaced.
    let rebuild e = Node.like node e

    let goPart =
        function
        | TextPart _ as p -> p
        | HolePart e -> HolePart(go e)

    match node.Exp with
    | Local (s, e) -> rebuild (Local(s, go e))
    | Apply (f, a) -> rebuild (Apply(go f, go a))
    | Tuple xs -> rebuild (Tuple(List.map go xs))
    | Product (positional, fusion, s, named) ->
        rebuild (Product(List.map go positional, fusion, s, List.map go named))
    | Record (s, e) -> rebuild (Record(s, go e))
    | Publish (s, access, e) -> rebuild (Publish(s, access, go e))
    | Identifier role -> rebuild (Identifier(resolveRole ctx node.Range role))
    | ScopeExp (s, e) -> rebuild (ScopeExp(s, go e))
    | Literal _ -> node
    | Set xs -> rebuild (Set(List.map go xs))
    | List xs -> rebuild (List(List.map go xs))
    | StringLiteral parts -> rebuild (StringLiteral(List.map goPart parts))
    | Projection (s, e, m) ->
        // The member of `receiver.member` is resolved against the record structure
        // of the receiver, not the enclosing lexical scopes.
        let receiver = go e
        rebuild (Projection(s, receiver, resolveMember ctx receiver m))
    | MemberProjection (s, e, m) -> rebuild (MemberProjection(s, go e, go m))
    | This role -> rebuild (This(resolveRole ctx node.Range role))
    | Void -> node
    | Unknown -> node
    | Throw e -> rebuild (Throw(go e))
    | Catch (a, b) -> rebuild (Catch(go a, go b))
    | IsDefined e -> rebuild (IsDefined(go e))
    | Equals (a, b) -> rebuild (Equals(go a, go b))
    | Primary e -> rebuild (Primary(go e))
    | Candidate e -> rebuild (Candidate(go e))
    // Surface marker nodes: eliminated by the Binder before resolution.
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
and private resolveMember (ctx: Context) (receiver: Node) (memberExp: Node) : Node =
    match memberExp.Exp with
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
        | Some id -> Node.like memberExp (Identifier(Resolved(id, name)))
        | None ->
            // A field was sought on a known record but not found: a real error.
            // Otherwise the receiver structure is unknown, so defer.
            match recScopeOpt with
            | Some _ -> ctx.Diags.Add { Kind = UnresolvedMember(formatName name); Span = memberExp.Range }
            | None -> ()

            memberExp
    | _ -> resolveExp ctx memberExp

// Find the record scope denoted by an expression, following identifier bindings
// (e.g. `person` -> `record(Name:string)`) with cycle protection.
and private recordScopeOf (ctx: Context) (visited: HashSet<Guid>) (exp: Node) : Scope option =
    match exp.Exp with
    | Record (recScope, _) -> Some recScope
    // A product literal's named section carries its field names, so a product
    // with named fields answers member lookups exactly as a record does.
    | Product (_, _, productScope, _) -> Some productScope
    | Set [ single ] -> recordScopeOf ctx visited single
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
// `baseScopes` are scopes that referential candidate lists name but that are not
// reachable from the tree — currently the parser's operator scope, which
// declares the functions operators lower to.
// Returns the rewritten tree and any diagnostics collected along the way.
let resolve (rootScope: Scope) (baseScopes: Scope list) (exp: Node) : Node * Diagnostic list =
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
