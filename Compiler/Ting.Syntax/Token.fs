module Ting.Syntax.Token

open Source

type NumberKind =
    | Int

type Token =
    { Kind: TokenKind
      Span: Span
      SpaceBefore: bool
      SpaceAfter: bool }

and TokenKind =
    | Identifier of string
    /// Digits without separators, including any fraction.
    | Number of string * NumberKind
    /// Operators and punctuation.
    | Symbol of string
    | EndOfInput

/// Longest match is applied by trying longer symbols first.
let symbols =
    [ "!="; "%"; "*"; "+"; "-"; "/"; "/%"; "<"; "<="; "="; "=="; ">"; ">="; "?="; "^"; "("; ")" ]
    |> List.sortByDescending String.length

let describe (kind: TokenKind) =
    match kind with
    | Identifier name -> $"identifier '{name}'"
    | Number(digits, _) -> $"number '{digits}'"
    | Symbol text -> $"'{text}'"
    | EndOfInput -> "end of input"
