module Parser

open System
open FParsec
open Exp
open ParseResult

// Parser implementation for Ting grammar.
// Uses FParsec's operator precedence parser for infix/prefix handling.

type UserState = unit

type Parser<'t> = FParsec.Primitives.Parser<'t, UserState>

/// Match a literal token without contributing an "expected" entry to error
/// messages. A comment may begin almost anywhere, so letting `//` and `/*` show
/// up in the expectation list of every unrelated parse error is pure noise.
let private skipTokenSilently (s: string) : Parser<unit> =
    fun stream ->
        if stream.Skip(s) then
            Reply(())
        else
            Reply(ReplyStatus.Error, FParsec.Error.NoErrorMessages)

/// `// ...` to the end of the line.
let private lineComment: Parser<unit> = skipTokenSilently "//" >>. skipRestOfLine false

/// `/* ... */`, nesting. Nesting is what makes commenting out a region that
/// already contains a comment work, which is the only reason block comments
/// earn their keep. The closing `*/` is *not* silent: an unterminated block
/// comment should say what it was looking for.
let private blockComment, blockCommentRef = createParserForwardedToRef<unit, UserState>()

blockCommentRef.Value <-
    skipTokenSilently "/*"
    >>. skipManyTill (attempt blockComment <|> skipAnyChar) (skipString "*/")

/// Whitespace, including comments. Comments are whitespace: they may appear
/// anywhere whitespace may, and nowhere else.
let ws: Parser<unit> = spaces >>. skipMany ((lineComment <|> blockComment) >>. spaces)

// Bridge from FParsec's position type to the parser-neutral one in Ting.Ast.
// FParsec positions already carry the stream name, so Range.File is populated
// from whatever name the caller passed to `parse`.
let private toPosition (p: FParsec.Position) : Range.Position =
    { Index = p.Index; Line = p.Line; Column = p.Column }

let private rangeBetween (start: FParsec.Position) (finish: FParsec.Position) : Range.Range =
    { File = (if isNull start.StreamName then "" else start.StreamName)
      Start = toPosition start
      End = toPosition finish }

/// Run a term parser and tag the result with the span it consumed.
///
/// The trailing `ws` is consumed *after* the end position is captured, so a
/// node's range covers exactly its own text and not the whitespace following it.
/// This is why the term cores below must not consume trailing whitespace
/// themselves.
let private located (p: Parser<Exp>) : Parser<Node> =
    pipe3 getPosition p getPosition (fun s e f -> Node.at (rangeBetween s f) e) .>> ws

let keywordCore (s: string) : Parser<unit> =
    let identCont = satisfy (fun c -> System.Char.IsLetterOrDigit c || c = '_')
    attempt (pstring s .>> notFollowedBy identCont) >>% ()

let keyword (s: string) : Parser<unit> = keywordCore s .>> ws

let isIdentStart c = System.Char.IsLetter c || c = '_'
let isIdentCont c = System.Char.IsLetterOrDigit c || c = '_'
let isDoubleQuote c = c = '"'
let isNotDoubleQuote c = c <> '"'

let keywords =
    System.Collections.Generic.HashSet<string>(
        [| "this"; "void"; "let"; "public"; "private"; "internal"; "protected"; "in"; "where"; "catch"; "throw"; "then"; "else"; "true"; "false"; "record"; "records"; "tuple"; "tuples"; "fn"; "class"; "primary"; "candidate" |]
    )

let pIdentifier: Parser<Node> =
    let pPlain =
        many1Satisfy2L isIdentStart isIdentCont "identifier"
        >>= (fun name ->
            if keywords.Contains(name) then
                fail $"'{name}' is a reserved keyword"
            else
                preturn (Exp.Identifier(Unbound [ TextPart name ])))

    let pQuoted =
        between (skipChar '`') (skipChar '`') (many1Satisfy (fun c -> c <> '`'))
        |>> (fun name -> Exp.Identifier(Unbound [ TextPart name ]))

    located (attempt pQuoted <|> pPlain)

/// An integer literal, always unsigned.
///
/// A leading `-` is never part of a literal: it is the negation operator (see
/// `pNegated`). Folding the sign into the literal is what used to make `n-1`
/// parse as `n` applied to `-1` instead of as a subtraction.
let private pUnsignedInt: Parser<int> =
    many1SatisfyL isDigit "integer"
    >>= fun digits ->
        match Int32.TryParse(digits, Globalization.NumberStyles.None, Globalization.CultureInfo.InvariantCulture) with
        | true, value -> preturn value
        | _ -> fail $"integer literal '{digits}' does not fit in a 32-bit integer"

