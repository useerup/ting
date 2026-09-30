module Ting.Syntax.Syntax

open Source
open Token

/// An operator occurrence: its source text (`+`, `in`, `-`, ...) and where it was written.
type Operator = { Text: string; Span: Span }

type Fusion =
    /// `@`: the positional value is fused one level into the product.
    | Fusing
    /// `@@`: the positional value stays a single field.
    | NonFusing

/// The surface syntax tree. It mirrors the source: no desugaring, no binding.
type Expr =
    | Identifier of Span * string
    | This of Span
    | Void of Span
    | Bool of Span * bool
    | Number of Span * string * NumberKind
    | String of Span * StringPart list
    /// `( e )`: grouping, not a product layer.
    | Group of Span * Expr
    /// `()` and `(e1, e2, ...)`.
    | Tuple of Span * Expr list
    /// `(p1, ... @ n1 = v1, ...)` and the `@@` form.
    | Product of Span * Expr list * Fusion * Expr list
    | Set of Span * Expr list
    | Multiset of Span * Expr list
    | List of Span * Expr list
    /// Juxtaposition `f x`.
    | Apply of Span * Expr * Expr
    /// Prefix keywords, `!`, and negation (operator text `-`).
    | Prefix of Span * Operator * Expr
    | Infix of Span * Operator * Expr * Expr

    member this.Span =
        match this with
        | Identifier(s, _)
        | This s
        | Void s
        | Bool(s, _)
        | Number(s, _, _)
        | String(s, _)
        | Group(s, _)
        | Tuple(s, _)
        | Product(s, _, _, _)
        | Set(s, _)
        | Multiset(s, _)
        | List(s, _)
        | Apply(s, _, _)
        | Prefix(s, _, _)
        | Infix(s, _, _, _) -> s

and StringPart =
    | Text of string
