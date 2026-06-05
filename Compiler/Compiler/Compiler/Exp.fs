module Exp

open System
open System.Collections.Generic

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


type Exp =
    | Local of Scope * Exp
    | Apply of Exp * Exp
    | Referential of Scope * Exp
    | Declarative of Scope * Exp
    | Reference of Exp
    | Publish of Scope * AccessModifier * Exp
    | Tuple of Exp list
    | Record of Scope * Exp list
    | Identifier of InterpolatedPart list
    | BuiltIn of string
    | Literal of Literal
    | Set of Exp list
    | List of Exp list
    | InterpolatedString of InterpolatedPart list
    | Projection of Scope * Exp * Exp
    | MemberProjection of Scope * Exp * Exp
    | This
    | Void
and InterpolatedPart =
    | TextPart of string
    | HolePart of Exp
and Scope = 
    | LocalScope of System.Guid
    | ExpressionScope of Exp
    | ExpressionMemberScope of Exp
    | RootScope

let newScope() = LocalScope(Guid.NewGuid())

type ScopeStack = Scope list * Scope list

let addDeclarative ((d,r):ScopeStack) (s:Scope) : ScopeStack = ScopeStack( s::d, r )
let addReferential((d,r):ScopeStack) (s:Scope) : ScopeStack = ScopeStack( d, s::r )
let makeReferential ((d,r):ScopeStack) : ScopeStack = ScopeStack( [], d @ r )
let isDeclarative ((d,_):ScopeStack) : bool = not d.IsEmpty


let newLocalScope() = Scope.LocalScope(Guid.NewGuid())
let rootScopeStack : ScopeStack = ScopeStack([], [RootScope])

