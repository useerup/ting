module Ting.Syntax.Syntax

open Source
open Token

/// An operator occurrence: its source text (`+`, `in`, `-`, ...) and where it was written.
type Operator = { Text: string; Span: Span }

/// The surface syntax tree. It mirrors the source: no desugaring, no binding.
type Expr =
    | Identifier of Span * string
    | Number of Span * string * NumberKind
    /// `( e )`: grouping, not a product layer.
    | Group of Span * Expr
    /// Juxtaposition `f x`.
    | Apply of Span * Expr * Expr
    | Infix of Span * Operator * Expr * Expr

    member this.Span =
        match this with
        | Identifier(s, _)
        | Number(s, _, _)
        | Group(s, _)
        | Apply(s, _, _)
        | Infix(s, _, _, _) -> s
