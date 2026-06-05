module Parser

open System
open System.Collections.Generic
open FParsec
open Exp

// Parser implementation for the language expression grammar.
// Uses FParsec's operator precedence parser for infix/prefix handling.

// user state (unused)
type UserState = unit

type Parser<'t> = FParsec.Primitives.Parser<'t, UserState>

let ws: Parser<unit> = spaces // whitespace to skip between tokens

// basic lexeme helpers
let str s = pstring s
let kw s = str s .>> ws

// keyword with identifier boundary and backtracking
let keyword (s: string) : Parser<unit> =
    let identCont = satisfy (fun c -> System.Char.IsLetterOrDigit c || c = '_')
    attempt (pstring s .>> notFollowedBy identCont) >>% ()
    .>> ws

let isIdentStart c = System.Char.IsLetter c || c = '_'
let isIdentCont c = System.Char.IsLetterOrDigit c || c = '_'
let isDoubleQuote c = c = '"'
let isNotDoubleQuote c = c <> '"'

// Reserved words that cannot be used as plain identifiers.
let keywords = System.Collections.Generic.HashSet<string>(
    [| "this"; "void"; "let"; "public"; "private"; "internal"; "protected"; "in"; "catch"; "throw"; "then"; "else"; "true"; "false"; "record"; "records"; "tuple"; "tuples"; "fn"; "class" |])

// Identifier parser:
// - plain identifiers: foo, _bar1
// - quoted identifiers: `name with spaces`
let pIdentifier: Parser<Exp> =
    let pPlain =
        many1Satisfy2L isIdentStart isIdentCont "identifier"
        >>= (fun name ->
            if keywords.Contains(name) then
                fail $"'{name}' is a reserved keyword"
            else
                preturn (Exp.Identifier [TextPart name]))
    let pQuoted =
        between (skipChar '`') (skipChar '`') (many1Satisfy (fun c -> c <> '`'))
        |>> (fun name -> Exp.Identifier [TextPart name])
    (attempt pQuoted <|> pPlain) .>> ws

// Numeric literal parser with suffix support:
// - 1, 1f, 1m, 1.0, 1.0f, 1.0m
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

// Forward declaration so expression parsers can recursively reference expr.
let expr, exprRef = createParserForwardedToRef<Exp, UserState>()

// Shared parser for a (possibly empty) comma-delimited list of expressions
let private delimitedExprList (openCh: char) (closeCh: char): Parser<Exp list> =
    let comma = skipChar ',' .>> ws
    between (skipChar openCh .>> ws) (skipChar closeCh .>> ws) (sepBy expr comma)

// Parentheses accept a comma-delimited list of expressions: (e1, e2, ...)
// Returns Tuple items (can be empty or any length)
let pParens: Parser<Exp> =
    delimitedExprList '(' ')'
    |>> fun items -> Tuple items

// Braces parse into SetLiteral without special-casing 0 or 1 elements
let pBraces: Parser<Exp> =
    delimitedExprList '{' '}'
    |>> fun items -> Set items

// Brackets parse into ListLiteral without special-casing 0 or 1 elements
let pBrackets: Parser<Exp> =
    delimitedExprList '[' ']'
    |>> fun items -> List items

// Quotes parse into string literal
let pString: Parser<Exp> =
    (between (skipSatisfy isDoubleQuote) (skipSatisfy isDoubleQuote) (manySatisfy isNotDoubleQuote) |>> fun s -> Literal(String s)) .>> ws

// Interpolated string: $"text {expr} text {expr} ..."
// Parses alternating literal segments and {expr} holes into InterpolatedString
let pInterpolatedString: Parser<Exp> =
    let literalPart: Parser<InterpolatedPart> = many1Satisfy (fun c -> c <> '"' && c <> '{') |>> TextPart
    let exprPart: Parser<InterpolatedPart> = between (skipChar '{') (skipChar '}') expr |>> HolePart
    let part: Parser<InterpolatedPart> = attempt exprPart <|> literalPart
    (skipChar '$' >>. between (skipSatisfy isDoubleQuote) (skipSatisfy isDoubleQuote) (many part)
    |>> fun parts -> Exp.InterpolatedString parts) .>> ws

