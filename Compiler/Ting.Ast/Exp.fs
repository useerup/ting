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

/// The raw abstract syntax tree produced directly by the parser (Parser.fs),
/// before the Binder lowers it into CoreExp. Each case documents the source
/// pattern that produces it:
///
///   Declare          Prefix `let E` — opens a declaration/constraint region over `E`.
///   Reference        Not a keyword: `pApp` wraps the callee of a juxtaposition
///                    application (`f x` => `Apply(Reference f, x)`) to flag it as a
///                    use-site rather than a declaration.
///   DeclareLeft      Infix operator whose LEFT operand is declarative; fields are
///                    (operator head, left, right). Produced by `x => body` (lambda
///                    parameter) and `elem \ gen` (set-comprehension binder).
///   DeclareRight     Infix operator whose RIGHT operand is declarative. Produced by
///                    `body where decls`.
///   ReferenceLeft    Infix operator with a plain-reference LEFT operand. Defined for
///                    symmetry but currently NOT produced by the parser (no operator
///                    is wired to `referenceLeftOperator`).
///   ReferenceRight   Infix operator whose RIGHT operand is a plain reference while the
///                    left keeps the ambient binding context. Produced by `=`, `in`,
///                    `then`, `->`, `catch`, `?`, `??`, `:`, `::`, `:::`, `<`, `<=`,
///                    `>=`, `>`, `==`, `!=`, `!:` — e.g. `x = 5`, `decls in body`.
///   Publish          Prefix access modifier on a declaration: `public E`, `private E`,
///                    `internal E`, `protected E`.
///   Apply            Function/operator application. Two sources: juxtaposition
///                    `f x` => `Apply(Reference f, x)`; and desugaring of every "plain"
///                    operator, e.g. `a + b` => `Apply(BuiltIn "_+_", Tuple [a; b])`,
///                    `a else b`, `a || b`, prefix `!x` => `Apply(BuiltIn "!_", x)`,
///                    `fn x`, `class x`.
///   Tuple            Parenthesized comma-separated list `(a, b, ...)`, including the
///                    empty tuple `()`. Also synthesized as the argument carrying both
///                    operands of a desugared binary operator: `Tuple [x; y]`.
///   Record           Prefix `record E`. (`records E` produces `Set [Record E]`.)
///   Identifier       An identifier term: a plain name `foo`, a backtick-quoted name
///                    `` `foo bar` `` (both a single TextPart), or an interpolated
///                    identifier `` $`pre{expr}post` `` (text + holes).
///   BuiltIn          Never written in source; synthesized as the operator/function head
///                    for every operator desugaring, carrying the canonical operator
///                    name, e.g. `_+_`, `_=>_`, `_where_`, `!_`, `fn_`.
///   Literal          A literal term (integer, float, decimal, boolean, or string);
///                    see the Literal type. Note `Float64` has no source syntax and is
///                    not produced by the parser.
///   Set              Brace comma-separated list `{a, b, ...}`, including the empty set
///                    `{}`. Also produced by prefix `records E` (`Set [Record E]`) and
///                    `tuples E` (`Set [E]`).
///   List             Bracket comma-separated list `[a, b, ...]`, including the empty
///                    list `[]`.
///   InterpolatedString  Interpolated string `$"text{expr}text"` — alternating literal
///                    text runs and `{expr}` holes.
///   PushScope        Infix `./` — `scope ./ body`: pushes `scope` as an expression
///                    scope over `body`.
///   PushMemberScope  Infix `../` — `scope ../ body`: pushes the members of `scope` as
///                    a scope over `body`.
///   Projection       Infix `.` — `receiver.member`: direct property/member projection.
///   MemberProjection Infix `..` — `receiver..member`: member (extension) projection.
///   This             The `this` keyword.
///   Void             The `void` keyword.
type Exp =
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