let pNumber: Parser<Node> =
    let pFloatingCore =
        pipe2 (many1Satisfy isDigit .>> skipChar '.') (many1Satisfy isDigit) (fun whole frac -> $"{whole}.{frac}")

    let pDecimal = attempt (pFloatingCore .>> skipChar 'm') |>> (fun s -> Literal(Decimal(decimal (Double.Parse(s, Globalization.CultureInfo.InvariantCulture)))))
    let pFloatSuffix = attempt (pFloatingCore .>> skipChar 'f') |>> (fun s -> Literal(Float32(float (Double.Parse(s, Globalization.CultureInfo.InvariantCulture)))))
    let pFloat = attempt (pFloatingCore .>> notFollowedBy (pchar 'f' <|> pchar 'm')) |>> (fun s -> Literal(Float32(float (Double.Parse(s, Globalization.CultureInfo.InvariantCulture)))))
    let pInt = attempt (pUnsignedInt .>> notFollowedBy (pchar 'f' <|> pchar 'm')) |>> (fun n -> Literal(Int32 n))
    let pIntWithFloatSuffix = attempt (pUnsignedInt .>> skipChar 'f') |>> (fun n -> Literal(Float32(float n)))
    let pIntWithDecimalSuffix = attempt (pUnsignedInt .>> skipChar 'm') |>> (fun n -> Literal(Decimal(decimal n)))
    located (choice [ pDecimal; pFloatSuffix; pIntWithFloatSuffix; pIntWithDecimalSuffix; pFloat; pInt ])

let pBool: Parser<Node> =
    located ((keywordCore "true" >>% Literal(Boolean true)) <|> (keywordCore "false" >>% Literal(Boolean false)))

let pThis: Parser<Node> = located (keywordCore "this" >>% This(Unbound []))
let pVoid: Parser<Node> = located (keywordCore "void" >>% Void)

let expr, exprRef = createParserForwardedToRef<Node, UserState>()

let private exprList: Parser<Node list> =
    let comma = skipChar ',' .>> ws
    sepBy expr comma

let private delimitedExprList (openCh: char) (closeCh: char): Parser<Node list> =
    between (skipChar openCh .>> ws) (skipChar closeCh) exprList

/// The punctuation opening the named section of a product literal.
///
/// `@@` is tried first, so longest-match decides between the two forms. Neither
/// may be followed by `"`: that is the opening of a verbatim string, which is
/// what keeps `@"..."` unambiguous.
let private pNamedSectionMark: Parser<Fusion> =
    let mark (s: string) (f: Fusion) =
        attempt (pstring s >>. notFollowedBy (pchar '"') >>% f)
    (mark "@@" NonFusing <|> mark "@" Fusing) .>> ws

/// A parenthesized product literal.
///
/// Without a named section this is an ordinary tuple, which keeps `(a, b)`
/// exactly as it was. With one, the elements after `@` / `@@` form a declarative
/// section harvested into a fresh names scope, like the operand of `record`.
let private pParensBody: Parser<Exp> =
    pipe2 exprList (opt (pNamedSectionMark .>>. exprList)) (fun positional named ->
        match named with
        | None -> Tuple positional
        | Some (fusion, fields) -> Product(positional, fusion, ExpScopes.createNamesScope(), fields))

// The closing delimiter is inside the range, the whitespace after it is not.
let pParens: Parser<Node> = located (between (skipChar '(' .>> ws) (skipChar ')') pParensBody)
let pBraces: Parser<Node> = located (delimitedExprList '{' '}' |>> Set)
let pBrackets: Parser<Node> = located (delimitedExprList '[' ']' |>> List)

let pString: Parser<Node> =
    located (between (skipSatisfy isDoubleQuote) (skipSatisfy isDoubleQuote) (manySatisfy isNotDoubleQuote) |>> fun s -> Literal(String s))

let pInterpolatedString: Parser<Node> =
    let literalPart: Parser<InterpolatedPart> = many1Satisfy (fun c -> c <> '"' && c <> '{') |>> TextPart
    let exprPart: Parser<InterpolatedPart> = between (skipChar '{') (skipChar '}') expr |>> HolePart
    let part: Parser<InterpolatedPart> = attempt exprPart <|> literalPart
    located (skipChar '$' >>. between (skipSatisfy isDoubleQuote) (skipSatisfy isDoubleQuote) (many part)
             |>> fun parts -> Exp.StringLiteral parts)

