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
    /// A regular or quoted identifier. A quoted identifier may spell a keyword.
    | Identifier of string
    | InterpolatedIdentifier of TokenStringPart list
    | Keyword of string
    /// Digits without separators, including any fraction.
    | Number of string * NumberKind
    /// Every string form after delimiter, escape and indentation processing.
    | String of TokenStringPart list
    /// Operators and punctuation.
    | Symbol of string
    | EndOfInput

and TokenStringPart =
    | TextPart of string
    /// The tokens of a replacement block, terminated by EndOfInput.
    | HolePart of Token list

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
    | InterpolatedIdentifier _ -> "interpolated identifier"
    | Keyword word -> $"'{word}'"
    | Number(digits, _) -> $"number '{digits}'"
    | String _ -> "string"
    | Symbol text -> $"'{text}'"
    | EndOfInput -> "end of input"
