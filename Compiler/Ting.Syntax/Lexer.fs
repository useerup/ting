module Ting.Syntax.Lexer

open System
open System.Globalization
open System.Text
open Source
open Token

let private isIdentStart (c: char) = Char.IsLetter c || c = '_'
let private isIdentCont (c: char) = Char.IsLetterOrDigit c || c = '_'

type private StringMode =
    /// Ends at an unescaped `"`. Escapes are processed; newlines are not allowed.
    | Regular
    /// Ends at a `"` not followed by another; `""` is a literal quote.
    | Verbatim

type private Lexer(source: SourceText) =
    let text = source.Text
    let len = text.Length
    let at i = if i >= 0 && i < len then text[i] else '\000'

    let startsWith (i: int) (s: string) =
        i + s.Length <= len && String.CompareOrdinal(text, i, s, 0, s.Length) = 0

    let runLength (i: int) (c: char) =
        let mutable j = i
        while j < len && text[j] = c do j <- j + 1
        j - i

    let rec skipBlockComment (start: int) (i: int) (depth: int) =
        if i >= len then source.Fail(start, start + 2, "unterminated block comment")
        elif startsWith i "/*" then skipBlockComment start (i + 2) (depth + 1)
        elif startsWith i "*/" then
            if depth = 1 then i + 2 else skipBlockComment start (i + 2) (depth - 1)
        else skipBlockComment start (i + 1) depth

    /// Comments are whitespace.
    let rec skipTrivia (i: int) =
        if i < len && Char.IsWhiteSpace text[i] then skipTrivia (i + 1)
        elif startsWith i "//" then
            let e = text.IndexOf('\n', i)
            if e < 0 then len else skipTrivia (e + 1)
        elif startsWith i "/*" then skipTrivia (skipBlockComment i (i + 2) 1)
        else i

    let hexValue (start: int) (count: int) =
        let digits = text.Substring(start, count)
        match Int32.TryParse(digits, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture) with
        | true, v -> v
        | _ -> source.Fail(start, start + count, $"invalid hexadecimal digits '{digits}'")

    /// Returns the decoded text and the index after the escape sequence.
    let escape (i: int) : string * int =
        let simple (s: string) = s, i + 2
        let isHex c = Uri.IsHexDigit c
        let scalar (start: int) (count: int) =
            let v = hexValue start count
            if v > 0x10FFFF || (v >= 0xD800 && v <= 0xDFFF && count <> 4) then
                source.Fail(i, start + count, "escape sequence is not a Unicode scalar value")
            (if v <= 0xFFFF then string (char v) else Char.ConvertFromUtf32 v), start + count
        match at (i + 1) with
        | '\\' -> simple "\\"
        | '"' -> simple "\""
        | 'n' -> simple "\n"
        | 'r' -> simple "\r"
        | 't' -> simple "\t"
        | 'a' -> simple "\a"
        | 'b' -> simple "\b"
        | 'f' -> simple "\f"
        | 'v' -> simple "\v"
        | '0' -> simple "\000"
        | 'u' when Seq.forall isHex (Seq.init 4 (fun k -> at (i + 2 + k))) -> scalar (i + 2) 4
        | 'U' when Seq.forall isHex (Seq.init 8 (fun k -> at (i + 2 + k))) -> scalar (i + 2) 8
        | 'x' when isHex (at (i + 2)) ->
            let mutable n = 1
            while n < 4 && isHex (at (i + 2 + n)) do n <- n + 1
            scalar (i + 2) n
        | _ -> source.Fail(i, min len (i + 2), "unrecognised escape sequence")

    member this.Tokens(start: int) : Token list =
        let tokens = ResizeArray<Token>()
        let mutable i = skipTrivia start
        let mutable spaceBefore = i > start
        while i < len do
            let kind, e = this.Token i
            let next = skipTrivia e
            tokens.Add
                { Kind = kind
                  Span = source.SpanOf(i, e)
                  SpaceBefore = spaceBefore
                  SpaceAfter = next > e || next >= len }
            spaceBefore <- next > e
            i <- next
        tokens.Add
            { Kind = EndOfInput
              Span = source.SpanOf(i, i)
              SpaceBefore = spaceBefore
              SpaceAfter = true }
        List.ofSeq tokens

    member this.Token(i: int) : TokenKind * int =
        let c = text[i]
        if Char.IsDigit c then
            this.Number i
        elif isIdentStart c then
            let mutable e = i + 1
            while e < len && isIdentCont text[e] do e <- e + 1
            let word = text.Substring(i, e - i)
            (if keywords.Contains word then Keyword word else Identifier word), e
        elif c = '"' then
            this.StringLiteral(i, i, false)
        elif c = '@' && at (i + 1) = '"' then
            this.StringLiteral(i, i + 1, true)
        elif c = '@' then
            if at (i + 1) = '@' && at (i + 2) <> '"' then Symbol "@@", i + 2 else Symbol "@", i + 1
        else
            match symbols |> List.tryFind (startsWith i) with
            | Some s -> Symbol s, i + s.Length
            | None -> source.Fail(i, i + 1, $"unexpected character '{c}'")

    member _.Number(i: int) : TokenKind * int =
        let digits = StringBuilder()
        let rec scanDigits j =
            if j < len && Char.IsDigit text[j] then
                digits.Append(text[j]) |> ignore
                scanDigits (j + 1)
            elif j < len && text[j] = '_' then
                if Char.IsDigit(at (j + 1)) then scanDigits (j + 1)
                else source.Fail(j, j + 1, "a digit separator must be followed by a digit")
            else
                j
        let j = scanDigits i
        let j, isReal =
            if at j = '.' && Char.IsDigit(at (j + 1)) then
                digits.Append('.') |> ignore
                scanDigits (j + 1), true
            else
                j, false
        let suffixes =
            [ "UL", ULong; "us", UShort; "u", UInt; "s", Short; "L", Long
              "f", Float; "d", Double; "m", Decimal ]
        let kind, e =
            match suffixes |> List.tryFind (fun (s, _) -> startsWith j s) with
            | Some(s, k) -> k, j + s.Length
            | None -> (if isReal then Float else Int), j
        if e < len && isIdentCont text[e] then
            source.Fail(i, e + 1, "invalid numeric literal suffix")
        let value = digits.ToString()
        let inv = CultureInfo.InvariantCulture
        let ints = NumberStyles.None
        let reals = NumberStyles.AllowDecimalPoint
        let fits =
            match kind with
            | Int -> not isReal && fst (Int32.TryParse(value, ints, inv))
            | UInt -> not isReal && fst (UInt32.TryParse(value, ints, inv))
            | Short -> not isReal && fst (Int16.TryParse(value, ints, inv))
            | UShort -> not isReal && fst (UInt16.TryParse(value, ints, inv))
            | Long -> not isReal && fst (Int64.TryParse(value, ints, inv))
            | ULong -> not isReal && fst (UInt64.TryParse(value, ints, inv))
            | Float ->
                match Single.TryParse(value, reals, inv) with
                | true, v -> Single.IsFinite v
                | _ -> false
            | Double ->
                match Double.TryParse(value, reals, inv) with
                | true, v -> Double.IsFinite v
                | _ -> false
            | Decimal -> fst (Decimal.TryParse(value, reals, inv))
        if not fits then
            let setName = (string kind).ToLowerInvariant()
            source.Fail(i, e, $"numeric literal '{text.Substring(i, e - i)}' is not representable as {setName}")
        Number(value, kind), e

    /// Scans string content. Returns the parts and the index of the terminator.
    member this.Content(start: int, openStart: int, mode: StringMode) : TokenStringPart list * int =
        let parts = ResizeArray<TokenStringPart>()
        let buffer = StringBuilder()
        let flush () =
            if buffer.Length > 0 then
                parts.Add(TextPart(buffer.ToString()))
                buffer.Clear() |> ignore
        let literal (s: string) = buffer.Append(s) |> ignore
        let unterminated i = source.Fail(openStart, i, "unterminated string literal")
        let rec loop (i: int) : int =
            if i >= len then
                unterminated i
            else
                let c = text[i]
                let isTerminator =
                    match mode with
                    | Regular -> c = '"'
                    | Verbatim -> c = '"' && at (i + 1) <> '"'
                if isTerminator then
                    i
                elif c = '\n' || (c = '\r' && at (i + 1) = '\n') then
                    match mode with
                    | Verbatim ->
                        literal (string c)
                        loop (i + 1)
                    | Regular -> unterminated i
                else
                    match mode, c with
                    | Regular, '\\' ->
                        let decoded, next = escape i
                        literal decoded
                        loop next
                    | Verbatim, '"' ->
                        literal "\""
                        loop (i + 2)
                    | _ ->
                        literal (string c)
                        loop (i + 1)
        let stop = loop start
        flush ()
        List.ofSeq parts, stop

    member this.StringLiteral(start: int, quote: int, verbatim: bool) : TokenKind * int =
        let quotes = runLength quote '"'
        if verbatim then
            let parts, stop = this.Content(quote + 1, start, Verbatim)
            String parts, stop + 1
        elif quotes = 2 then
            String [], quote + 2
        else
            let parts, stop = this.Content(quote + 1, start, Regular)
            String parts, stop + 1

/// Raises SyntaxException at the first lexical error.
let tokenize (source: SourceText) : Token list =
    Lexer(source).Tokens 0
