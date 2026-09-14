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
        | Record (_, _, e)
        | Publish (_, _, _, e)
        | Throw (_, e)
        | Primary (_, e)
        | Candidate (_, e)
        | IsDefined (_, e) -> [ e ]
        | ScopeExp (_, scope, body) ->
            match scope with
            | :? ExpressionScope as expressionScope -> [ expressionScope.Expression; body ]
            | :? ExpressionMemberScope as expressionScope -> [ expressionScope.Expression; body ]
            | _ -> [ body ]
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

let rec private allScopes (node: Exp) : Scope list =
    let self =
        match node with
        | Local (_, s, _)
        | Record (_, s, _)
        | Projection (_, s, _, _)
        | MemberProjection (_, s, _, _)
        | Publish (_, s, _, _) -> [ s ]
        | Product (_, _, _, s, _) -> [ s ]
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

    self @ List.collect allScopes children

let private declaredInOperatorScope (name: string) =
    Parser.operatorScope.Names
    |> Seq.exists (fun kvp -> formatName kvp.Value.NameParts = name)

let private operatorDeclId (name: string) : Guid option =
    Parser.operatorScope.Names
    |> Seq.tryPick (fun kvp -> if formatName kvp.Value.NameParts = name then Some kvp.Key else None)

/// The names bound to the operator scope in a parsed expression.
let private operatorNamesIn (source: string) : string list =
    parseNode source
    |> allRoles
    |> List.choose (function
        | Bound (Referenced, id, parts) when operatorDeclId (formatName parts) = Some id -> Some(formatName parts)
        | _ -> None)

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
let ``operator heads are pre-resolved to the operator scope declarations`` () =
    // Nothing else may participate in resolving an operator head: it is not a
    // lexical lookup, so no enclosing scope can shadow or intercept it.
    let heads =
        allRoles (parseNode "let x = 1 in x + x")
        |> List.choose (function
            | Bound (Referenced, id, parts) ->
                let name = formatName parts
                if name.StartsWith "_" && name.EndsWith "_" then Some(name, id) else None
            | _ -> None)

    Assert.Equal<string list>([ "_in_"; "_=_"; "_+_" ], heads |> List.map fst)

    for (name, id) in heads do
        Assert.Equal(operatorDeclId name, Some id)
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
            | Bound (Declared, _, parts) -> formatName parts = "_+_"
            | _ -> false)

    Assert.True(declared, "expected the quoted `_+_` declaration to stay declarative")

[<Fact>]
let ``ordinary identifiers are not bound to the operator scope`` () =
    let roles = allRoles (parseNode "a + b")

    let lexicalPendings =
        roles
        |> List.choose (function
            | Pending (frames, parts) when formatName parts <> "_+_" ->
                Some(frames |> List.map (fun f -> f.Scope.Id))
            | _ -> None)

    Assert.NotEmpty(lexicalPendings)

    for scopeIds in lexicalPendings do
        Assert.DoesNotContain(Parser.operatorScope.Id, scopeIds)

[<Fact>]
let ``declaration propagation stops at referential barriers`` () =
    let node = parseNode "p where ((q where n) where r)"

    let nDeclarationsAcrossScopes =
        allScopes node
        |> List.sumBy (fun s ->
            s.Names
            |> Seq.filter (fun kvp -> formatName kvp.Value.NameParts = "n")
            |> Seq.length)

    Assert.Equal(1, nDeclarationsAcrossScopes)

let private resolveNode (source: string) : Exp * Resolver.Diagnostic list =
    let rootScope = LocalScope(Guid.Empty) :> Scope
    match Parser.parseNamed rootScope "test.ting" source with
    | ParseResult.ParseSucceeded node -> Resolver.resolve rootScope [ Parser.operatorScope ] node
    | ParseResult.ParseFailed err -> failwith $"parse failed for '{source}': {err}"

let private rolesOf (name: string) (node: Exp) : BindingRole list =
    allRoles node
    |> List.filter (fun role ->
        match role with
        | Unbound p
        | Pending (_, p)
        | Bound (_, _, p) -> formatName p = name)

[<Theory>]
[<InlineData("let (source, records(Field = 1) = source) in source ./ Field")>]
[<InlineData("let (source, source = records(Field = 1)) in source ./ Field")>]
[<InlineData("let (source = alias, alias = source, records(Field = 1) = alias) in source ./ Field")>]
[<InlineData("(let source = records(Field = 1) in source) ./ Field")>]
[<InlineData("let container = record(Source = records(Field = 1)) in container.Source ./ Field")>]
[<InlineData("let (container = record(Source = records(Field = 1)), alias = container.Source) in alias ./ Field")>]
let ``expression scopes derive structure from equality constraints`` (source: string) =
    let resolved, diags = resolveNode source

    Assert.Empty(diags)
    let fields = rolesOf "Field" resolved
    let declarations = fields |> List.choose (function Bound (Declared, id, _) -> Some id | _ -> None)
    let references = fields |> List.choose (function Bound (Referenced, id, _) -> Some id | _ -> None)
    Assert.Single(declarations) |> ignore
    Assert.Single(references) |> ignore
    Assert.Equal(declarations.Head, references.Head)

