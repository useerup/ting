module Ting.Syntax.Source

/// Line and column are 1-based; Offset is the 0-based character index.
[<Struct>]
type Position = { Offset: int; Line: int; Column: int }

/// End is exclusive.
type Span =
    { Start: Position
      End: Position }

module Span =
    let between (first: Span) (last: Span) = { Start = first.Start; End = last.End }

    let render (span: Span) =
        $"({span.Start.Line},{span.Start.Column}-{span.End.Line},{span.End.Column})"

type SyntaxError = { Message: string; Span: Span }

/// Lexing and parsing stop at the first error.
exception SyntaxException of SyntaxError

/// Maps character offsets to line/column positions.
type SourceText(text: string) =
    let lineStarts =
        [| yield 0
           for i in 0 .. text.Length - 1 do
               if text[i] = '\n' then yield i + 1 |]

    member _.Text = text

    member _.PositionAt(offset: int) : Position =
        let mutable lo = 0
        let mutable hi = lineStarts.Length - 1
        while lo < hi do
            let mid = (lo + hi + 1) / 2
            if lineStarts[mid] <= offset then lo <- mid else hi <- mid - 1
        { Offset = offset; Line = lo + 1; Column = offset - lineStarts[lo] + 1 }

    member this.SpanOf(start: int, finish: int) : Span =
        { Start = this.PositionAt start; End = this.PositionAt finish }

    member this.Fail(start: int, finish: int, message: string) : 'a =
        raise (SyntaxException { Message = message; Span = this.SpanOf(start, finish) })
