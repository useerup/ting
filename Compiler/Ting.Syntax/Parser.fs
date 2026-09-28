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

    let infixOf (token: Token) =
        match token.Kind with
        | Symbol text -> tryInfix text
        | _ -> None

    let startsAtom (token: Token) =
        match token.Kind with
        | TokenKind.Identifier _
        | TokenKind.Number _ -> true
        | Symbol "(" -> true
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
            | Some info when leftPower info >= minPower ->
                advance () |> ignore
                let right = this.Expr(rightPower info)
                left <- Infix(Span.between left.Span right.Span, operatorOf token info.Text, left, right)
            | _ -> go <- false
        left

    member this.PrefixPosition() : Expr =
        let token = peek ()
        match token.Kind with
        | _ when startsAtom token -> this.Application()
        | _ -> unexpected token "an expression"

    /// Juxtaposition: an atom applied to following atoms, left-associative.
    member this.Application() : Expr =
        let mutable result = this.Atom()
        while startsAtom (peek ()) do
            let argument = this.Atom()
            result <- Apply(Span.between result.Span argument.Span, result, argument)
        result

    member this.Atom() : Expr =
        let token = advance ()
        match token.Kind with
        | TokenKind.Identifier name -> Identifier(token.Span, name)
        | TokenKind.Number(digits, kind) -> Number(token.Span, digits, kind)
        | Symbol "(" ->
            let inner = this.Expr 0
            let close = expectSymbol ")"
            Group(Span.between token.Span close.Span, inner)
        | _ -> unexpected token "an expression"

/// Parses one compilation unit. Stops at the first error.
let parse (text: string) : Result<Expr, SyntaxError> =
    try
        let tokens = Lexer.tokenize (SourceText text) |> Array.ofList
        Ok(Parser(tokens).Complete())
    with SyntaxException error ->
        Error error
