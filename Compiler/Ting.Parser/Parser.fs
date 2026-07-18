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

let keyword (s: string) : Parser<unit> =
    let identCont = satisfy (fun c -> System.Char.IsLetterOrDigit c || c = '_')
    attempt (pstring s .>> notFollowedBy identCont) >>% ()
    .>> ws

let isIdentStart c = System.Char.IsLetter c || c = '_'
let isIdentCont c = System.Char.IsLetterOrDigit c || c = '_'
let isDoubleQuote c = c = '"'
let isNotDoubleQuote c = c <> '"'

let keywords =
    System.Collections.Generic.HashSet<string>(
        [| "this"; "void"; "let"; "public"; "private"; "internal"; "protected"; "in"; "where"; "catch"; "throw"; "then"; "else"; "true"; "false"; "record"; "records"; "tuple"; "tuples"; "fn"; "class" |]
    )

let pIdentifier: Parser<Exp> =
    let pPlain =
        many1Satisfy2L isIdentStart isIdentCont "identifier"
        >>= (fun name ->
            if keywords.Contains(name) then
                fail $"'{name}' is a reserved keyword"
            else
                preturn (Exp.Identifier [ TextPart name ]))

    let pQuoted =
        between (skipChar '`') (skipChar '`') (many1Satisfy (fun c -> c <> '`'))
        |>> (fun name -> Exp.Identifier [ TextPart name ])

    (attempt pQuoted <|> pPlain) .>> ws

let pNumber: Parser<Exp> =
    let pFloatingCore =
        pipe2 (many1Satisfy isDigit .>> skipChar '.') (many1Satisfy isDigit) (fun whole frac -> $"{whole}.{frac}")

    let pDecimal = attempt (pFloatingCore .>> skipChar 'm') |>> (fun s -> Literal(Decimal(decimal (Double.Parse(s, Globalization.CultureInfo.InvariantCulture)))))
    let pFloatSuffix = attempt (pFloatingCore .>> skipChar 'f') |>> (fun s -> Literal(Float32(float (Double.Parse(s, Globalization.CultureInfo.InvariantCulture)))))
    let pFloat = attempt (pFloatingCore .>> notFollowedBy (pchar 'f' <|> pchar 'm')) |>> (fun s -> Literal(Float32(float (Double.Parse(s, Globalization.CultureInfo.InvariantCulture)))))
    let pInt = attempt (pint32 .>> notFollowedBy (pchar 'f' <|> pchar 'm')) |>> (fun n -> Literal(Int32 n))
    let pIntWithFloatSuffix = attempt (pint32 .>> skipChar 'f') |>> (fun n -> Literal(Float32(float n)))
    let pIntWithDecimalSuffix = attempt (pint32 .>> skipChar 'm') |>> (fun n -> Literal(Decimal(decimal n)))
    choice [ pDecimal; pFloatSuffix; pIntWithFloatSuffix; pIntWithDecimalSuffix; pFloat; pInt ] .>> ws

let pBool: Parser<Exp> =
    (keyword "true" >>% Literal(Boolean true)) <|> (keyword "false" >>% Literal(Boolean false))

let pThis: Parser<Exp> = keyword "this" >>% This
let pVoid: Parser<Exp> = keyword "void" >>% Void

let expr, exprRef = createParserForwardedToRef<Exp, UserState>()

let private delimitedExprList (openCh: char) (closeCh: char): Parser<Exp list> =
    let comma = skipChar ',' .>> ws
    between (skipChar openCh .>> ws) (skipChar closeCh .>> ws) (sepBy expr comma)

let pParens: Parser<Exp> = delimitedExprList '(' ')' |>> Tuple
let pBraces: Parser<Exp> = delimitedExprList '{' '}' |>> Set
let pBrackets: Parser<Exp> = delimitedExprList '[' ']' |>> List

let pString: Parser<Exp> =
    (between (skipSatisfy isDoubleQuote) (skipSatisfy isDoubleQuote) (manySatisfy isNotDoubleQuote) |>> fun s -> Literal(String s)) .>> ws

let pInterpolatedString: Parser<Exp> =
    let literalPart: Parser<InterpolatedPart> = many1Satisfy (fun c -> c <> '"' && c <> '{') |>> TextPart
    let exprPart: Parser<InterpolatedPart> = between (skipChar '{') (skipChar '}') expr |>> HolePart
    let part: Parser<InterpolatedPart> = attempt exprPart <|> literalPart
    (skipChar '$' >>. between (skipSatisfy isDoubleQuote) (skipSatisfy isDoubleQuote) (many part)
    |>> fun parts -> Exp.InterpolatedString parts) .>> ws

