module Ting.Syntax.Lexer

open System
open System.Globalization
open Source
open Token

let private isIdentStart (c: char) = Char.IsLetter c || c = '_'
let private isIdentCont (c: char) = Char.IsLetterOrDigit c || c = '_'

type private Lexer(source: SourceText) =
    let text = source.Text
    let len = text.Length

    let startsWith (i: int) (s: string) =
        i + s.Length <= len && String.CompareOrdinal(text, i, s, 0, s.Length) = 0

    let rec skipTrivia (i: int) =
        if i < len && Char.IsWhiteSpace text[i] then skipTrivia (i + 1)
        else i

    member this.Tokens(start: int) : Token list =
        let tokens = ResizeArray<Token>()
        let mutable i = skipTrivia start
        while i < len do
            let kind, e = this.Token i
            tokens.Add { Kind = kind; Span = source.SpanOf(i, e) }
            i <- skipTrivia e
        tokens.Add { Kind = EndOfInput; Span = source.SpanOf(i, i) }
        List.ofSeq tokens

    member this.Token(i: int) : TokenKind * int =
        let c = text[i]
        if Char.IsDigit c then
            this.Number i
        elif isIdentStart c then
            let mutable e = i + 1
            while e < len && isIdentCont text[e] do e <- e + 1
            Identifier(text.Substring(i, e - i)), e
        else
            match symbols |> List.tryFind (startsWith i) with
            | Some s -> Symbol s, i + s.Length
            | None -> source.Fail(i, i + 1, $"unexpected character '{c}'")

    member _.Number(i: int) : TokenKind * int =
        let mutable e = i
        while e < len && Char.IsDigit text[e] do e <- e + 1
        let value = text.Substring(i, e - i)
        if not (fst (Int32.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture))) then
            source.Fail(i, e, $"numeric literal '{value}' is not representable as int")
        Number(value, Int), e

/// Raises SyntaxException at the first lexical error.
let tokenize (source: SourceText) : Token list =
    Lexer(source).Tokens 0
