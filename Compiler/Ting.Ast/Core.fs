module Core

open Range
open Exp

// The core expression language, as specified in
// docs/specification/core-expression.md.
//
// Exp is the *surface* tree: it mirrors what was written, including the
// binding-intent markers the parser emits and the Binder eliminates. Core is
// what everything after name resolution works on. It is deliberately smaller:
//
//   - the seven surface marker cases (Reference, Declare/Reference Left/Right,
//     PushScope, PushMemberScope) do not exist here at all, so no pass
//     downstream of Lowering has to carry a case for something that cannot
//     occur;
//   - the relations `=`, `==` and the attribute-blind comparisons have become
//     the semantic primitives `equals` and `primary` rather than applications
//     of an operator function;
//   - projections have lost the inert scope the parser attaches to them.
//
// The binding model is shared, not duplicated: BindingRole, Scope, Literal,
// AccessModifier and Fusion all come from Exp. A declaration id means the same
// thing in both trees, which is what lets the scope tables built during binding
// and resolution stay valid across lowering.
//
// As in Exp, every case carries its source Range as its first field; read it
// back with the `Range` member. Do not print this with %A — use
// CoreDiagnostics.renderCore.

[<RequireQualifiedAccess>]
type CoreExp =
    // Application. `apply f a` applies f to a; there is no separate form for
    // multi-argument application, which is application to a tuple.
    | Apply of Range * CoreExp * CoreExp

    // Aggregates.
    | Tuple of Range * CoreExp list
    /// A product with a named section: positional elements, the fusion mode of
    /// the positional section, the scope its names are declared in, and the
    /// named elements.
    | Product of Range * CoreExp list * Fusion * Scope * CoreExp list
    | Set of Range * CoreExp list
    | List of Range * CoreExp list
    /// The spec's `string` form: literal text segments and embedded expressions.
    /// A string with no holes is a Literal, not this.
    | StringLiteral of Range * CorePart list

    // Terms.
    | Literal of Range * Literal
    | Identifier of Range * BindingRole
    | This of Range * BindingRole
    | Void of Range
    /// A placeholder for a subexpression that is not known.
    | Unknown of Range

    // Scope introduction.
    | Local of Range * Scope * CoreExp
    | Scoped of Range * Scope * CoreExp
    | Publish of Range * AccessModifier * Scope * CoreExp
    | Record of Range * Scope * CoreExp

    // Projection.
    | Project of Range * CoreExp * CoreExp
    | ProjectMembers of Range * CoreExp * CoreExp

    // Semantic primitives. There is deliberately no form for `==`: it lowers to
    // Equals over Primary operands.
    | Equals of Range * CoreExp * CoreExp
    | Primary of Range * CoreExp
    | Candidate of Range * CoreExp
    | IsDefined of Range * CoreExp
    | Throw of Range * CoreExp
    | Catch of Range * CoreExp * CoreExp

    /// The source span this expression came from.
    member this.Range =
        match this with
        | Apply (r, _, _)
        | Tuple (r, _)
        | Product (r, _, _, _, _)
        | Set (r, _)
        | List (r, _)
        | StringLiteral (r, _)
        | Literal (r, _)
        | Identifier (r, _)
        | This (r, _)
        | Void r
        | Unknown r
        | Local (r, _, _)
        | Scoped (r, _, _)
        | Publish (r, _, _, _)
        | Record (r, _, _)
        | Project (r, _, _)
        | ProjectMembers (r, _, _)
        | Equals (r, _, _)
        | Primary (r, _)
        | Candidate (r, _)
        | IsDefined (r, _)
        | Throw (r, _)
        | Catch (r, _, _) -> r

and CorePart =
    | CoreText of string
    | CoreHole of CoreExp