// Interpolated identifier: $`text {expr} text {expr} ...`
// Parses alternating literal segments and {expr} holes into InterpolatedIdentifier
let pInterpolatedIdentifier: Parser<Exp> =
    let literalPart: Parser<InterpolatedPart> = many1Satisfy (fun c -> c <> '`' && c <> '{') |>> TextPart
    let exprPart: Parser<InterpolatedPart> = between (skipChar '{') (skipChar '}') expr |>> HolePart
    let part: Parser<InterpolatedPart> = attempt exprPart <|> literalPart
    (skipChar '$' >>. between (skipChar '`') (skipChar '`') (many part)
    |>> fun parts -> Exp.Identifier parts) .>> ws



// Atomic expression parser used as the basis for application and operator parsing.
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

// Helpers for mapping parsed operators to AST forms.
let declareLeftOperator (op:string) = fun x y -> let s = newScope() in  Local(s, Apply(BuiltIn $"_{op}_", Tuple [Declarative(s,x);Referential(s,y)]))
let declareRightOperator (op:string) = fun x y -> let s =  newScope() in Local(s,  Apply(BuiltIn $"_{op}_", Tuple [Referential(s,x);Declarative(s,y)]))
let referenceRightOperator (op:string) = fun x y -> Apply(BuiltIn $"_{op}_", Tuple [x;Reference y])
let referenceLeftOperator (op:string) = fun x y -> Apply(BuiltIn $"_{op}_", Tuple [Reference x;y])
let operator(op:string) = fun x y -> Apply(BuiltIn $"_{op}_", Tuple [x;y])
let prefixOperator(op:string) = fun x -> Apply(BuiltIn $"{op}_", x)

// Function application by juxtaposition: `f x y` → Apply(Apply(f, x), y)
let pApp: Parser<Exp> =
    many1 (attempt pTerm) |>> List.reduce (fun f x -> Apply(Reference f, x))

opp.TermParser <- pApp

// Prefix declaration/visibility operators.
opp.AddOperator(PrefixOperator("let", ws, 1, true, fun b -> let s = newScope() in Local(s, Declarative(s,b))))
opp.AddOperator(PrefixOperator("public", ws, 1, true, fun b -> let s = newScope() in Publish(s, Public, b)))
opp.AddOperator(PrefixOperator("private", ws, 1, true, fun b -> let s = newScope() in Publish(s, Private, b)))
opp.AddOperator(PrefixOperator("protected", ws, 1, true, fun b -> let s = newScope() in Publish(s, Protected, b)))
opp.AddOperator(PrefixOperator("internal", ws, 1, true, fun b -> let s = newScope() in Publish(s, Internal, b)))

// Control-flow and binding operators.
opp.AddOperator(InfixOperator("in", ws, 5, Associativity.Right, referenceRightOperator "in"))

opp.AddOperator(InfixOperator("=", ws, 10, Associativity.Left, referenceRightOperator "="))

opp.AddOperator(InfixOperator("then", ws, 15, Associativity.Right, referenceRightOperator "then"))
opp.AddOperator(InfixOperator("else", ws, 15, Associativity.Right, operator "else"))

opp.AddOperator(InfixOperator("->", ws, 20, Associativity.Left, referenceRightOperator "->"))

// Composition and pipeline operators.
opp.AddOperator(InfixOperator(">>", ws, 30, Associativity.Left, operator ">>"))
opp.AddOperator(InfixOperator("<<", ws, 30, Associativity.Left, operator "<<"))

opp.AddOperator(InfixOperator("|>", ws, 30, Associativity.Left, operator "|>"))
opp.AddOperator(InfixOperator("<|", ws, 30, Associativity.Left, operator "<|"))

opp.AddOperator(InfixOperator("=>", ws, 40, Associativity.Right, declareLeftOperator "=>" ))

opp.AddOperator(InfixOperator("./", ws, 70, Associativity.Left, fun a b -> let s = ExpressionScope a in Local(s, Referential(s,b))))
opp.AddOperator(InfixOperator("../", ws, 70, Associativity.Left, fun a b -> let s = ExpressionMemberScope a in Local(s,Referential(s,b))))

