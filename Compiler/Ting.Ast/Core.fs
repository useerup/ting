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

[<RequireQualifiedAccess>]
type CoreExp =
    // Application. `apply f a` applies f to a; there is no separate form for
    // multi-argument application, which is application to a tuple.
    | Apply of CoreNode * CoreNode

    // Aggregates.
    | Tuple of CoreNode list
    /// A product with a named section: positional elements, the fusion mode of
    /// the positional section, the scope its names are declared in, and the
    /// named elements.
    | Product of CoreNode list * Fusion * Scope * CoreNode list
    | Set of CoreNode list
    | List of CoreNode list
    /// The spec's `string` form: literal text segments and embedded expressions.
    /// A string with no holes is a Literal, not this.
    | StringLiteral of CorePart list

    // Terms.
    | Literal of Literal
    | Identifier of BindingRole
    | This of BindingRole
    | Void
    /// A placeholder for a subexpression that is not known.
    | Unknown

    // Scope introduction.
    | Local of Scope * CoreNode
    | Scoped of Scope * CoreNode
    | Publish of AccessModifier * Scope * CoreNode
    | Record of Scope * CoreNode

    // Projection.
    | Project of CoreNode * CoreNode
    | ProjectMembers of CoreNode * CoreNode

    // Semantic primitives. There is deliberately no form for `==`: it lowers to
    // Equals over Primary operands.
    | Equals of CoreNode * CoreNode
    | Primary of CoreNode
    | Candidate of CoreNode
    | IsDefined of CoreNode
    | Throw of CoreNode
    | Catch of CoreNode * CoreNode

/// A core expression together with the source span it came from.
///
/// The span field is named Span rather than Range so that `.Range` keeps
/// resolving to Exp.Node in inference; the same reason ParseError and
/// Resolver.Diagnostic name theirs Span.
///
/// As with Node: do not print this with %A, each hop costs print depth. Use
/// CoreDiagnostics.renderCore.
and CoreNode = {
    Core: CoreExp
    Span: Range
}

and CorePart =
    | CoreText of string
    | CoreHole of CoreNode

[<RequireQualifiedAccess>]
module CoreNode =
    let at (span: Range) (core: CoreExp) = { Core = core; Span = span }

    /// A node with no source text of its own.
    let synthetic (core: CoreExp) = { Core = core; Span = Range.synthetic }

    /// A node standing in for a surface node, reporting its span.
    let like (source: Node) (core: CoreExp) = { Core = core; Span = source.Range }

    /// A node spanning two operands.
    let spanning (a: CoreNode) (b: CoreNode) (core: CoreExp) = {
        Core = core
        Span = Range.span a.Span b.Span
    }

/// Match a core node's expression while ignoring its span, for nested pattern
/// positions. Prefer plain `match node.Core with` at the top level of a match so
/// exhaustiveness checking keeps working.
let inline (|C|) (node: CoreNode) = node.Core
