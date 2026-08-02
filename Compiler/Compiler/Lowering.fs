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
let private headName (node: Node) : string option =
    match node.Exp with
    | Identifier (Resolved (_, [ TextPart s ]))
    | Identifier (Referential (_, [ TextPart s ]))
    | Identifier (Declarative (_, [ TextPart s ])) -> Some s
    | _ -> None

// Relations that compare values up to attribute erasure. `==` is not in this
// list because it lowers to `equals` rather than to an application.
let private isAttributeBlindRelation (op: string) =
    match op with
    | "_<_" | "_<=_" | "_>=_" | "_>_" | "_!=_" | "_!:_" -> true
    | _ -> false

let rec lower (node: Node) : CoreNode =
    let here core = CoreNode.like node core

    match node.Exp with
    | Apply (f, arg) -> lowerApply node f arg
    | Tuple xs -> here (CoreExp.Tuple(List.map lower xs))
    | Product (positional, fusion, scope, named) ->
        here (CoreExp.Product(List.map lower positional, fusion, scope, List.map lower named))
    | Set xs -> here (CoreExp.Set(List.map lower xs))
    | List xs -> here (CoreExp.List(List.map lower xs))
    | StringLiteral parts -> here (CoreExp.StringLiteral(List.map lowerPart parts))
    | Literal lit -> here (CoreExp.Literal lit)
    | Identifier role -> here (CoreExp.Identifier role)
    | This role -> here (CoreExp.This role)
    | Void -> here CoreExp.Void
    | Unknown -> here CoreExp.Unknown
    | Local (s, e) -> here (CoreExp.Local(s, lower e))
    | ScopeExp (s, e) -> here (CoreExp.Scoped(s, lower e))
    | Publish (s, access, e) -> here (CoreExp.Publish(access, s, lower e))
    | Record (s, e) -> here (CoreExp.Record(s, lower e))
    // The scope the parser attaches to a projection is inert - nothing is ever
    // declared into it - so the core form drops it.
    | Projection (_, receiver, m) -> here (CoreExp.Project(lower receiver, lower m))
    | MemberProjection (_, receiver, m) -> here (CoreExp.ProjectMembers(lower receiver, lower m))
    | Equals (a, b) -> here (CoreExp.Equals(lower a, lower b))
    | Primary e -> here (CoreExp.Primary(lower e))
    | Candidate e -> here (CoreExp.Candidate(lower e))
    | IsDefined e -> here (CoreExp.IsDefined(lower e))
    | Throw e -> here (CoreExp.Throw(lower e))
    | Catch (a, b) -> here (CoreExp.Catch(lower a, lower b))
    // Surface marker nodes. The Binder eliminates all seven while parsing, so
    // one arriving here means an earlier pass is broken, not that the program is.
    | Reference _
    | DeclareLeft _
    | DeclareRight _
    | ReferenceLeft _
    | ReferenceRight _
    | PushScope _
    | PushMemberScope _ ->
        failwith
            $"internal error: surface marker node reached lowering at {Range.Range.format node.Range}"

and private lowerPart (part: InterpolatedPart) : CorePart =
    match part with
    | TextPart s -> CoreText s
    | HolePart e -> CoreHole(lower e)

// `a = b` is the primitive `equals`; the `_=_` operator head does not survive
// lowering. `a == b` is the same primitive over primary operands, and every
// other attribute-blind relation keeps its operator function but has its
// operands erased down to the structural core the same way.
and private lowerApply (node: Node) (f: Node) (arg: Node) : CoreNode =
    let here core = CoreNode.like node core
    let primaryOf (e: Node) = CoreNode.like e (CoreExp.Primary(lower e))

    match headName f, arg.Exp with
    | Some "_=_", Tuple [ a; b ] -> here (CoreExp.Equals(lower a, lower b))
    | Some "_==_", Tuple [ a; b ] -> here (CoreExp.Equals(primaryOf a, primaryOf b))
    | Some op, Tuple [ a; b ] when isAttributeBlindRelation op ->
        let operands = CoreNode.like arg (CoreExp.Tuple [ primaryOf a; primaryOf b ])
        here (CoreExp.Apply(lower f, operands))
    | _ -> here (CoreExp.Apply(lower f, lower arg))