opp.AddOperator(InfixOperator("catch", ws, 75, Associativity.Left, referenceRightOperator "catch"))
opp.AddOperator(InfixOperator(@"\", ws, 80, Associativity.Left, declareLeftOperator @"\"))

// Logical operators.
opp.AddOperator(InfixOperator("|", ws, 90, Associativity.Left, operator "|"))
opp.AddOperator(InfixOperator("||", ws, 90, Associativity.Left, operator "||"))

opp.AddOperator(InfixOperator("&", ws, 100, Associativity.Left, operator "&"))
opp.AddOperator(InfixOperator("&&", ws, 100, Associativity.Left, operator "&&"))

opp.AddOperator(InfixOperator("?", ws, 103, Associativity.Left, referenceRightOperator "?"))
opp.AddOperator(InfixOperator("??", ws, 103, Associativity.Left, referenceRightOperator "??"))

opp.AddOperator(PrefixOperator("!", ws, 110, true, prefixOperator "!"))

// Comparison and type-relation style operators.
opp.AddOperator(InfixOperator(":", ws, 115, Associativity.None, referenceRightOperator ":"))
opp.AddOperator(InfixOperator("::", ws, 115, Associativity.None, referenceRightOperator "::"))
opp.AddOperator(InfixOperator(":::", ws, 115, Associativity.None, referenceRightOperator ":::"))
opp.AddOperator(InfixOperator("<", ws, 115, Associativity.None, referenceRightOperator "<"))
opp.AddOperator(InfixOperator("<=", ws, 115, Associativity.None, referenceRightOperator "<="))
opp.AddOperator(InfixOperator(">=", ws, 115, Associativity.None, referenceRightOperator ">="))
opp.AddOperator(InfixOperator(">", ws, 115, Associativity.None, referenceRightOperator ">"))
opp.AddOperator(InfixOperator("==", ws, 115, Associativity.None, referenceRightOperator "=="))
opp.AddOperator(InfixOperator("!=", ws, 115, Associativity.None, referenceRightOperator "!="))
opp.AddOperator(InfixOperator("!:", ws, 115, Associativity.None, referenceRightOperator "!:"));

// Range operators.
opp.AddOperator(InfixOperator("...", ws, 120, Associativity.None, operator "..."))
opp.AddOperator(InfixOperator("...<", ws, 120, Associativity.None, operator "...<"))
opp.AddOperator(InfixOperator("<...", ws, 120, Associativity.None, operator "<..."))
opp.AddOperator(InfixOperator("<...<", ws, 120, Associativity.None, operator "<...<"))

// Choice operators
opp.AddOperator(InfixOperator(";", ws, 120, Associativity.Left, operator ";"))
opp.AddOperator(InfixOperator(";;", ws, 120, Associativity.Left, operator ";;"))

// Arithmetic operators by precedence.
opp.AddOperator(InfixOperator("^^", ws, 125, Associativity.Left, operator "^^"))
opp.AddOperator(InfixOperator("**", ws, 127, Associativity.Left, operator "**"))

opp.AddOperator(InfixOperator("+", ws, 130, Associativity.Left, operator "+"))
opp.AddOperator(InfixOperator("-", ws, 130, Associativity.Left, operator "-"))

opp.AddOperator(InfixOperator("*", ws, 140, Associativity.Left, operator "*"))
opp.AddOperator(InfixOperator("/", ws, 140, Associativity.Left, operator "/"))
opp.AddOperator(InfixOperator("%", ws, 140, Associativity.Left, operator "%"))
opp.AddOperator(InfixOperator("/%", ws, 140, Associativity.Left, operator "/%"))

opp.AddOperator(InfixOperator("^", ws, 150, Associativity.Left, operator "^"))

// Structural/type-shaping prefix operators.
opp.AddOperator(PrefixOperator("record", ws, 155, true, fun b -> let s = newScope() in Record(s,[b])))
opp.AddOperator(PrefixOperator("records", ws, 155, true, fun b -> let s = newScope() in Set([Record(s, [b])])))
opp.AddOperator(PrefixOperator("tuple", ws, 155, true, fun b -> Tuple [b]))
opp.AddOperator(PrefixOperator("tuples", ws, 155, true, fun b -> Set([Tuple [b]])))
opp.AddOperator(PrefixOperator("fn", ws, 155, true, prefixOperator "fn"))
opp.AddOperator(PrefixOperator("class", ws, 155, true, prefixOperator "class"))


// Projection operators have the highest precedence.
opp.AddOperator(InfixOperator(".", ws, 160, Associativity.Left, fun  a b -> let s = newLocalScope() in Projection(s,a,Declarative(s, b))))
opp.AddOperator(InfixOperator("..", ws, 160, Associativity.Left, fun a b -> let s = newLocalScope() in MemberProjection(s,a,Declarative(s, b))))

// wire the forward reference for expressions (for parentheses etc.)
exprRef := pExpr

// Public expression parser that skips leading/trailing whitespace and expects EOF
let parseExpression: Parser<Exp> = ws >>. pExpr .>> eof

// Convenience API to run the parser over a string
let tryParseExpression (input: string) = run parseExpression input
