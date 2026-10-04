module ParserTests

open Xunit
open Ting.Syntax

let private parsed (code: string) =
    match Parser.parse code with
    | Ok expr -> Render.render expr
    | Error e -> $"error: {e.Message}"

// The spacing table from the specification's entry for `-`.
[<Theory>]
[<InlineData("n - 1", "(- n 1)")>]
[<InlineData("n-1", "(- n 1)")>]
[<InlineData("n- 1", "(- n 1)")>]
[<InlineData("n -1", "(apply n (negate 1))")>]
[<InlineData("f -1 -2", "(apply (apply f (negate 1)) (negate 2))")>]
[<InlineData("x + -1", "(+ x (negate 1))")>]
[<InlineData("[1, -1]", "(list 1 (negate 1))")>]
[<InlineData("( -1)", "(negate 1)")>]
[<InlineData(" -a.b", "(negate (. a b))")>]
[<InlineData(" -a^b", "(^ (negate a) b)")>]
[<InlineData("x ? - 1", "(? x (section - 1))")>]
let ``minus spacing`` (code: string, expected: string) = Assert.Equal(expected, parsed code)

[<Theory>]
[<InlineData("(-1)")>]
[<InlineData("-1")>]
let ``minus without a left operand is rejected`` (code: string) =
    Assert.StartsWith("error:", parsed code)

[<Theory>]
[<InlineData("f a.b", "(apply f (. a b))")>]
[<InlineData("Console.WriteLine message", "(apply (. Console WriteLine) message)")>]
[<InlineData("f a.b..c x", "(apply (apply f (.. (. a b) c)) x)")>]
let ``property access binds tighter than juxtaposition`` (code: string, expected: string) =
    Assert.Equal(expected, parsed code)

[<Theory>]
[<InlineData("2 ^ 3 ^ 2", "(^ 2 (^ 3 2))")>]
[<InlineData("f <| g <| x", "(<| f (<| g x))")>]
[<InlineData("x |> f |> g", "(|> (|> x f) g)")>]
let ``associativity`` (code: string, expected: string) = Assert.Equal(expected, parsed code)

[<Theory>]
[<InlineData("a = b = c")>]
[<InlineData("f ?= g = h")>]
[<InlineData("x |> f <| y")>]
[<InlineData("f <| g |> x")>]
let ``operators that do not associate are rejected`` (code: string) =
    Assert.StartsWith("error:", parsed code)

[<Fact>]
let ``spans cover the source text`` () =
    match Parser.parse "let x = 1 in\n  x + 2" with
    | Ok(Syntax.Prefix(span, op, Syntax.Infix(_, inOp, _, body))) ->
        Assert.Equal("(1,1-2,8)", Source.Span.render span)
        Assert.Equal("(1,1-1,4)", Source.Span.render op.Span)
        Assert.Equal("(1,11-1,13)", Source.Span.render inOp.Span)
        Assert.Equal("(2,3-2,8)", Source.Span.render body.Span)
    | other -> failwith $"unexpected parse {other}"

[<Fact>]
let ``error positions point at the offending token`` () =
    match Parser.parse "a +\n  )" with
    | Error e -> Assert.Equal("(2,3-2,4)", Source.Span.render e.Span)
    | Ok _ -> failwith "expected an error"