let pInterpolatedIdentifier: Parser<Node> =
    let literalPart: Parser<InterpolatedPart> = many1Satisfy (fun c -> c <> '`' && c <> '{') |>> TextPart
    let exprPart: Parser<InterpolatedPart> = between (skipChar '{') (skipChar '}') expr |>> HolePart
    let part: Parser<InterpolatedPart> = attempt exprPart <|> literalPart
    located (skipChar '$' >>. between (skipChar '`') (skipChar '`') (many part)
             |>> fun parts -> Exp.Identifier(Unbound parts))

let private pTermCore: Parser<Node> =
    choice [
        pNumber
        pBool
        pThis
        pVoid
        attempt pInterpolatedIdentifier
        pIdentifier
        pParens
        pBraces
        pBrackets
        pInterpolatedString
        pString
    ]

/// Negation, `-x`. Defined below, once the machinery for synthesizing operator
/// heads exists; forwarded because its operand is a term in its own right.
let private pNegated, pNegatedRef = createParserForwardedToRef<Node, UserState>()

/// A term. Negation is part of the term rather than an operator in the
/// precedence parser, because a negated value has to be usable as a juxtaposed
/// argument: `f -1 -2` passes two negative numbers.
let pTerm: Parser<Node> = pNegated <|> pTermCore

// The operator precedence parser carries the position just past each operator
// token as its "after string" value, which is how an operator's own range is
// recovered for the synthesized operator head.
let opp = new OperatorPrecedenceParser<Node, FParsec.Position, UserState>()
let pExpr: Parser<Node> = opp.ExpressionParser

/// The scope declaring the functions that operators lower to.
///
/// Ting has no operators of its own: every operator is sugar for applying an
/// ordinary function whose name marks the operand positions with `_` — `a + b`
/// is `_+_ (a, b)`, `!a` is `!_ a`. The parser knows every operator it can
/// parse, so it also knows every function those operators can name, and it
/// declares them all here.
///
/// Because this scope is known at parse time, an operator head needs no later
/// binding pass: the parser emits it as a Referential identifier whose single
/// candidate scope is this one. Operator names are therefore never looked up
/// lexically and can never be shadowed by user declarations.
let operatorScope: Scope = LocalScope(Guid.NewGuid()) :> Scope

/// Declare the function an operator lowers to. Called from the operator table
/// below, so the scope cannot drift from the set of operators the parser accepts.
let private declareOperatorFunction (name: string) =
    operatorScope.defineName(Guid.NewGuid(), name, Unknown)

/// Parser used after every operator token: capture the position, then skip
/// whitespace. Capturing before `ws` is what keeps operator ranges tight.
let private opws: Parser<FParsec.Position> = getPosition .>> ws

/// Reconstruct an operator token's range from the position just past it.
/// Operator tokens are literals with no escapes and never span a line, so the
/// start column/index is exactly the end minus the token length.
let private opRange (op: string) (posAfter: FParsec.Position) : Range.Range =
    let len = int64 op.Length
    { File = (if isNull posAfter.StreamName then "" else posAfter.StreamName)
      Start = { Index = posAfter.Index - len; Line = posAfter.Line; Column = posAfter.Column - len }
      End = toPosition posAfter }

// Operator lowerings. Each takes the position just past the operator token so
// the synthesized head reports the operator's own span, while the node as a
// whole spans both operands.
let private operatorFunction (name: string) = Identifier(Referential([ operatorScope.Id ], [ TextPart name ]))

let private operatorHead (op: string) (name: string) (pos: FParsec.Position) =
    Node.at (opRange op pos) (operatorFunction name)

/// Whether a node is the head the parser synthesizes for a given operator.
let private isOperatorHead (name: string) (node: Node) =
    match node.Exp with
    | Identifier (Referential ([ sid ], [ TextPart s ])) -> sid = operatorScope.Id && s = name
    | _ -> false

let declareLeftOperator (op: string) =
    fun pos x y -> Node.spanning x y (DeclareLeft(operatorHead op $"_{op}_" pos, x, y))

let declareRightOperator (op: string) =
    fun pos x y -> Node.spanning x y (DeclareRight(operatorHead op $"_{op}_" pos, x, y))

let referenceLeftOperator (op: string) =
    fun pos x y -> Node.spanning x y (ReferenceLeft(operatorHead op $"_{op}_" pos, x, y))