let pInterpolatedIdentifier: Parser<Exp> =
    let literalPart: Parser<InterpolatedPart> = many1Satisfy (fun c -> c <> '`' && c <> '{') |>> TextPart
    let exprPart: Parser<InterpolatedPart> = between (skipChar '{') (skipChar '}') expr |>> HolePart
    let part: Parser<InterpolatedPart> = attempt exprPart <|> literalPart
    (skipChar '$' >>. between (skipChar '`') (skipChar '`') (many part)
    |>> fun parts -> Exp.Identifier parts) .>> ws

let pTerm: Parser<Exp> =
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

let opp = new OperatorPrecedenceParser<Exp, UserState, unit>()
let pExpr: Parser<Exp> = opp.ExpressionParser

let declareLeftOperator (op: string) = fun x y -> DeclareLeft(BuiltIn $"_{op}_", x, y)
let declareRightOperator (op: string) = fun x y -> DeclareRight(BuiltIn $"_{op}_", x, y)
let referenceLeftOperator (op: string) = fun x y -> ReferenceLeft(BuiltIn $"_{op}_", x, y)
let referenceRightOperator (op: string) = fun x y -> ReferenceRight(BuiltIn $"_{op}_", x, y)

let operator (op: string) = fun x y -> Apply(BuiltIn $"_{op}_", Tuple [ x; y ])

let prefixOperator (op: string) = fun x -> Apply(BuiltIn $"{op}_", x)

let pApp: Parser<Exp> = many1 (attempt pTerm) |>> List.reduce (fun f x -> Apply(Reference f, x))

opp.TermParser <- pApp

opp.AddOperator(PrefixOperator("let", ws, 1, true, fun b -> Declare b))
opp.AddOperator(PrefixOperator("public", ws, 1, true, fun b -> Publish(Public, b)))
opp.AddOperator(PrefixOperator("private", ws, 1, true, fun b -> Publish(Private, b)))
opp.AddOperator(PrefixOperator("protected", ws, 1, true, fun b -> Publish(Protected, b)))
opp.AddOperator(PrefixOperator("internal", ws, 1, true, fun b -> Publish(Internal, b)))

opp.AddOperator(InfixOperator("where", ws, 3, Associativity.Right, declareRightOperator "where"))

opp.AddOperator(InfixOperator("in", ws, 5, Associativity.Right, referenceRightOperator "in"))
opp.AddOperator(InfixOperator("=", ws, 10, Associativity.Left, referenceRightOperator "="))
opp.AddOperator(InfixOperator("then", ws, 15, Associativity.Right, referenceRightOperator "then"))
opp.AddOperator(InfixOperator("else", ws, 15, Associativity.Right, operator "else"))
opp.AddOperator(InfixOperator("->", ws, 20, Associativity.Left, referenceRightOperator "->"))

opp.AddOperator(InfixOperator(">>", ws, 30, Associativity.Left, operator ">>"))
opp.AddOperator(InfixOperator("<<", ws, 30, Associativity.Left, operator "<<"))
opp.AddOperator(InfixOperator("|>", ws, 30, Associativity.Left, operator "|>"))
opp.AddOperator(InfixOperator("<|", ws, 30, Associativity.Left, operator "<|"))

opp.AddOperator(InfixOperator("=>", ws, 40, Associativity.Right, declareLeftOperator "=>"))

opp.AddOperator(InfixOperator("./", ws, 70, Associativity.Left, fun x y -> PushScope(x,y)))
opp.AddOperator(InfixOperator("../", ws, 70, Associativity.Left, fun x y -> PushMemberScope(x,y)))

