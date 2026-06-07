module Exp

open System

type Literal =
    | Int32 of int
    | Boolean of bool
    | Float32 of float
    | Float64 of double
    | Decimal of decimal
    | String of string

type AccessModifier =
    | Public
    | Private
    | Internal
    | Protected

and Exp =
    | Declare of Exp
    | Reference of Exp
    | DeclareLeft of Exp * Exp * Exp
    | DeclareRight of Exp * Exp * Exp
    | ReferenceLeft of Exp * Exp * Exp
    | ReferenceRight of Exp * Exp * Exp
    | Publish of AccessModifier * Exp
    | Apply of Exp * Exp
    | Tuple of Exp list
    | Record of Exp
    | Identifier of InterpolatedPart list
    | BuiltIn of string
    | Literal of Literal
    | Set of Exp list
    | List of Exp list
    | InterpolatedString of InterpolatedPart list
    | PushScope of Exp * Exp
    | PushMemberScope of Exp * Exp
    | Projection of Exp * Exp
    | MemberProjection of Exp * Exp
    | This
    | Void
and InterpolatedPart =
    | TextPart of string
    | HolePart of Exp


