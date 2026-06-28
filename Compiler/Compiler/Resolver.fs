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

// Map every scope id reachable from the program (plus the root scope) to its Scope.
let private buildRegistry (rootScope: Scope) (exp: CoreExp) : Dictionary<Guid, Scope> =
    let registry = Dictionary<Guid, Scope>()
    registry[rootScope.Id] <- rootScope
    for s in collectAllScopes exp do
        if not (registry.ContainsKey s.Id) then
            registry[s.Id] <- s
    registry

let private resolveRole
    (registry: Dictionary<Guid, Scope>)
    (diags: ResizeArray<Diagnostic>)
    (role: BindingRole)
    : BindingRole =
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
                match registry.TryGetValue sid with
                | true, scope ->
                    match scope with
                    | :? LocalScope ->
                        match lookupInScope scope name with
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
                diags.Add(IllegalShadowing(formatName name))

            match resolved with
            | Some declId -> Resolved(declId, name)
            | None ->
                if not hasDeferredScope then
                    diags.Add(UnresolvedName(formatName name))
                role

let rec private resolveExp
    (registry: Dictionary<Guid, Scope>)
    (diags: ResizeArray<Diagnostic>)
    (exp: CoreExp)
    : CoreExp =
    let go = resolveExp registry diags
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
    | Identifier role -> Identifier(resolveRole registry diags role)
    | ScopeExp (s, e) -> ScopeExp(s, go e)
    | Literal _ -> exp
    | Set xs -> Set(List.map go xs)
    | List xs -> List(List.map go xs)
    | InterpolatedString parts -> InterpolatedString(List.map goPart parts)
    | Projection (s, e, m) -> Projection(s, go e, go m)
    | MemberProjection (s, e, m) -> MemberProjection(s, go e, go m)
    | This role -> This(resolveRole registry diags role)
    | Void -> exp
    | Unknown -> exp
    | Throw e -> Throw(go e)
    | Catch (a, b) -> Catch(go a, go b)
    | IsDefined e -> IsDefined(go e)
    | Equals (a, b) -> Equals(go a, go b)
    | ValueEquals (a, b) -> ValueEquals(go a, go b)

// Resolve referential occurrences against the program's scopes.
// Returns the rewritten tree and any diagnostics collected along the way.
let resolve (rootScope: Scope) (exp: CoreExp) : CoreExp * Diagnostic list =
    let registry = buildRegistry rootScope exp
    let diags = ResizeArray<Diagnostic>()
    let resolved = resolveExp registry diags exp
    resolved, List.ofSeq diags

let formatDiagnostic =
    function
    | UnresolvedName name -> $"unresolved name '{name}'"
    | IllegalShadowing name -> $"illegal shadowing of '{name}' in nested local scope"
