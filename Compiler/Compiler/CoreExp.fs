module CoreExp

open System
open Exp

// Interpolated fragments used in identifiers and strings after analysis.
type CoreInterpolatedPart =
    | TextPart of string
    | HolePart of CoreExp

// Name table for a scope: declaration id -> analyzed name parts.
and Names = System.Collections.Generic.Dictionary<Guid, CoreInterpolatedPart list>

// Core, scope-annotated expression tree produced by Analyzer.markDeclarations.
and CoreExp =
    | Local of Scope * CoreExp
    | Apply of CoreExp * CoreExp
    | Tuple of CoreExp list
    | Record of Scope * CoreExp
    | Publish of Scope * Exp.AccessModifier * CoreExp
    | Identifier of BindingRole
    | ScopeExp of Scope * CoreExp
    | Literal of Exp.Literal
    | Set of CoreExp list
    | List of CoreExp list
    | InterpolatedString of CoreInterpolatedPart list
    | Projection of Scope * CoreExp * CoreExp
    | MemberProjection of Scope * CoreExp * CoreExp
    | StructuralEquals of CoreExp * CoreExp
    | IdentityEquals of CoreExp * CoreExp
    | This of BindingRole
    | Void

// Scope hierarchy.
and [<AbstractClass>] Scope(id: Guid, names: Names) =
    member _.Id = id
    member _.Names = names

and RootScope() =
    inherit Scope(Guid.Empty, new Names())

and [<AbstractClass>] ChildScope(id: Guid) =
    inherit Scope(id, new Names())

and LocalScope(id: Guid) =
    inherit ChildScope(id)

and ExpressionScope(id: Guid, expression: CoreExp) =
    inherit ChildScope(id)
    member _.Expression = expression

and ExpressionMemberScope(id: Guid, expression: CoreExp) =
    inherit ChildScope(id)
    member _.Expression = expression

// Declarative carries declaration identity; referential carries candidate scope ids + name pattern.
and BindingRole =
    | Declarative of Guid * CoreInterpolatedPart list
    | Referential of Guid list * CoreInterpolatedPart list

type ScopeStack = Scope list * Scope list

let scopeId (s: Scope) = s.Id

