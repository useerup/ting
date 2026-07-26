module Range

// Source locations for AST nodes.
//
// These types are deliberately independent of any parser library: the parser is
// a swappable implementation detail behind the ParseResult contract, so it is
// the parser's job to convert its own position type into these at the boundary.
//
// Line and Column are 1-based, matching how editors and compiler diagnostics
// report positions. Index is the 0-based character offset into the stream and is
// what slicing the original source text expects.

[<Struct>]
type Position = {
    Index: int64
    Line: int64
    Column: int64
}

/// A half-open source span [Start, End): Start is the first character of the
/// construct and End is the position just past its last character. Empty spans
/// (Start = End) are legal and denote an insertion point.
[<Struct>]
type Range = {
    File: string
    Start: Position
    End: Position
}

[<RequireQualifiedAccess>]
module Position =
    let zero = { Index = 0L; Line = 1L; Column = 1L }

    let create (index: int64) (line: int64) (column: int64) = {
        Index = index
        Line = line
        Column = column
    }

    /// Order by stream offset. Positions from different files are not comparable
    /// and callers are expected not to mix them.
    let min (a: Position) (b: Position) = if a.Index <= b.Index then a else b

    let max (a: Position) (b: Position) = if a.Index >= b.Index then a else b

[<RequireQualifiedAccess>]
module Range =
    /// The range used for nodes that have no source text of their own: builtins
    /// registered into the root scope, and placeholders such as Unknown.
    let synthetic = {
        File = ""
        Start = Position.zero
        End = Position.zero
    }

    let isSynthetic (r: Range) = r.File = "" && r.Start.Index = 0L && r.End.Index = 0L

    let create (file: string) (start: Position) (finish: Position) = {
        File = file
        Start = start
        End = finish
    }

    /// The smallest range covering both inputs. A synthetic operand contributes
    /// nothing, so desugarings that mix real and synthetic parts still report
    /// the real source span.
    let span (a: Range) (b: Range) =
        if isSynthetic a then b
        elif isSynthetic b then a
        else
            { File = (if a.File = "" then b.File else a.File)
              Start = Position.min a.Start b.Start
              End = Position.max a.End b.End }

    let spanAll (ranges: Range seq) = Seq.fold span synthetic ranges

    /// "line,column-line,column" — the shape used in diagnostics messages.
    let format (r: Range) =
        if isSynthetic r then
            "(synthetic)"
        else
            $"({r.Start.Line},{r.Start.Column}-{r.End.Line},{r.End.Column})"
