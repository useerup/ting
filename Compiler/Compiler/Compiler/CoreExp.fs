module CoreExp

open Exp

open System.Collections.Generic

type BindingRole =
    | Declarative of Scope list
    | Referential of Scope list

let bindingRole ((d,r):ScopeStack) : BindingRole =
    if d.IsEmpty then BindingRole.Referential r
    else Declarative d

type CoreInterpolatedPart =
    | TextPart of string
    | HolePart of CoreExp

and CoreExp =
    | Local of Scope * CoreExp 
    | Apply of CoreExp * CoreExp
    | Tuple of CoreExp list
    | Record of Scope * CoreExp list
    | Publish of Scope * AccessModifier * CoreExp
    | Identifier of BindingRole * CoreInterpolatedPart list 
    | Literal of Literal
    | Set of CoreExp list
    | List of CoreExp list
    | InterpolatedString of CoreInterpolatedPart list
    | Projection of Scope * CoreExp * CoreExp
    | MemberProjection of Scope * CoreExp * CoreExp
    | Equals of CoreExp * CoreExp
    | This of BindingRole
    | Void