opp.AddOperator(InfixOperator("catch", ws, 75, Associativity.Left, referenceRightOperator "catch"))
opp.AddOperator(InfixOperator(@"\", ws, 80, Associativity.Left, declareLeftOperator @"\"))

opp.AddOperator(InfixOperator("|", ws, 90, Associativity.Left, operator "|"))
opp.AddOperator(InfixOperator("||", ws, 90, Associativity.Left, operator "||"))
opp.AddOperator(InfixOperator("&", ws, 100, Associativity.Left, operator "&"))
opp.AddOperator(InfixOperator("&&", ws, 100, Associativity.Left, operator "&&"))
opp.AddOperator(InfixOperator("?", ws, 103, Associativity.Left, referenceRightOperator "?"))
opp.AddOperator(InfixOperator("??", ws, 103, Associativity.Left, referenceRightOperator "??"))
opp.AddOperator(PrefixOperator("!", ws, 110, true, prefixOperator "!"))

opp.AddOperator(InfixOperator(":", ws, 115, Associativity.None, referenceRightOperator ":"))
opp.AddOperator(InfixOperator("::", ws, 115, Associativity.None, referenceRightOperator "::"))
opp.AddOperator(InfixOperator(":::", ws, 115, Associativity.None, referenceRightOperator ":::"))
opp.AddOperator(InfixOperator("<", ws, 115, Associativity.None, referenceRightOperator "<"))
opp.AddOperator(InfixOperator("<=", ws, 115, Associativity.None, referenceRightOperator "<="))
opp.AddOperator(InfixOperator(">=", ws, 115, Associativity.None, referenceRightOperator ">="))
opp.AddOperator(InfixOperator(">", ws, 115, Associativity.None, referenceRightOperator ">"))
opp.AddOperator(InfixOperator("==", ws, 115, Associativity.None, referenceRightOperator "=="))
opp.AddOperator(InfixOperator("!=", ws, 115, Associativity.None, referenceRightOperator "!="))
opp.AddOperator(InfixOperator("!:", ws, 115, Associativity.None, referenceRightOperator "!:"))

opp.AddOperator(InfixOperator("...", ws, 120, Associativity.None, operator "..."))
opp.AddOperator(InfixOperator("...<", ws, 120, Associativity.None, operator "...<"))
opp.AddOperator(InfixOperator("<...", ws, 120, Associativity.None, operator "<..."))
opp.AddOperator(InfixOperator("<...<", ws, 120, Associativity.None, operator "<...<"))

opp.AddOperator(InfixOperator(";", ws, 120, Associativity.Left, operator ";"))
opp.AddOperator(InfixOperator(";;", ws, 120, Associativity.Left, operator ";;"))

opp.AddOperator(InfixOperator("^^", ws, 125, Associativity.Left, operator "^^"))
opp.AddOperator(InfixOperator("**", ws, 127, Associativity.Left, operator "**"))
opp.AddOperator(InfixOperator("+", ws, 130, Associativity.Left, operator "+"))
opp.AddOperator(InfixOperator("-", ws, 130, Associativity.Left, operator "-"))
opp.AddOperator(InfixOperator("*", ws, 140, Associativity.Left, operator "*"))
opp.AddOperator(InfixOperator("/", ws, 140, Associativity.Left, operator "/"))
opp.AddOperator(InfixOperator("%", ws, 140, Associativity.Left, operator "%"))
opp.AddOperator(InfixOperator("/%", ws, 140, Associativity.Left, operator "/%"))
opp.AddOperator(InfixOperator("^", ws, 150, Associativity.Left, operator "^"))

opp.AddOperator(PrefixOperator("record", ws, 155, true, fun b -> Record b))
opp.AddOperator(PrefixOperator("records", ws, 155, true, fun b -> Set([ Record b ])))
opp.AddOperator(PrefixOperator("tuple", ws, 155, true, fun b -> b))
opp.AddOperator(PrefixOperator("tuples", ws, 155, true, fun b -> Set [b]))
opp.AddOperator(PrefixOperator("fn", ws, 155, true, prefixOperator "fn"))
opp.AddOperator(PrefixOperator("class", ws, 155, true, prefixOperator "class"))

opp.AddOperator(InfixOperator(".", ws, 160, Associativity.Left, fun a b -> Projection(a,b)))
opp.AddOperator(InfixOperator("..", ws, 160, Associativity.Left, fun a b -> MemberProjection(a,b)))

exprRef := pExpr

let parseExpression: Parser<Exp> = ws >>. pExpr .>> eof
let tryParseExpression (input: string) = run parseExpression input

// Parser-neutral entry point. This is the contract the compiler consumes, so
// no FParsec type leaks past this library and the parser can be swapped later.
let parse (input: string) : ParseResult =
    match tryParseExpression input with
    | Success(result, _, _) -> ParseSucceeded result
    | Failure(errorMsg, _, _) -> ParseFailed errorMsg