let referenceRightOperator (op: string) =
    fun pos x y -> Node.spanning x y (ReferenceRight(operatorHead op $"_{op}_" pos, x, y))

let operator (op: string) =
    fun pos x y ->
        Node.spanning x y (Apply(operatorHead op $"_{op}_" pos, Node.spanning x y (Tuple [ x; y ])))

let prefixOperator (op: string) =
    fun pos x ->
        let head = operatorHead op $"{op}_" pos
        Node.at (Range.Range.span (opRange op pos) x.Range) (Apply(head, x))

/// A prefix operator that wraps its operand in a single node spanning the
/// keyword and the operand (`let`, `record`, the access modifiers).
let private prefixWrap (op: string) (make: Node -> Exp) =
    fun pos (x: Node) -> Node.at (Range.Range.span (opRange op pos) x.Range) (make x)

/// The cases of a `fn { p1 -> r1, ... }` case form, as (`->` head, pattern,
/// result) triples. A brace literal qualifies only when every element is a
/// mapping, which is what separates the case form from `fn` applied to an
/// ordinary collection of mappings.
let private tryCaseForm (operand: Node) : (Node * Node * Node) list option =
    let asCase (item: Node) =
        match item.Exp with
        | ReferenceRight (head, pattern, result) when isOperatorHead "_->_" head -> Some(head, pattern, result)
        | _ -> None

    match operand.Exp with
    | Set (_ :: _ as items) ->
        let cases = items |> List.map asCase
        if List.forall Option.isSome cases then
            Some(cases |> List.map Option.get)
        else
            None
    | _ -> None

/// `fn`.
///
/// The case form `fn { p1 -> r1, p2 -> r2, ... }` is a syntactic form of its
/// own: it lowers to the ordered choice of lambdas
/// `(p1 => r1) || (p2 => r2) || ...`, so the cases are tried in order and the
/// first one defined for the argument decides the result. Each case pattern
/// therefore declares, as a lambda parameter does. `fn` applied to anything
/// else stays an ordinary application, converting a collection of mappings into
/// a function; that reading is nondeterministic where mappings overlap, which
/// is exactly where it differs from the case form.
let private fnPrefix (op: string) =
    fun pos (operand: Node) ->
        match tryCaseForm operand with
        | None -> prefixOperator op pos operand
        | Some cases ->
            let lambda (head: Node, pattern: Node, result: Node) =
                let lambdaHead = Node.like head (operatorFunction "_=>_")
                Node.spanning pattern result (DeclareLeft(lambdaHead, pattern, result))

            let orElse (left: Node) (right: Node) =
                let head = Node.synthetic (operatorFunction "_||_")
                Node.spanning left right (Apply(head, Node.spanning left right (Tuple [ left; right ])))

            let chain = cases |> List.map lambda |> List.reduce orElse
            Node.at (Range.Range.span (opRange op pos) operand.Range) chain.Exp

// Registration helpers. `addInfix`/`addPrefix` are for operators that lower to
// something other than a function application (`./`, `.`, `let`, `record`, ...).
let private addInfix (op: string) prec assoc (f: FParsec.Position -> Node -> Node -> Node) =
    opp.AddOperator(InfixOperator(op, opws, prec, assoc, (), f))

let private addPrefix (op: string) prec isAssoc (f: FParsec.Position -> Node -> Node) =
    opp.AddOperator(PrefixOperator(op, opws, prec, isAssoc, (), f))

// Operators that lower to an application of a function go through these, which
// declare that function in operatorScope as a side effect. Registering the
// operator and declaring its function is therefore a single act and the scope
// cannot drift from the grammar.
let private addFunctionInfix (op: string) prec assoc (lower: string -> FParsec.Position -> Node -> Node -> Node) =
    declareOperatorFunction $"_{op}_"
    addInfix op prec assoc (lower op)

let private addFunctionPrefix (op: string) prec (lower: string -> FParsec.Position -> Node -> Node) =
    declareOperatorFunction $"{op}_"
    addPrefix op prec true (lower op)

