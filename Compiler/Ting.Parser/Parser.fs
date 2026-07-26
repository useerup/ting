module Parser

open System
open FParsec
open Exp
open ParseResult

// Parser implementation for Ting grammar.
// Uses FParsec's operator precedence parser for infix/prefix handling.

type UserState = unit

type Parser<'t> = FParsec.Primitives.Parser<'t, UserState>

let ws: Parser<unit> = spaces

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
        [| "this"; "void"; "let"; "public"; "private"; "internal"; "protected"; "in"; "where"; "catch"; "throw"; "then"; "else"; "true"; "false"; "record"; "records"; "tuple"; "tuples"; "fn"; "class" |]
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

let pNumber: Parser<Node> =
    let pFloatingCore =
        pipe2 (many1Satisfy isDigit .>> skipChar '.') (many1Satisfy isDigit) (fun whole frac -> $"{whole}.{frac}")

    let pDecimal = attempt (pFloatingCore .>> skipChar 'm') |>> (fun s -> Literal(Decimal(decimal (Double.Parse(s, Globalization.CultureInfo.InvariantCulture)))))
    let pFloatSuffix = attempt (pFloatingCore .>> skipChar 'f') |>> (fun s -> Literal(Float32(float (Double.Parse(s, Globalization.CultureInfo.InvariantCulture)))))
    let pFloat = attempt (pFloatingCore .>> notFollowedBy (pchar 'f' <|> pchar 'm')) |>> (fun s -> Literal(Float32(float (Double.Parse(s, Globalization.CultureInfo.InvariantCulture)))))
    let pInt = attempt (pint32 .>> notFollowedBy (pchar 'f' <|> pchar 'm')) |>> (fun n -> Literal(Int32 n))
    let pIntWithFloatSuffix = attempt (pint32 .>> skipChar 'f') |>> (fun n -> Literal(Float32(float n)))
    let pIntWithDecimalSuffix = attempt (pint32 .>> skipChar 'm') |>> (fun n -> Literal(Decimal(decimal n)))
    located (choice [ pDecimal; pFloatSuffix; pIntWithFloatSuffix; pIntWithDecimalSuffix; pFloat; pInt ])

let pBool: Parser<Node> =
    located ((keywordCore "true" >>% Literal(Boolean true)) <|> (keywordCore "false" >>% Literal(Boolean false)))

let pThis: Parser<Node> = located (keywordCore "this" >>% This(Unbound []))
let pVoid: Parser<Node> = located (keywordCore "void" >>% Void)

let expr, exprRef = createParserForwardedToRef<Node, UserState>()

let private delimitedExprList (openCh: char) (closeCh: char): Parser<Node list> =
    let comma = skipChar ',' .>> ws
    between (skipChar openCh .>> ws) (skipChar closeCh) (sepBy expr comma)

// The closing delimiter is inside the range, the whitespace after it is not.
let pParens: Parser<Node> = located (delimitedExprList '(' ')' |>> Tuple)
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

let pTerm: Parser<Node> =
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

// The operator precedence parser carries the position just past each operator
// token as its "after string" value, which is how an operator's own range is
// recovered for the synthesized BuiltIn head.
let opp = new OperatorPrecedenceParser<Node, FParsec.Position, UserState>()
let pExpr: Parser<Node> = opp.ExpressionParser

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
// the synthesized BuiltIn head reports the operator's own span, while the node
// as a whole spans both operands.
let private builtInNode (op: string) (name: string) (pos: FParsec.Position) =
    Node.at (opRange op pos) (BuiltIn name)

let declareLeftOperator (op: string) =
    fun pos x y -> Node.spanning x y (DeclareLeft(builtInNode op $"_{op}_" pos, x, y))

let declareRightOperator (op: string) =
    fun pos x y -> Node.spanning x y (DeclareRight(builtInNode op $"_{op}_" pos, x, y))

let referenceLeftOperator (op: string) =
    fun pos x y -> Node.spanning x y (ReferenceLeft(builtInNode op $"_{op}_" pos, x, y))

let referenceRightOperator (op: string) =
    fun pos x y -> Node.spanning x y (ReferenceRight(builtInNode op $"_{op}_" pos, x, y))

let operator (op: string) =
    fun pos x y ->
        Node.spanning x y (Apply(builtInNode op $"_{op}_" pos, Node.spanning x y (Tuple [ x; y ])))

let prefixOperator (op: string) =
    fun pos x ->
        let head = builtInNode op $"{op}_" pos
        Node.at (Range.Range.span (opRange op pos) x.Range) (Apply(head, x))

/// A prefix operator that wraps its operand in a single node spanning the
/// keyword and the operand (`let`, `record`, the access modifiers).
let private prefixWrap (op: string) (make: Node -> Exp) =
    fun pos (x: Node) -> Node.at (Range.Range.span (opRange op pos) x.Range) (make x)

let private addInfix (op: string) prec assoc (f: FParsec.Position -> Node -> Node -> Node) =
    opp.AddOperator(InfixOperator(op, opws, prec, assoc, (), f))

let private addPrefix (op: string) prec isAssoc (f: FParsec.Position -> Node -> Node) =
    opp.AddOperator(PrefixOperator(op, opws, prec, isAssoc, (), f))

