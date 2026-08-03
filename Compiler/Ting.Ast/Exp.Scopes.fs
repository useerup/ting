module ExpScopes

open System
open Exp

let createNamesScope () = LocalScope(Guid.NewGuid()) :> Scope

let createExpressionScope (e: Exp) = ExpressionScope(Guid.NewGuid(), e) :> Scope

let createExpressionMemberScope (e: Exp) = ExpressionMemberScope(Guid.NewGuid(), e) :> Scope

// Add the same declaration id/name into each declarative scope.
let declareNameInScopes (id: Guid) (name: InterpolatedParts) (declarativeScopes: Scope list) : unit =
    for s in declarativeScopes do
        s.defineName (id, name, Exp.unknown)
