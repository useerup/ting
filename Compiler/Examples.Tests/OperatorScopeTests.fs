module OperatorScopeTests

open System
open Xunit
open Exp

// Tests for the parser's operator scope.
//
// Operators are sugar for applying ordinary functions whose names mark the
// operand positions with `_`. The parser knows every operator it accepts, so it
// declares those functions itself and binds each operator head straight to that
// scope. These tests pin that contract down: the notebook snapshots only show
// the *result* of resolution, so they cannot tell a head bound to the operator
// scope from one that happened to resolve lexically.

let private parseNode (source: string) : Exp =
    let rootScope = LocalScope(Guid.Empty) :> Scope
    match Parser.parseNamed rootScope "test.ting" source with
    | ParseResult.ParseSucceeded node -> node
    | ParseResult.ParseFailed err -> failwith $"parse failed for '{source}': {err}"

/// Every binding role in the tree. Written without a wildcard so that adding an
/// Exp case is a compile error here rather than a silently unvisited subtree.
let rec private allRoles (node: Exp) : BindingRole list =
    let self =
        match node with
        | Identifier (_, role)
        | This (_, role) -> [ role ]
        | _ -> []

    let children =
        match node with
        | Reference (_, e)
        | Local (_, _, e)
        | ScopeExp (_, _, e)
        | Record (_, _, e)
        | Publish (_, _, _, e)
        | Throw (_, e)
        | Primary (_, e)
        | Candidate (_, e)
        | IsDefined (_, e) -> [ e ]
        | DeclareLeft (_, a, b, c)
        | DeclareRight (_, a, b, c)
        | ReferenceLeft (_, a, b, c)
        | ReferenceRight (_, a, b, c) -> [ a; b; c ]
        | Apply (_, a, b)
        | PushScope (_, a, b)
        | PushMemberScope (_, a, b)
        | Projection (_, _, a, b)
        | MemberProjection (_, _, a, b)
        | Catch (_, a, b)
        | Equals (_, a, b) -> [ a; b ]
        | Tuple (_, xs)
        | Set (_, xs)
        | List (_, xs) -> xs
        | Product (_, positional, _, _, named) -> positional @ named
        | StringLiteral (_, parts) ->
            parts
            |> List.choose (function
                | HolePart e -> Some e
                | TextPart _ -> None)
        | Identifier _
        | This _
        | Literal _
        | Void _
        | Unknown _ -> []

    self @ List.collect allRoles children

let private formatName (parts: InterpolatedParts) =
    parts
    |> List.map (function
        | TextPart s -> s
        | HolePart _ -> "{...}")
    |> String.concat ""

/// The names bound to the operator scope in a parsed expression.
let private operatorNamesIn (source: string) : string list =
    parseNode source
    |> allRoles
    |> List.choose (function
        | Referential ([ sid ], parts) when sid = Parser.operatorScope.Id -> Some(formatName parts)
        | _ -> None)

let private declaredInOperatorScope (name: string) =
    Parser.operatorScope.Names
    |> Seq.exists (fun kvp -> formatName kvp.Value.NameParts = name)

/// One source snippet per operator, paired with the function that operator
/// lowers to. Keeping the whole table here is what makes the tests below a
/// check on the *grammar*, not just on a couple of sampled operators.
let private infixOperators =
    [ "where"; "in"; "="; "?="; "then"; "else"; "->"
      ">>"; "<<"; "|>"; "<|"
      "=>"; "catch"; "\\"
      "|"; "||"; "|||"; "&"; "&&"; "&&&"; "?"; "??"
      ":"; "::"; ":::"; "<"; "<="; ">="; ">"; "=="; "!="; "!:"
      "..."; "...<"; "<..."; "<...<"
      ";"; ";;"
      "^^"; "**"; "+"; "-"; "*"; "/"; "%"; "/%"; "^" ]

/// Prefix operators, each paired with a snippet that uses it. Unary minus has
/// to be spelled with a space before it and none after: that combination, and
/// only that combination, is negation rather than the binary `-`.
let private prefixOperators = [ "!", "! a"; "fn", "fn a"; "class", "class a"; "-", "f -a" ]

[<Fact>]
let ``every infix operator head is bound to the operator scope`` () =
    for op in infixOperators do
        let expected = $"_{op}_"
        let found = operatorNamesIn $"a {op} b"
        Assert.True(List.contains expected found, $"'a {op} b' did not bind {expected} to the operator scope (found {found})")

[<Fact>]
let ``every prefix operator head is bound to the operator scope`` () =
    for (op, source) in prefixOperators do
        let expected = $"{op}_"
        let found = operatorNamesIn source
        Assert.True(List.contains expected found, $"'{source}' did not bind {expected} to the operator scope (found {found})")

[<Fact>]
let ``the operator scope declares every operator function`` () =
    for op in infixOperators do
        Assert.True(declaredInOperatorScope $"_{op}_", $"operator scope does not declare _{op}_")

    for (op, _) in prefixOperators do
        Assert.True(declaredInOperatorScope $"{op}_", $"operator scope does not declare {op}_")

[<Fact>]
let ``operator heads carry the operator scope as their only candidate`` () =
    // Nothing else may participate in resolving an operator head: it is not a
    // lexical lookup, so no enclosing scope can shadow or intercept it.
    let heads =
        allRoles (parseNode "let x = 1 in x + x")
        |> List.choose (function
            | Referential (candidates, parts) ->
                let name = formatName parts
                if name.StartsWith "_" && name.EndsWith "_" then Some(name, candidates) else None
            | _ -> None)

    Assert.Equal<string list>([ "_in_"; "_=_"; "_+_" ], heads |> List.map fst)

    for (name, candidates) in heads do
        Assert.Equal<Guid list>([ Parser.operatorScope.Id ], candidates)
        Assert.True(declaredInOperatorScope name, $"{name} is not declared in the operator scope")

[<Fact>]
let ``a user declaration of an operator function does not capture operator heads`` () =
    let names = operatorNamesIn "let `_+_` = 1 in 2 + 3"
    Assert.Contains("_+_", names)
    // The declaration is declarative and lexical; the operator head is neither,
    // so both `_+_` occurrences stay independent.
    let roles = allRoles (parseNode "let `_+_` = 1 in 2 + 3")

    let declared =
        roles
        |> List.exists (function
            | Declarative (_, parts) -> formatName parts = "_+_"
            | _ -> false)

    Assert.True(declared, "expected the quoted `_+_` declaration to stay declarative")

[<Fact>]
let ``ordinary identifiers are not bound to the operator scope`` () =
    let roles = allRoles (parseNode "a + b")

    let lexical =
        roles
        |> List.choose (function
            | Referential (candidates, parts) when formatName parts <> "_+_" -> Some candidates
            | _ -> None)

    Assert.NotEmpty(lexical)

    for candidates in lexical do
        Assert.DoesNotContain(Parser.operatorScope.Id, candidates)
