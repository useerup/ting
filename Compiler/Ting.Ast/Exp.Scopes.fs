module ExpScopes

open System
open Exp

let createNamesScope () = LocalScope(Guid.NewGuid()) :> Scope

let createExpressionScope (e: Node) = ExpressionScope(Guid.NewGuid(), e) :> Scope

let createExpressionMemberScope (e: Node) = ExpressionMemberScope(Guid.NewGuid(), e) :> Scope

// Add the same declaration id/name into each declarative scope.
let declareNameInScopes (id: Guid) (name: InterpolatedParts) (declarativeScopes: Scope list) : unit =
    for s in declarativeScopes do
        s.defineName (id, name, Node.unknown)
