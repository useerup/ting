module Ting.Syntax.Token

open Source

type NumberKind =
    | Int
    | UInt
    | Short
    | UShort
    | Long
    | ULong
    | Float
    | Double
    | Decimal

type Token =
    { Kind: TokenKind
      Span: Span
      SpaceBefore: bool
      SpaceAfter: bool }

and TokenKind =
    | Identifier of string
    | Keyword of string
    /// Digits without separators, including any fraction.
    | Number of string * NumberKind
    /// Operators and punctuation.
    | Symbol of string
    | EndOfInput

let keywords =
    set
        [ "void"; "this"; "true"; "false" ]

/// Longest match is applied by trying longer symbols first.
let symbols =
    [ "!"; "!="; "%"; "&"; "&&"; "&&&"; "*"; "+"; "-"; "/"; "/%"; "<"; "<="; "="; "=="; ">"; ">="; "?="; "^"
      "|"; "||"; "|||"
      "("; ")"; "["; "]"; "{"; "}"; "{*"; "*}"; ","; "@"; "@@" ]
    |> List.sortByDescending String.length

let describe (kind: TokenKind) =
    match kind with
    | Identifier name -> $"identifier '{name}'"
    | Keyword word -> $"'{word}'"
    | Number(digits, _) -> $"number '{digits}'"
    | Symbol text -> $"'{text}'"
    | EndOfInput -> "end of input"