// Negation. Like every other operator it lowers to a function application, so
// its function is declared in operatorScope alongside the rest.
//
// `-` negates when it has whitespace before it and none after it. Every other
// combination is the binary `-`. So:
//
//   n - 1   n-1   n- 1     subtraction
//   n -1                   `n` applied to the negation of `1`
//   2 + -1                 negation
//
// Two things force this rule. A negative value has to be usable as a juxtaposed
// argument (`f -1 -2`) without stealing the subtraction in `n-1`; and `- x`,
// with a space on both sides of a single operand, is the prefix form of the
// binary operator, which denotes partial application rather than negation.
//
// The rule is purely lexical and admits no exception, so `(-1)` and a leading
// `-1` at the very start of a file are *not* negation: nothing precedes the
// `-`, so it reads as the binary operator and finds no left operand. Write
// `( -1)` and ` -1`.
//
// The whole thing is an `attempt` because the leading `-` of `->` also passes
// the spacing test: `a -> b` gets this far and then fails to find a term, at
// which point the precedence parser reads `->` as the infix operator it is.
declareOperatorFunction "-_"

pNegatedRef.Value <-
    attempt (
        previousCharSatisfies Char.IsWhiteSpace
        >>. skipChar '-'
        >>. notFollowedBy (satisfy Char.IsWhiteSpace)
        >>. getPosition
        .>>. pTerm)
    |>> fun (posAfterMinus, operand) -> prefixOperator "-" posAfterMinus operand

// Juxtaposition application: `f x y` associates left, each Apply spanning from
// the callee's start to the argument's end.
let pApp: Parser<Node> =
    many1 (attempt pTerm)
    |>> List.reduce (fun f x -> Node.spanning f x (Apply(Node.like f (Reference f), x)))

opp.TermParser <- pApp

addPrefix "let" 1 true (prefixWrap "let" (fun b -> Local(ExpScopes.createNamesScope(), b)))
addPrefix "public" 1 true (prefixWrap "public" (fun b -> Publish(ExpScopes.createNamesScope(), Public, b)))
addPrefix "private" 1 true (prefixWrap "private" (fun b -> Publish(ExpScopes.createNamesScope(), Private, b)))
addPrefix "protected" 1 true (prefixWrap "protected" (fun b -> Publish(ExpScopes.createNamesScope(), Protected, b)))
addPrefix "internal" 1 true (prefixWrap "internal" (fun b -> Publish(ExpScopes.createNamesScope(), Internal, b)))

addFunctionInfix "where" 3 Associativity.Right declareRightOperator

addFunctionInfix "in" 5 Associativity.Right referenceRightOperator
addFunctionInfix "=" 10 Associativity.Left referenceRightOperator
// `?=` declares as `=` does, but unifies rather than asserts: the name it
// declares is in scope on the right, which is what makes recursive bindings
// (`let f ?= fn{ ... f ... }`) work. Same precedence as `=`, since it is the
// same kind of construct.
addFunctionInfix "?=" 10 Associativity.Left referenceRightOperator
addFunctionInfix "then" 15 Associativity.Right referenceRightOperator
addFunctionInfix "else" 15 Associativity.Right operator
addFunctionInfix "->" 20 Associativity.Left referenceRightOperator

addFunctionInfix ">>" 30 Associativity.Left operator
addFunctionInfix "<<" 30 Associativity.Left operator
addFunctionInfix "|>" 30 Associativity.Left operator
addFunctionInfix "<|" 30 Associativity.Left operator

addFunctionInfix "=>" 40 Associativity.Right declareLeftOperator

addInfix "./" 70 Associativity.Left (fun _ x y -> Node.spanning x y (PushScope(x, y)))
addInfix "../" 70 Associativity.Left (fun _ x y -> Node.spanning x y (PushMemberScope(x, y)))

addFunctionInfix "catch" 75 Associativity.Left referenceRightOperator
addFunctionInfix @"\" 80 Associativity.Left declareLeftOperator

addFunctionInfix "|" 90 Associativity.Left operator
addFunctionInfix "||" 90 Associativity.Left operator
addFunctionInfix "|||" 90 Associativity.Left operator
addFunctionInfix "&" 100 Associativity.Left operator
addFunctionInfix "&&" 100 Associativity.Left operator
addFunctionInfix "&&&" 100 Associativity.Left operator
addFunctionInfix "?" 103 Associativity.Left referenceRightOperator
addFunctionInfix "??" 103 Associativity.Left referenceRightOperator
addFunctionPrefix "!" 110 prefixOperator

