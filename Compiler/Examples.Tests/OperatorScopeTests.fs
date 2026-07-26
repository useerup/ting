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

let private parseNode (source: string) : Node =
    let rootScope = LocalScope(Guid.Empty) :> Scope
    match Parser.parseNamed rootScope "test.ting" source with
    | ParseResult.ParseSucceeded node -> node
    | ParseResult.ParseFailed err -> failwith $"parse failed for '{source}': {err}"

/// Every binding role in the tree. Written without a wildcard so that adding an
/// Exp case is a compile error here rather than a silently unvisited subtree.
let rec private allRoles (node: Node) : BindingRole list =
    let self =
        match node.Exp with
        | Identifier role
        | This role -> [ role ]
        | _ -> []

    let children =
        match node.Exp with
        | Declare e
        | Reference e
        | Local (_, e)
        | ScopeExp (_, e)
        | Record (_, e)
        | Publish (_, _, e)
        | Throw e
        | IsDefined e -> [ e ]
        | DeclareLeft (a, b, c)
        | DeclareRight (a, b, c)
        | ReferenceLeft (a, b, c)
        | ReferenceRight (a, b, c) -> [ a; b; c ]
        | Apply (a, b)
        | PushScope (a, b)
        | PushMemberScope (a, b)
        | Projection (_, a, b)
        | MemberProjection (_, a, b)
        | Catch (a, b)
        | Equals (a, b)
        | ValueEquals (a, b) -> [ a; b ]
        | Tuple xs
        | Set xs
        | List xs -> xs
        | StringLiteral parts ->
            parts
            |> List.choose (function
                | HolePart e -> Some e
                | TextPart _ -> None)
        | Identifier _
        | This _
        | Literal _
        | Void
        | Unknown -> []

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
    [ "where"; "in"; "="; "then"; "else"; "->"
      ">>"; "<<"; "|>"; "<|"
      "=>"; "catch"; "\\"
      "|"; "||"; "&"; "&&"; "?"; "??"
      ":"; "::"; ":::"; "<"; "<="; ">="; ">"; "=="; "!="; "!:"
      "..."; "...<"; "<..."; "<...<"
      ";"; ";;"
      "^^"; "**"; "+"; "-"; "*"; "/"; "%"; "/%"; "^" ]

let private prefixOperators = [ "!"; "fn"; "class" ]

[<Fact>]
let ``every infix operator head is bound to the operator scope`` () =
    for op in infixOperators do
        let expected = $"_{op}_"
        let found = operatorNamesIn $"a {op} b"
        Assert.True(List.contains expected found, $"'a {op} b' did not bind {expected} to the operator scope (found {found})")

[<Fact>]
let ``every prefix operator head is bound to the operator scope`` () =
    for op in prefixOperators do
        let expected = $"{op}_"
        let found = operatorNamesIn $"{op} a"
        Assert.True(List.contains expected found, $"'{op} a' did not bind {expected} to the operator scope (found {found})")

[<Fact>]
let ``the operator scope declares every operator function`` () =
    for op in infixOperators do
        Assert.True(declaredInOperatorScope $"_{op}_", $"operator scope does not declare _{op}_")

    for op in prefixOperators do
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
