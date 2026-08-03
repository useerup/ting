module RangeTests

open System
open Xunit
open Exp

// Source-range tests.
//
// These exist because the notebook snapshot tests cannot catch range errors:
// renderScopeReport/renderExpression deliberately do not print ranges, so every
// range could be wrong and all 35 snapshots would still pass. These assert
// concrete line/column spans instead.

let private parseNode (source: string) : Exp =
    let rootScope = LocalScope(Guid.Empty) :> Scope
    match Parser.parseNamed rootScope "test.ting" source with
    | ParseResult.ParseSucceeded node -> node
    | ParseResult.ParseFailed err -> failwith $"parse failed: {err}"

/// (startLine, startColumn, endLine, endColumn)
let private span (n: Exp) =
    (n.Range.Start.Line, n.Range.Start.Column, n.Range.End.Line, n.Range.End.Column)

/// The source text a node's range points at, to prove ranges index the original
/// text correctly rather than merely being self-consistent.
let private sliceOf (source: string) (n: Exp) =
    source.Substring(int n.Range.Start.Index, int (n.Range.End.Index - n.Range.Start.Index))

[<Fact>]
let ``literal range covers exactly the literal`` () =
    let source = "42"
    let node = parseNode source
    Assert.Equal((1L, 1L, 1L, 3L), span node)
    Assert.Equal("42", sliceOf source node)

[<Fact>]
let ``trailing whitespace is not included in a range`` () =
    let source = "42   "
    let node = parseNode source
    Assert.Equal("42", sliceOf source node)

[<Fact>]
let ``binary operator node spans both operands`` () =
    let source = "1 + 2"
    let node = parseNode source
    Assert.Equal("1 + 2", sliceOf source node)

[<Fact>]
let ``operand ranges point at their own text`` () =
    let source = "1 + 2"
    match parseNode source with
    | Apply (_, _, Tuple (_, [ left; right ])) ->
        Assert.Equal("1", sliceOf source left)
        Assert.Equal("2", sliceOf source right)
    | other -> failwith $"unexpected shape: {other}"

[<Fact>]
let ``operator head range points at the operator token`` () =
    let source = "1 + 2"
    match parseNode source with
    | Apply (_, head, _) ->
        Assert.Equal("+", sliceOf source head)
        Assert.Equal((1L, 3L, 1L, 4L), span head)
    | other -> failwith $"unexpected shape: {other}"

[<Fact>]
let ``multi character operator head range is correct`` () =
    let source = "a == b"
    match parseNode source with
    | Apply (_, head, _) -> Assert.Equal("==", sliceOf source head)
    | other -> failwith $"unexpected shape: {other}"

[<Fact>]
let ``ranges span multiple lines`` () =
    let source = "1 +\n  2"
    let node = parseNode source
    let (sl, sc, el, _) = span node
    Assert.Equal(1L, sl)
    Assert.Equal(1L, sc)
    Assert.Equal(2L, el)

[<Fact>]
let ``identifier on a later line reports that line`` () =
    let source = "let x = 1 in\nx"
    let node = parseNode source
    // The whole expression starts at line 1 and ends on line 2.
    let (sl, _, el, _) = span node
    Assert.Equal(1L, sl)
    Assert.Equal(2L, el)

[<Fact>]
let ``parenthesised tuple range includes both delimiters`` () =
    let source = "(1, 2)"
    let node = parseNode source
    Assert.Equal("(1, 2)", sliceOf source node)

[<Fact>]
let ``let declaration range covers keyword and body`` () =
    let source = "let x = 1"
    let node = parseNode source
    Assert.Equal("let x = 1", sliceOf source node)

[<Fact>]
let ``juxtaposition application spans callee and argument`` () =
    let source = "f x"
    let node = parseNode source
    Assert.Equal("f x", sliceOf source node)

[<Fact>]
let ``stream name is recorded in every range`` () =
    let node = parseNode "1 + 2"
    Assert.Equal("test.ting", node.Range.File)

[<Fact>]
let ``projection range covers receiver and member`` () =
    let source = "a.b"
    let node = parseNode source
    Assert.Equal("a.b", sliceOf source node)