addFunctionInfix ":" 115 Associativity.None referenceRightOperator
addFunctionInfix "::" 115 Associativity.None referenceRightOperator
addFunctionInfix ":::" 115 Associativity.None referenceRightOperator
addFunctionInfix "<" 115 Associativity.None referenceRightOperator
addFunctionInfix "<=" 115 Associativity.None referenceRightOperator
addFunctionInfix ">=" 115 Associativity.None referenceRightOperator
addFunctionInfix ">" 115 Associativity.None referenceRightOperator
addFunctionInfix "==" 115 Associativity.None referenceRightOperator
addFunctionInfix "!=" 115 Associativity.None referenceRightOperator
addFunctionInfix "!:" 115 Associativity.None referenceRightOperator

addFunctionInfix "..." 120 Associativity.None operator
addFunctionInfix "...<" 120 Associativity.None operator
addFunctionInfix "<..." 120 Associativity.None operator
addFunctionInfix "<...<" 120 Associativity.None operator

addFunctionInfix ";" 120 Associativity.Left operator
addFunctionInfix ";;" 120 Associativity.Left operator

addFunctionInfix "^^" 125 Associativity.Right operator
addFunctionInfix "**" 127 Associativity.Left operator
addFunctionInfix "+" 130 Associativity.Left operator
addFunctionInfix "-" 130 Associativity.Left operator
addFunctionInfix "*" 140 Associativity.Left operator
addFunctionInfix "/" 140 Associativity.Left operator
addFunctionInfix "%" 140 Associativity.Left operator
addFunctionInfix "/%" 140 Associativity.Left operator
addFunctionInfix "^" 150 Associativity.Left operator

addPrefix "record" 155 true (prefixWrap "record" (fun b -> Record(ExpScopes.createNamesScope(), b)))
addPrefix "records" 155 true (prefixWrap "records" (fun b -> Set [ Node.like b (Record(ExpScopes.createNamesScope(), b)) ]))
addPrefix "tuple" 155 true (fun _ b -> b)
addPrefix "tuples" 155 true (prefixWrap "tuples" (fun b -> Set [ b ]))
addFunctionPrefix "fn" 155 fnPrefix
addFunctionPrefix "class" 155 prefixOperator

// `throw` is a semantic form, not a function application: it produces no value
// to apply anything to. It binds as tightly as the other prefix keywords, so
// `throw e` is an operand like any other (`x ?? throw e` parses).
addPrefix "throw" 155 true (prefixWrap "throw" Throw)

// The two erasures. Unlike the operators above these do not lower to a function
// application: they are semantic forms in their own right, because the solver
// needs to invert them (`primary a = primary b` must be able to constrain `a`).
// They are keywords rather than properties so that they cannot be shadowed by a
// field of the same name on a programmer-authored record, class or entity.
addPrefix "primary" 155 true (prefixWrap "primary" Primary)
addPrefix "candidate" 155 true (prefixWrap "candidate" Candidate)

addInfix "." 160 Associativity.Left (fun _ a b -> Node.spanning a b (Projection(ExpScopes.createNamesScope(), a, b)))
addInfix ".." 160 Associativity.Left (fun _ a b -> Node.spanning a b (MemberProjection(ExpScopes.createNamesScope(), a, b)))

exprRef := pExpr

let parseExpression: Parser<Node> = ws >>. pExpr .>> eof

/// `streamName` is recorded in every produced Range as its File, so downstream
/// diagnostics can identify the source without threading a filename separately.
let tryParseExpression (streamName: string) (input: string) =
    runParserOnString parseExpression () streamName input

// Parser-neutral entry point. This is the contract the compiler consumes, so
// no FParsec type leaks past this library and the parser can be swapped later.
//
// Parsing produces the analyzed AST directly: the raw parse tree is annotated
// by the Binder before it is returned, so declarative identifiers already
// carry their declaration ids (recorded in the declarative scopes) and
// referential identifiers carry their ordered candidate scope-id lists.
// Every node carries the source range it came from.
// `rootScope` is the outermost (usually builtin-populated) scope, which is the
// last referential candidate for every free name. Operator heads do not go
// through it: they are already bound to `operatorScope`, which callers must
// therefore make known to later passes (see Resolver.resolve).
let parseNamed (rootScope: Scope) (streamName: string) (input: string) : ParseResult =
    match tryParseExpression streamName input with
    | Success(result, _, _) -> ParseSucceeded(Binder.markDeclarations result ([], [ rootScope ]))
    | Failure(errorMsg, error, _) ->
        let position = toPosition error.Position
        ParseFailed
            { Message = errorMsg
              Span = Range.Range.create streamName position position }

let parse (rootScope: Scope) (input: string) : ParseResult =
    parseNamed rootScope "" input