[<Theory>]
[<InlineData("let (source = alias, alias = source) in source ./ Field")>]
[<InlineData("let (source, target = records(Field = 1)) in (source ./ target) ./ Field")>]
[<InlineData("let (source = records(Left = 1), source = records(Right = 2)) in source ./ Field")>]
let ``undetermined expression scopes retain pending references`` (source: string) =
    let resolved, diags = resolveNode source

    Assert.Empty(diags)
    let pendingFields =
        rolesOf "Field" resolved
        |> List.choose (function Pending (frames, _) -> Some frames | _ -> None)
    Assert.Single(pendingFields) |> ignore
    Assert.NotEmpty(pendingFields.Head)

[<Fact>]
let ``iterative scope resolution reports missing names only once`` () =
    let source = "let (container = record(Source = records(Field = 1)), alias = container.Source) in alias ./ (Field + missing)"
    let _, diags = resolveNode source

    let diagnostic = Assert.Single(diags)
    Assert.Equal(Resolver.UnresolvedName "missing", diagnostic.Kind)

[<Fact>]
let ``references in scope-defining operands are classified`` () =
    let resolved, diags = resolveNode "(let source = records(Field = 1) in source) ./ Field"

    Assert.Empty(diags)
    for role in allRoles resolved do
        match role with
        | Bound _ -> ()
        | _ -> Assert.Fail($"expected a fully classified occurrence, got {role}")

[<Theory>]
[<InlineData("let source = records(Field = 1) in source ./ Field", "Compilation succeeded.")>]
[<InlineData("let source in source ./ Field", "Compilation incomplete:")>]
[<InlineData("let source in source.Field", "Compilation incomplete:")>]
[<InlineData("missing ./ Field", "Compilation failed.")>]
[<InlineData("let source = records(Field = 1) in source ./ missing", "Compilation failed.")>]
let ``compilation reports whether resolution completed`` (source: string, expected: string) =
    Assert.StartsWith(expected, CompilerEngine.compileTingProgram source)

[<Fact>]
let ``a where-bound name is referential in the left operand`` () =
    // `y` left of `where` must resolve to the where-scope declaration, not
    // declare into an enclosing declarative scope.
    let resolved, diags = resolveNode "(y where y = 2) \\ k = 42"

    let ys = rolesOf "y" resolved
    let declared = ys |> List.choose (function Bound (Declared, id, _) -> Some id | _ -> None)
    let referenced = ys |> List.choose (function Bound (Referenced, id, _) -> Some id | _ -> None)

    Assert.Equal(1, List.length declared)
    Assert.Equal(1, List.length referenced)
    Assert.Equal(declared.Head, referenced.Head)

    let unresolvedYs =
        diags
        |> List.filter (fun d ->
            match d.Kind with
            | Resolver.UnresolvedName "y" -> true
            | _ -> false)

    Assert.Empty(unresolvedYs)

[<Fact>]
let ``a lambda body name declares into the enclosing such-that scope`` () =
    // `k` in the body crosses the referential lambda frame and declares in the
    // such-that scope; the guard `k` then references that declaration.
    let resolved, _ = resolveNode "(int x => x * 2 + k) \\ k = 42"

    let ks = rolesOf "k" resolved
    let declared = ks |> List.choose (function Bound (Declared, id, _) -> Some id | _ -> None)
    let referenced = ks |> List.choose (function Bound (Referenced, id, _) -> Some id | _ -> None)

    Assert.Equal(1, List.length declared)
    Assert.Equal(1, List.length referenced)
    Assert.Equal(declared.Head, referenced.Head)

    let xs = rolesOf "x" resolved
    let xDeclared = xs |> List.choose (function Bound (Declared, id, _) -> Some id | _ -> None)
    let xReferenced = xs |> List.choose (function Bound (Referenced, id, _) -> Some id | _ -> None)

    Assert.Equal(1, List.length xDeclared)
    Assert.Equal(1, List.length xReferenced)
    Assert.Equal(xDeclared.Head, xReferenced.Head)

[<Fact>]
let ``an expression scope resolves member names and passes unknown names outward`` () =
    // `Name` must resolve through the scope `person ./ ...` pushes, while
    // `Title` passes through it and declares in the such-that scope.
    let source = "let person = record(Name = \"Bob\") in (person ./ (Title + Name)) \\ Title = \"mr. \""
    let resolved, diags = resolveNode source

    let names = rolesOf "Name" resolved
    let nameDeclared = names |> List.choose (function Bound (Declared, id, _) -> Some id | _ -> None)
    let nameReferenced = names |> List.choose (function Bound (Referenced, id, _) -> Some id | _ -> None)

    // One declaration (the record field), one reference (inside ./), same id.
    Assert.Equal(1, List.length nameDeclared)
    Assert.Equal(1, List.length nameReferenced)
    Assert.Equal(nameDeclared.Head, nameReferenced.Head)

    let titles = rolesOf "Title" resolved
    let titleDeclared = titles |> List.choose (function Bound (Declared, id, _) -> Some id | _ -> None)
    let titleReferenced = titles |> List.choose (function Bound (Referenced, id, _) -> Some id | _ -> None)

    Assert.Equal(1, List.length titleDeclared)
    Assert.Equal(1, List.length titleReferenced)
    Assert.Equal(titleDeclared.Head, titleReferenced.Head)
    Assert.NotEqual<Guid>(nameDeclared.Head, titleDeclared.Head)

    let unresolved =
        diags
        |> List.filter (fun d ->
            match d.Kind with
            | Resolver.UnresolvedName _ -> true
            | _ -> false)

    Assert.Empty(unresolved)
