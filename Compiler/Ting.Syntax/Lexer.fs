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
    /// A raw string on a single line, ending at a run of exactly K quotes.
    | RawLine of int
    /// A multi-line raw string: the base indentation and the end of the content lines.
    | RawBlock of string * int

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

    /// Lexes tokens from `start`. With `closing = Some n` it stops before a run of
    /// n closing braces at bracket depth zero (the end of a replacement block).
    member this.Tokens(start: int, closing: int option) : Token list * int =
        let tokens = ResizeArray<Token>()
        let mutable depth = 0
        let mutable i = skipTrivia start
        let mutable spaceBefore = i > start
        let mutable stop = -1
        while stop < 0 do
            match closing with
            | Some n when depth = 0 && runLength i '}' >= n -> stop <- i
            | Some _ when i >= len -> source.Fail(start, i, "unterminated replacement block")
            | None when i >= len -> stop <- i
            | _ ->
                let kind, e = this.Token i
                match kind with
                | Symbol "{"
                | Symbol "{*" -> depth <- depth + 1
                | Symbol "}"
                | Symbol "*}" -> depth <- depth - 1
                | _ -> ()
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
              Span = source.SpanOf(stop, stop)
              SpaceBefore = spaceBefore
              SpaceAfter = true }
        List.ofSeq tokens, stop

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
            this.StringLiteral(i, i, 0, false)
        elif c = '$' then
            let n = runLength i '$'
            let j = i + n
            match at j, at (j + 1) with
            | '"', _ -> this.StringLiteral(i, j, n, false)
            | '@', '"' -> this.StringLiteral(i, j + 1, n, true)
            | _ -> source.Fail(i, j, "'$' must be followed by a string")
        elif c = '@' && at (i + 1) = '"' then
            this.StringLiteral(i, i + 1, 0, true)
        elif c = '@' && at (i + 1) = '$' then
            let n = runLength (i + 1) '$'
            if at (i + 1 + n) = '"' then
                this.StringLiteral(i, i + 1 + n, n, true)
            else
                source.Fail(i, i + 1 + n, "'@$' must be followed by a string")
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

    /// A replacement block whose opening braces end just before `start`.
    /// Not a block when it is empty or cannot be lexed; the braces are then literal.
    member this.TryHole(start: int, braces: int) : (Token list * int) option =
        try
            match this.Tokens(start, Some braces) with
            | [ _ ], _ -> None
            | tokens, stop -> Some(tokens, stop + braces)
        with SyntaxException _ ->
            None

    /// Scans string content. Returns the parts and the index of the terminator.
    member this.Content(start: int, openStart: int, mode: StringMode, dollars: int) : TokenStringPart list * int =
        let parts = ResizeArray<TokenStringPart>()
        let buffer = StringBuilder()
        let flush () =
            if buffer.Length > 0 then
                parts.Add(TextPart(buffer.ToString()))
                buffer.Clear() |> ignore
        let literal (s: string) = buffer.Append(s) |> ignore
        let unterminated i = source.Fail(openStart, i, "unterminated string literal")
        let rec loop (i: int) (lineStart: bool) : int =
            match mode with
            | RawBlock(_, limit) when i >= limit -> i
            | RawBlock(indent, limit) when lineStart ->
                if startsWith i indent then
                    loop (i + indent.Length) false
                else
                    let lineEnd = let e = text.IndexOf('\n', i) in if e < 0 || e > limit then limit else e
                    if Seq.forall Char.IsWhiteSpace (text.Substring(i, lineEnd - i)) then
                        literal "\n"
                        loop (lineEnd + 1) true
                    else
                        source.Fail(i, lineEnd, "raw string content is indented less than its closing delimiter")
            | _ when i >= len -> unterminated i
            | _ ->
                let c = text[i]
                let isTerminator =
                    match mode with
                    | Regular -> c = '"'
                    | Verbatim -> c = '"' && at (i + 1) <> '"'
                    | RawLine k -> c = '"' && runLength i '"' = k
                    | RawBlock _ -> false
                if isTerminator then
                    i
                elif c = '\n' || (c = '\r' && at (i + 1) = '\n') then
                    match mode with
                    | Verbatim ->
                        literal (string c)
                        loop (i + 1) false
                    | RawBlock _ ->
                        literal "\n"
                        loop (if c = '\r' then i + 2 else i + 1) true
                    | _ -> unterminated i
                elif dollars > 0 && c = '{' then
                    let run = runLength i '{'
                    if run < dollars then
                        literal (System.String('{', run))
                        loop (i + run) false
                    else
                        match this.TryHole(i + run, dollars) with
                        | Some(tokens, next) ->
                            literal (System.String('{', run - dollars))
                            flush ()
                            parts.Add(HolePart tokens)
                            loop next false
                        | None ->
                            literal (System.String('{', run))
                            loop (i + run) false
                else
                    match mode, c with
                    | Regular, '\\' ->
                        let decoded, next = escape i
                        literal decoded
                        loop next false
                    | Verbatim, '"' ->
                        literal "\""
                        loop (i + 2) false
                    | RawLine _, '"' ->
                        let run = runLength i c
                        literal (System.String(c, run))
                        loop (i + run) false
                    | _ ->
                        literal (string c)
                        loop (i + 1) false
        let stop = loop start true
        flush ()
        List.ofSeq parts, stop

    member this.StringLiteral(start: int, quote: int, dollars: int, verbatim: bool) : TokenKind * int =
        let quotes = runLength quote '"'
        if verbatim then
            let parts, stop = this.Content(quote + 1, start, Verbatim, dollars)
            String parts, stop + 1
        elif quotes = 2 then
            String [], quote + 2
        elif quotes = 1 then
            let parts, stop = this.Content(quote + 1, start, Regular, dollars)
            String parts, stop + 1
        else
            let afterOpen = quote + quotes
            let lineEnd = let e = text.IndexOf('\n', afterOpen) in if e < 0 then len else e
            if Seq.forall Char.IsWhiteSpace (text.Substring(afterOpen, lineEnd - afterOpen)) then
                if lineEnd >= len then source.Fail(start, afterOpen, "unterminated raw string literal")
                let contentStart = lineEnd + 1
                let rec findClose (i: int) =
                    let q = text.IndexOf('"', i)
                    if q < 0 then source.Fail(start, afterOpen, "unterminated raw string literal")
                    else
                        let run = runLength q '"'
                        if run = quotes then q else findClose (q + run)
                let close = findClose contentStart
                let lineStart = text.LastIndexOf('\n', close - 1) + 1
                let indent = text.Substring(lineStart, close - lineStart)
                if lineStart < contentStart || not (Seq.forall Char.IsWhiteSpace indent) then
                    source.Fail(lineStart, close + quotes, "a raw string's closing delimiter must be on its own line")
                let parts, _ = this.Content(contentStart, start, RawBlock(indent, lineStart), dollars)
                String parts, close + quotes
            else
                let parts, stop = this.Content(afterOpen, start, RawLine quotes, dollars)
                String parts, stop + quotes

/// Raises SyntaxException at the first lexical error.
let tokenize (source: SourceText) : Token list =
    fst (Lexer(source).Tokens(0, None))
