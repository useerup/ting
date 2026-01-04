open FParsec
open Exp

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

let pIdentifier: Parser<Exp> =
    many1Satisfy2L isIdentStart isIdentCont "identifier"
    |>> fun name -> Exp.Identifier (Unresolved name)
    .>> ws

let pNumber: Parser<Exp> = pint32 .>> ws |>> fun n -> Number n

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
    |>> fun items -> SetLiteral items

// Brackets parse into ListLiteral without special-casing 0 or 1 elements
let pBrackets: Parser<Exp> =
    delimitedExprList '[' ']'
    |>> fun items -> ListLiteral items

// Quotes parse into string literal 
let pStringLiteral: Parser<Exp> =
    between (skipSatisfy isDoubleQuote) (skipSatisfy isDoubleQuote) (manySatisfy isNotDoubleQuote) |>> StringLiteral .>> ws

let pExportType: Parser<ExportType> =
    choice [
        keyword "public" >>% ExportType.Public
        keyword "private" >>% ExportType.Private
        keyword "internal" >>% ExportType.Internal
        keyword "protected" >>% ExportType.Protected
        keyword "local" >>% ExportType.Local
    ]

let pExport: Parser<Exp> =
    pExportType >>= fun exportType ->
        expr
        |>> fun operand ->
            let s = LocalScope (System.Guid.NewGuid())
            Export(exportType, s, Declare(s, operand))



let pTerm: Parser<Exp> = choice [ pNumber; pExport; pIdentifier; pParens; pBraces; pBrackets; pStringLiteral ]

let opp = new OperatorPrecedenceParser<Exp, UserState, unit>()
let pExpr: Parser<Exp> = opp.ExpressionParser

let rootIdentifier s = Identifier(Resolved(s, RootScope))

let newScope() : Scope = LocalScope(System.Guid.NewGuid())

let declare (exp:Exp) = let scope = newScope() in Scoped(scope, Declare(scope, exp)) 
let declareLeftOperator (s:string) = let scope = newScope() in fun x y -> Scoped(scope, Apply(rootIdentifier $"_{s}_", Tuple [Declare(scope,x);Reference(scope,y)]))
let referenceRightOperator (s:string) = fun x y -> Apply(rootIdentifier $"_{s}_", Tuple [x;Referential(y)])

let operator(s:string) = fun x y -> Apply(rootIdentifier $"_{s}_", Tuple [x;y])
let prefixOperator(s:string) = fun x -> Apply(rootIdentifier $"{s}_", x)

opp.TermParser <- pTerm

opp.AddOperator(PrefixOperator("let", ws, 1, true, fun b -> declare b))

opp.AddOperator(InfixOperator("in", ws, 2, Associativity.Right, referenceRightOperator "in"))

opp.AddOperator(InfixOperator("=", ws, 10, Associativity.Left, referenceRightOperator "="))

opp.AddOperator(InfixOperator("->", ws, 20, Associativity.Left, declareLeftOperator "->"))

opp.AddOperator(InfixOperator(">>", ws, 30, Associativity.Left, operator ">>"))
opp.AddOperator(InfixOperator("<<", ws, 30, Associativity.Left, operator "<<"))

opp.AddOperator(InfixOperator("|>", ws, 30, Associativity.Left, operator "|>"))
opp.AddOperator(InfixOperator("<|", ws, 30, Associativity.Left, operator "<|"))

opp.AddOperator(InfixOperator("~", ws, 35, Associativity.Left, declareLeftOperator "~"))

opp.AddOperator(InfixOperator("=>", ws, 40, Associativity.Right, declareLeftOperator "=>"))

opp.AddOperator(InfixOperator("./", ws, 70, Associativity.Left, operator "./"))
opp.AddOperator(InfixOperator("../", ws, 70, Associativity.Left, operator "../"))

