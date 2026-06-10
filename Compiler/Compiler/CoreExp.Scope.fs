module CoreExpScopes

open System
open CoreExp

let createNamesScope () = LocalScope(Guid.NewGuid()) :> Scope

let createExpressionScope (e: CoreExp) = ExpressionScope(Guid.NewGuid(), e) :> Scope

let createExpressionMemberScope (e: CoreExp) = ExpressionMemberScope(Guid.NewGuid(), e) :> Scope

let root = RootScope() :> Scope

// Add the same declaration id/name into each declarative scope.
let declareNameInScopes (id: Guid) (name: CoreInterpolatedPart list) (declarativeScopes: Scope list) : unit =
    for s in declarativeScopes do
        s.Names.Add(id, name) |> ignore
