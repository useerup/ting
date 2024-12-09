// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System

open CoreExp

open type SyntaxExp

// f = let int a -> a + 42 
let lambda = Lambda( Apply(Identifier(Name "int"), Identifier(Name "a")), Apply(Identifier(Name "+"),Tuple [Identifier(Name "a");Literal(IntLiteral 42)]), To)
let exp = Let ( Apply((Identifier(Name "=")), Tuple[Identifier(Name "f"); Reference lambda]))
    

[<EntryPoint>]
let main argv =
    let x = scopeIdentifiers RootScope [] exp in
    Console.WriteLine x
    0