// Juxtaposition application: `f x y` associates left, each Apply spanning from
// the callee's start to the argument's end.
let pApp: Parser<Node> =
    many1 (attempt pTerm)
    |>> List.reduce (fun f x -> Node.spanning f x (Apply(Node.like f (Reference f), x)))

opp.TermParser <- pApp

addPrefix "let" 1 true (prefixWrap "let" Declare)
addPrefix "public" 1 true (prefixWrap "public" (fun b -> Publish(ExpScopes.createNamesScope(), Public, b)))
addPrefix "private" 1 true (prefixWrap "private" (fun b -> Publish(ExpScopes.createNamesScope(), Private, b)))
addPrefix "protected" 1 true (prefixWrap "protected" (fun b -> Publish(ExpScopes.createNamesScope(), Protected, b)))
addPrefix "internal" 1 true (prefixWrap "internal" (fun b -> Publish(ExpScopes.createNamesScope(), Internal, b)))

addInfix "where" 3 Associativity.Right (declareRightOperator "where")

addInfix "in" 5 Associativity.Right (referenceRightOperator "in")
addInfix "=" 10 Associativity.Left (referenceRightOperator "=")
addInfix "then" 15 Associativity.Right (referenceRightOperator "then")
addInfix "else" 15 Associativity.Right (operator "else")
addInfix "->" 20 Associativity.Left (referenceRightOperator "->")

addInfix ">>" 30 Associativity.Left (operator ">>")
addInfix "<<" 30 Associativity.Left (operator "<<")
addInfix "|>" 30 Associativity.Left (operator "|>")
addInfix "<|" 30 Associativity.Left (operator "<|")

addInfix "=>" 40 Associativity.Right (declareLeftOperator "=>")

addInfix "./" 70 Associativity.Left (fun _ x y -> Node.spanning x y (PushScope(x, y)))
addInfix "../" 70 Associativity.Left (fun _ x y -> Node.spanning x y (PushMemberScope(x, y)))

addInfix "catch" 75 Associativity.Left (referenceRightOperator "catch")
addInfix @"\" 80 Associativity.Left (declareLeftOperator @"\")

addInfix "|" 90 Associativity.Left (operator "|")
addInfix "||" 90 Associativity.Left (operator "||")
addInfix "&" 100 Associativity.Left (operator "&")
addInfix "&&" 100 Associativity.Left (operator "&&")
addInfix "?" 103 Associativity.Left (referenceRightOperator "?")
addInfix "??" 103 Associativity.Left (referenceRightOperator "??")
addPrefix "!" 110 true (prefixOperator "!")

addInfix ":" 115 Associativity.None (referenceRightOperator ":")
addInfix "::" 115 Associativity.None (referenceRightOperator "::")
addInfix ":::" 115 Associativity.None (referenceRightOperator ":::")
addInfix "<" 115 Associativity.None (referenceRightOperator "<")
addInfix "<=" 115 Associativity.None (referenceRightOperator "<=")
addInfix ">=" 115 Associativity.None (referenceRightOperator ">=")
addInfix ">" 115 Associativity.None (referenceRightOperator ">")
addInfix "==" 115 Associativity.None (referenceRightOperator "==")
addInfix "!=" 115 Associativity.None (referenceRightOperator "!=")
addInfix "!:" 115 Associativity.None (referenceRightOperator "!:")

addInfix "..." 120 Associativity.None (operator "...")
addInfix "...<" 120 Associativity.None (operator "...<")
addInfix "<..." 120 Associativity.None (operator "<...")
addInfix "<...<" 120 Associativity.None (operator "<...<")

addInfix ";" 120 Associativity.Left (operator ";")
addInfix ";;" 120 Associativity.Left (operator ";;")

addInfix "^^" 125 Associativity.Left (operator "^^")
addInfix "**" 127 Associativity.Left (operator "**")
addInfix "+" 130 Associativity.Left (operator "+")
addInfix "-" 130 Associativity.Left (operator "-")
addInfix "*" 140 Associativity.Left (operator "*")
addInfix "/" 140 Associativity.Left (operator "/")
addInfix "%" 140 Associativity.Left (operator "%")
addInfix "/%" 140 Associativity.Left (operator "/%")
addInfix "^" 150 Associativity.Left (operator "^")

addPrefix "record" 155 true (prefixWrap "record" (fun b -> Record(ExpScopes.createNamesScope(), b)))
addPrefix "records" 155 true (prefixWrap "records" (fun b -> Set [ Node.like b (Record(ExpScopes.createNamesScope(), b)) ]))
addPrefix "tuple" 155 true (fun _ b -> b)
addPrefix "tuples" 155 true (prefixWrap "tuples" (fun b -> Set [ b ]))
addPrefix "fn" 155 true (prefixOperator "fn")
addPrefix "class" 155 true (prefixOperator "class")

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
// last referential candidate for every free name.
let parseNamed (rootScope: Scope) (streamName: string) (input: string) : ParseResult =
    match tryParseExpression streamName input with
    | Success(result, _, _) -> ParseSucceeded(Binder.markDeclarations rootScope result ([], [ rootScope ]))
    | Failure(errorMsg, _, _) -> ParseFailed errorMsg

let parse (rootScope: Scope) (input: string) : ParseResult =
    parseNamed rootScope "" input
