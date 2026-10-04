module Ting.Syntax.Parser

open Source
open Token
open Syntax
open Operators

/// Recursive descent with a binding-power loop for the infix operators.
type private Parser(tokens: Token[]) =
    let mutable position = 0

    let peek () = tokens[position]

    let advance () =
        let token = tokens[position]
        if position < tokens.Length - 1 then position <- position + 1
        token

    let fail (span: Span) (message: string) : 'a =
        raise (SyntaxException { Message = message; Span = span })

    let unexpected (token: Token) (expected: string) : 'a =
        fail token.Span $"unexpected {describe token.Kind}, expected {expected}"

    let isSymbol (text: string) (token: Token) = token.Kind = Symbol text

    let expectSymbol (text: string) =
        let token = peek ()
        if isSymbol text token then advance () else unexpected token $"'{text}'"

    /// `-` with whitespace before it and none after it.
    let isNegation (token: Token) =
        token.Kind = Symbol "-" && token.SpaceBefore && not token.SpaceAfter

    let isProjection (token: Token) = token.Kind = Symbol "." || token.Kind = Symbol ".."

    let infixOf (token: Token) =
        match token.Kind with
        | Symbol text
        | Keyword text -> tryInfix text
        | _ -> None

    let startsAtom (token: Token) =
        match token.Kind with
        | TokenKind.Identifier _
        | TokenKind.InterpolatedIdentifier _
        | TokenKind.Number _
        | TokenKind.String _ -> true
        | Keyword("this" | "void" | "true" | "false") -> true
        | Symbol("(" | "[" | "{" | "{*") -> true
        | Symbol "-" -> isNegation token
        | _ -> false

    let operatorOf (token: Token) (text: string) = { Text = text; Span = token.Span }

    member this.Complete() : Expr =
        let expr = this.Expr 0
        let token = peek ()
        if token.Kind <> EndOfInput then unexpected token "an operator or the end of the expression"
        expr

    member this.Expr(minPower: int) : Expr =
        let mutable left = this.PrefixPosition()
        let mutable go = true
        while go do
            let token = peek ()
            match infixOf token with
            | Some info when leftPower info >= minPower && not (isNegation token) ->
                advance () |> ignore
                let right = this.Expr(rightPower info)
                let mixes (other: InfixInfo) =
                    other.Row = info.Row && (info.Assoc = NonAssoc || other.Assoc <> info.Assoc)
                match right with
                | Infix(_, inner, _, _) when mixes (infixTable[inner.Text]) ->
                    fail inner.Span $"'{info.Text}' and '{inner.Text}' do not associate; add parentheses"
                | _ -> ()
                left <- Infix(Span.between left.Span right.Span, operatorOf token info.Text, left, right)
                let next = peek ()
                match infixOf next with
                | Some nextInfo when mixes nextInfo && not (isNegation next) ->
                    fail next.Span $"'{info.Text}' and '{nextInfo.Text}' do not associate; add parentheses"
                | _ -> ()
            | _ -> go <- false
        left

    member this.PrefixPosition() : Expr =
        let token = peek ()
        match token.Kind with
        | Keyword word when isPrefixKeyword word -> this.PrefixOperator(token, word)
        | Symbol "!" -> this.PrefixOperator(token, "!")
        | _ when startsAtom token -> this.Application()
        | Symbol "-" when not token.SpaceBefore ->
            fail token.Span "'-' without whitespace before it is subtraction, which needs a left operand; write ' -x' for negation"
        | _ ->
            match infixOf token with
            | Some info ->
                advance () |> ignore
                let operand = if isProjection token then this.Postfix() else this.Expr(rightPower info)
                Section(Span.between token.Span operand.Span, operatorOf token info.Text, operand)
            | None -> unexpected token "an expression"

    member this.PrefixOperator(token: Token, text: string) : Expr =
        advance () |> ignore
        let operand = this.Expr(prefixOperandPower text)
        Prefix(Span.between token.Span operand.Span, operatorOf token text, operand)

    /// Juxtaposition: a postfix term applied to following postfix terms, left-associative.
    member this.Application() : Expr =
        let mutable result = this.Postfix()
        while startsAtom (peek ()) do
            let argument = this.Postfix()
            result <- Apply(Span.between result.Span argument.Span, result, argument)
        result

    /// An atom followed by `.`/`..` projections, which bind tighter than juxtaposition.
    member this.Postfix() : Expr =
        let mutable result = this.Atom()
        while isProjection (peek ()) do
            let token = advance ()
            let property = this.Atom()
            let text = if token.Kind = Symbol "." then "." else ".."
            result <- Infix(Span.between result.Span property.Span, operatorOf token text, result, property)
        result

    member this.Atom() : Expr =
        let token = advance ()
        match token.Kind with
        | TokenKind.Identifier name -> Identifier(token.Span, name)
        | TokenKind.InterpolatedIdentifier parts -> InterpolatedIdentifier(token.Span, this.Parts parts)
        | TokenKind.Number(digits, kind) -> Number(token.Span, digits, kind)
        | TokenKind.String parts -> String(token.Span, this.Parts parts)
        | Keyword "this" -> This token.Span
        | Keyword "void" -> Void token.Span
        | Keyword "true" -> Bool(token.Span, true)
        | Keyword "false" -> Bool(token.Span, false)
        | Symbol "-" when isNegation token ->
            let operand = this.Postfix()
            Prefix(Span.between token.Span operand.Span, operatorOf token "-", operand)
        | Symbol "(" -> this.Parens token
        | Symbol "[" ->
            let items, close = this.Items "]"
            List(Span.between token.Span close.Span, items)
        | Symbol "{" ->
            let items, close = this.Items "}"
            Set(Span.between token.Span close.Span, items)
        | Symbol "{*" ->
            let items, close = this.Items "*}"
            Multiset(Span.between token.Span close.Span, items)
        | _ -> unexpected token "an expression"

    /// One or more comma-separated expressions.
    member this.CommaList() : Expr list =
        let items = ResizeArray [ this.Expr 0 ]
        while isSymbol "," (peek ()) do
            advance () |> ignore
            items.Add(this.Expr 0)
        List.ofSeq items

    member this.Items(close: string) : Expr list * Token =
        if isSymbol close (peek ()) then
            [], advance ()
        else
            let items = this.CommaList()
            let token = peek ()
            if isSymbol close token then items, advance () else unexpected token $"',' or '{close}'"

    member this.Parens(openToken: Token) : Expr =
        let positional =
            match (peek ()).Kind with
            | Symbol(")" | "@" | "@@") -> []
            | _ -> this.CommaList()
        let token = advance ()
        match token.Kind, positional with
        | Symbol ")", [ single ] -> Group(Span.between openToken.Span token.Span, single)
        | Symbol ")", items -> Tuple(Span.between openToken.Span token.Span, items)
        | Symbol("@" | "@@" as mark), _ ->
            let fusion = if mark = "@" then Fusing else NonFusing
            let named = this.CommaList()
            let close = expectSymbol ")"
            Product(Span.between openToken.Span close.Span, positional, fusion, named)
        | _ -> unexpected token "',', '@', '@@' or ')'"

    member _.Parts(parts: TokenStringPart list) : StringPart list =
        parts
        |> List.map (function
            | TextPart text -> Text text
            | HolePart holeTokens -> Hole(Parser(Array.ofList holeTokens).Complete()))

/// Parses one compilation unit. Stops at the first error.
let parse (text: string) : Result<Expr, SyntaxError> =
    try
        let tokens = Lexer.tokenize (SourceText text) |> Array.ofList
        Ok(Parser(tokens).Complete())
    with SyntaxException error ->
        Error error