opp.AddOperator(InfixOperator("?", ws, 80, Associativity.Left, operator "?"))
opp.AddOperator(InfixOperator("??", ws, 80, Associativity.Left, operator "??"))
opp.AddOperator(InfixOperator("?!", ws, 80, Associativity.Left, operator "?!"))
opp.AddOperator(InfixOperator("??!", ws, 80, Associativity.Left, operator "??!"))
opp.AddOperator(InfixOperator("catch", ws, 80, Associativity.Left, operator "catch"))
opp.AddOperator(InfixOperator(@"\", ws, 80, Associativity.Left, declareLeftOperator @"\"))

opp.AddOperator(InfixOperator("|", ws, 90, Associativity.Left, operator "|"))
opp.AddOperator(InfixOperator("||", ws, 90, Associativity.Left, operator "||"))

opp.AddOperator(InfixOperator("&", ws, 100, Associativity.Left, operator "&"))
opp.AddOperator(InfixOperator("&&", ws, 100, Associativity.Left, operator "&&"))

opp.AddOperator(PrefixOperator("!", ws, 110, true, prefixOperator "!"))

opp.AddOperator(InfixOperator(":", ws, 115, Associativity.Left, referenceRightOperator ":"))
opp.AddOperator(InfixOperator("::", ws, 115, Associativity.Left, referenceRightOperator "::"))
opp.AddOperator(InfixOperator(":::", ws, 115, Associativity.Left, referenceRightOperator ":::"))
opp.AddOperator(InfixOperator("<", ws, 115, Associativity.Left, referenceRightOperator "<"))
opp.AddOperator(InfixOperator("<=", ws, 115, Associativity.None, referenceRightOperator "<="))
opp.AddOperator(InfixOperator(">=", ws, 115, Associativity.None, referenceRightOperator ">="))
opp.AddOperator(InfixOperator(">", ws, 115, Associativity.None, referenceRightOperator ">"))
opp.AddOperator(InfixOperator("==", ws, 115, Associativity.Left, referenceRightOperator "=="))
opp.AddOperator(InfixOperator("!=", ws, 115, Associativity.Left, referenceRightOperator "!="))
opp.AddOperator(InfixOperator("!:", ws, 115, Associativity.None, referenceRightOperator "!:"))

opp.AddOperator(InfixOperator("...", ws, 120, Associativity.Left, operator "..."))
opp.AddOperator(InfixOperator("...<", ws, 120, Associativity.Left, operator "...<"))
opp.AddOperator(InfixOperator("<...", ws, 120, Associativity.Left, operator "<..."))
opp.AddOperator(InfixOperator("<...<", ws, 120, Associativity.Left, operator "<...<"))
opp.AddOperator(InfixOperator(";", ws, 120, Associativity.Left, operator ";"))
opp.AddOperator(InfixOperator(";;", ws, 120, Associativity.Left, operator ";;"))

opp.AddOperator(InfixOperator("+", ws, 130, Associativity.Left, operator "+"))
opp.AddOperator(InfixOperator("-", ws, 130, Associativity.Left, operator "-"))

opp.AddOperator(InfixOperator("*", ws, 140, Associativity.Left, operator "*"))
opp.AddOperator(InfixOperator("/", ws, 140, Associativity.Left, operator "/"))
opp.AddOperator(InfixOperator("%", ws, 140, Associativity.Left, operator "%"))
opp.AddOperator(InfixOperator("/%", ws, 140, Associativity.Left, operator "/%"))

opp.AddOperator(InfixOperator("^", ws, 150, Associativity.Left, operator "^"))

opp.AddOperator(InfixOperator(".", ws, 160, Associativity.Left, operator "."))
opp.AddOperator(InfixOperator("..", ws, 160, Associativity.Left, operator ".."))



// wire the forward reference for expressions (for parentheses etc.)
exprRef := pExpr

// Public expression parser that skips leading/trailing whitespace and expects EOF
let parseExpression: Parser<Exp> = ws >>. pExpr .>> eof

// Convenience API to run the parser over a string
let tryParseExpression (input: string) = run parseExpression input



[<EntryPoint>]
let main argv =
    let inputs = 
        [ 
          "public { 1, 2, 3 }"
          "protected { 1, 2, 3 }"
          "protected internal { 1, 2, 3 }"
          "internal public { 1, 2, 3 }"
          "let x in y"
          "person.(Name,Age)"  
          "let person.Name = \"Zaphod\""
          "let x=21 in Answer = x*2"
          "!a in b + c * d"
          "!(let 42 / 6 - 1)"
          "(x + y) * (a - b) in z"
          "invalid$char + 5" // This input contains an invalid character
        ] 
    for input in inputs do
        match tryParseExpression input with
        | Success(result, _, _) ->
            printfn $"Input: {input}\nParsed: {formatExp result}\n" 
        | Failure(errorMsg, _, _) ->
            printfn "Input: %s\nError: %s\n" input errorMsg
    0 // return an integer exit code
