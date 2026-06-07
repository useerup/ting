module Program

open FParsec
open Parser
open Analyzer
open Exp
open CoreExp

[<EntryPoint>]
let main argv =
    let inputs =
        [
            "x => x * 2m"
            "`then` = 42"
            "a==2 then a==3 then 0 else 1 else 2"
            "DateTime this => $\"{Year}-{Month}-{Day}\""
            "let answer=42 in $`Flollop{answer}`"
            "Invoke true"
            "{ true, false }"
            "public { 1, 2, 3, `a` }"
            "protected { 1, 2, 3, $`a{zark}b` }"
            "protected internal { 1, 2, 3, a }"
            "internal public { 1, 2, 3, a }"
            "let x in y"
            "person.(Name,Age)"
            "let person.Name = \"Zaphod\""
            "person.Name = \"Zaphod\""
            "let x=21 in Answer = x*2"
            "!a in b + c * d"
            "!(let 42 / 6 - 1)"
            "(x + y) * (a - b) in z"
            "()"
            "[]"
            "{}"
            "this"
            "void"
            "{ 1...10 }"
            "invalid$char + 5"
            "int^2 ^^ int"
            "record(Answer:int) \ (Answer = 42)"
            "flollop"
            "$\"{Word name} = {int value}\" => (name->value)"
            "\"zark\" => (name->value)"
            "{ int x \ x % 2 == 0}"
            "x => x + y => x + y + z => x"
            "42./(wunder)"
        ]

    System.Console.Clear()

    for input in inputs do
        printfn "--------------------------------------------------------------------------------------------------------------------"
        printfn $"{input}"
        printfn "--------------------------------------------------------------------------------------------------------------------"

        match tryParseExpression input with
        | Success(result, _, _) ->
            let res = markDeclarations result ([], [])
            printAllScopes res
            printfn "......................................."
            printfn "%s\n\n" (res.ToString())
        | Failure(errorMsg, _, _) ->
            printfn "Error: %s\n" errorMsg

    0
