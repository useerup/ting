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
[<InlineData(" -a.b", "(. (negate a) b)")>]
[<InlineData(" -a^b", "(^ (negate a) b)")>]
[<InlineData("x ? - 1", "(? x (section - 1))")>]
let ``minus spacing`` (code: string, expected: string) = Assert.Equal(expected, parsed code)

[<Theory>]
[<InlineData("(-1)")>]
[<InlineData("-1")>]
let ``minus without a left operand is rejected`` (code: string) =
    Assert.StartsWith("error:", parsed code)

[<Fact>]
let ``juxtaposition binds tighter than property access`` () =
    Assert.Equal("(. (apply f a) b)", parsed "f a.b")

[<Fact>]
let ``error positions point at the offending token`` () =
    match Parser.parse "a +\n  )" with
    | Error e -> Assert.Equal("(2,3-2,4)", Source.Span.render e.Span)
    | Ok _ -> failwith "expected an error"
