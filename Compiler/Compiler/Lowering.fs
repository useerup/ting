module Lowering

open Exp
open Core

// Surface tree -> core expression language. Runs after name resolution, so the
// binding roles it copies across are the resolved ones.
//
// Two things happen here. Structurally, the surface tree is mapped onto the
// smaller core vocabulary; the seven binding-intent markers are gone by this
// point (the Binder eliminates them during parsing), so reaching one is an
// internal error rather than a case to handle. Semantically, the relations are
// lowered per the specification: `=` becomes the primitive `equals`, and the
// attribute-blind relations get their operands wrapped in `primary`, which is
// what makes them ignore the attribute layer.
//
// Everything else stays an ordinary application. The specification's equations
// for `&`, `->`, `!` and friends define what those operator *functions* mean;
// they are not rewrites the compiler performs here.

// The plain (non-interpolated) name of an operator head.
let private headName (node: Exp) : string option =
    match node with
    | Identifier (_, Resolved (_, [ TextPart s ]))
    | Identifier (_, Referential (_, [ TextPart s ]))
    | Identifier (_, Declarative (_, [ TextPart s ])) -> Some s
    | _ -> None

// Relations that compare values up to attribute erasure. `==` is not in this
// list because it lowers to `equals` rather than to an application.
let private isAttributeBlindRelation (op: string) =
    match op with
    | "_<_" | "_<=_" | "_>=_" | "_>_" | "_!=_" | "_!:_" -> true
    | _ -> false

let rec lower (node: Exp) : CoreExp =
    match node with
    | Apply (r, f, arg) -> lowerApply r f arg
    | Tuple (r, xs) -> CoreExp.Tuple(r, List.map lower xs)
    | Product (r, positional, fusion, scope, named) ->
        CoreExp.Product(r, List.map lower positional, fusion, scope, List.map lower named)
    | Set (r, xs) -> CoreExp.Set(r, List.map lower xs)
    | List (r, xs) -> CoreExp.List(r, List.map lower xs)
    | StringLiteral (r, parts) -> CoreExp.StringLiteral(r, List.map lowerPart parts)
    | Literal (r, lit) -> CoreExp.Literal(r, lit)
    | Identifier (r, role) -> CoreExp.Identifier(r, role)
    | This (r, role) -> CoreExp.This(r, role)
    | Void r -> CoreExp.Void r
    | Unknown r -> CoreExp.Unknown r
    | Local (r, s, e) -> CoreExp.Local(r, s, lower e)
    | ScopeExp (r, s, e) -> CoreExp.Scoped(r, s, lower e)
    | Publish (r, s, access, e) -> CoreExp.Publish(r, access, s, lower e)
    | Record (r, s, e) -> CoreExp.Record(r, s, lower e)
    // The scope the parser attaches to a projection is inert - nothing is ever
    // declared into it - so the core form drops it.
    | Projection (r, _, receiver, m) -> CoreExp.Project(r, lower receiver, lower m)
    | MemberProjection (r, _, receiver, m) -> CoreExp.ProjectMembers(r, lower receiver, lower m)
    | Equals (r, a, b) -> CoreExp.Equals(r, lower a, lower b)
    | Primary (r, e) -> CoreExp.Primary(r, lower e)
    | Candidate (r, e) -> CoreExp.Candidate(r, lower e)
    | IsDefined (r, e) -> CoreExp.IsDefined(r, lower e)
    | Throw (r, e) -> CoreExp.Throw(r, lower e)
    | Catch (r, a, b) -> CoreExp.Catch(r, lower a, lower b)
    // Surface markers. The Binder eliminates all seven while parsing, so one
    // arriving here means an earlier pass is broken, not that the program is.
    | Reference _
    | DeclareLeft _
    | DeclareRight _
    | ReferenceLeft _
    | ReferenceRight _
    | PushScope _
    | PushMemberScope _ ->
        failwith
            $"internal error: surface marker reached lowering at {Range.Range.format node.Range}"

and private lowerPart (part: InterpolatedPart) : CorePart =
    match part with
    | TextPart s -> CoreText s
    | HolePart e -> CoreHole(lower e)

// `a = b` is the primitive `equals`; the `_=_` operator head does not survive
// lowering. `a == b` is the same primitive over primary operands, and every
// other attribute-blind relation keeps its operator function but has its
// operands erased down to the structural core the same way.
and private lowerApply (range: Range.Range) (f: Exp) (arg: Exp) : CoreExp =
    let primaryOf (e: Exp) = CoreExp.Primary(e.Range, lower e)

    match headName f, arg with
    | Some "_=_", Tuple (_, [ a; b ]) -> CoreExp.Equals(range, lower a, lower b)
    | Some "_==_", Tuple (_, [ a; b ]) -> CoreExp.Equals(range, primaryOf a, primaryOf b)
    | Some op, Tuple (argRange, [ a; b ]) when isAttributeBlindRelation op ->
        let operands = CoreExp.Tuple(argRange, [ primaryOf a; primaryOf b ])
        CoreExp.Apply(range, lower f, operands)
    | _ -> CoreExp.Apply(range, lower f, lower arg)
