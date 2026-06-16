module Program

open System
open System.IO
open FParsec
open Parser
open Binder
open CoreExp
open CoreExpDiagnostics
open CoreExpScopes

[<EntryPoint>]
let main argv =
    let examplesDir = Path.Combine(__SOURCE_DIRECTORY__, "examples")
    let outputDir = Path.Combine(examplesDir, "out")

    if not (Directory.Exists examplesDir) then
        Directory.CreateDirectory examplesDir |> ignore

    if not (Directory.Exists outputDir) then
        Directory.CreateDirectory outputDir |> ignore

    for outFile in Directory.GetFiles(outputDir, "*.out") do
        File.Delete(outFile)

    let exampleFiles =
        Directory.GetFiles(examplesDir, "*.ting")
        |> Array.sort

    Console.Clear()

    for exampleFile in exampleFiles do
        let fileName = Path.GetFileName(exampleFile)
        let input = File.ReadAllText(exampleFile)

        let outputFileName = Path.ChangeExtension(fileName, ".out")
        let outputPath = Path.Combine(outputDir, outputFileName)

        let output =
            match tryParseExpression input with
            | Success(result, _, _) ->
                let rootScope = CoreExp.LocalScope(Guid.Empty) :> CoreExp.Scope
                rootScope.defineName(Guid.NewGuid(), "_=>_", Unknown)
                rootScope.defineName(Guid.NewGuid(), "_==_", Unknown)
                rootScope.defineName(Guid.NewGuid(), "_=_", Unknown)
                rootScope.defineName(Guid.NewGuid(), "_&_", Unknown)
                rootScope.defineName(Guid.NewGuid(), "_|_", Unknown)
                rootScope.defineName(Guid.NewGuid(), "_&&_", Unknown)
                rootScope.defineName(Guid.NewGuid(), "_||_", Unknown)
                rootScope.defineName(Guid.NewGuid(), "_&&&_", Unknown)
                rootScope.defineName(Guid.NewGuid(), "_|||_", Unknown)

                rootScope.defineName(Guid.NewGuid(), "bool", Unknown)

                rootScope.defineName(Guid.NewGuid(), "string", Unknown)
                rootScope.defineName(Guid.NewGuid(), "char", Unknown)

                rootScope.defineName(Guid.NewGuid(), "float", Unknown)
                rootScope.defineName(Guid.NewGuid(), "double", Unknown)

                rootScope.defineName(Guid.NewGuid(), "int", Unknown)
                rootScope.defineName(Guid.NewGuid(), "uint", Unknown)
                rootScope.defineName(Guid.NewGuid(), "byte", Unknown)
                rootScope.defineName(Guid.NewGuid(), "sbyte", Unknown)
                rootScope.defineName(Guid.NewGuid(), "short", Unknown)
                rootScope.defineName(Guid.NewGuid(), "ushort", Unknown)
                rootScope.defineName(Guid.NewGuid(), "long", Unknown)
                rootScope.defineName(Guid.NewGuid(), "ulong", Unknown)
                rootScope.defineName(Guid.NewGuid(), "half", Unknown)

                rootScope.defineName(Guid.NewGuid(), "decimal", Unknown)

                rootScope.defineName(Guid.NewGuid(), "DateTime", Unknown)
                rootScope.defineName(Guid.NewGuid(), "DateTimeOffset", Unknown)

                rootScope.defineName(Guid.NewGuid(), "set", Unknown)
                rootScope.defineName(Guid.NewGuid(), "any", Unknown)
                rootScope.defineName(Guid.NewGuid(), "Func", Unknown)

                rootScope.defineName(Guid.NewGuid(), "_all_", Unknown)

                rootScope.defineName(Guid.NewGuid(), "_in_", Unknown)
                rootScope.defineName(Guid.NewGuid(), "_+_", Unknown)
                rootScope.defineName(Guid.NewGuid(), "_-_", Unknown)
                rootScope.defineName(Guid.NewGuid(), "_*_", Unknown)
                rootScope.defineName(Guid.NewGuid(), "_/_", Unknown)
                rootScope.defineName(Guid.NewGuid(), "_%_", Unknown)
                rootScope.defineName(Guid.NewGuid(), "_/%_", Unknown)
                rootScope.defineName(Guid.NewGuid(), "_^_", Unknown)
                rootScope.defineName(Guid.NewGuid(), "_->_", Unknown)
                rootScope.defineName(Guid.NewGuid(), "_^^_", Unknown)
                rootScope.defineName(Guid.NewGuid(), "_^_", Unknown)
                rootScope.defineName(Guid.NewGuid(), "_then_", Unknown)
                rootScope.defineName(Guid.NewGuid(), "_else_", Unknown)
                rootScope.defineName(Guid.NewGuid(), "_..._", Unknown)
                rootScope.defineName(Guid.NewGuid(), "_<..._", Unknown)
                rootScope.defineName(Guid.NewGuid(), "_...<_", Unknown)
                rootScope.defineName(Guid.NewGuid(), "_<...<_", Unknown)
                rootScope.defineName(Guid.NewGuid(), "_:_", Unknown)
                rootScope.defineName(Guid.NewGuid(), "_::_", Unknown)
                rootScope.defineName(Guid.NewGuid(), "_:::_", Unknown)
                rootScope.defineName(Guid.NewGuid(), "_<_", Unknown)
                rootScope.defineName(Guid.NewGuid(), "_>_", Unknown)
                rootScope.defineName(Guid.NewGuid(), "_<=_", Unknown)
                rootScope.defineName(Guid.NewGuid(), "_>=_", Unknown)
                rootScope.defineName(Guid.NewGuid(), "_\_", Unknown)
                rootScope.defineName(Guid.NewGuid(), "_?_", Unknown)
                rootScope.defineName(Guid.NewGuid(), "_<=_", Unknown)
                
                let res = markDeclarations rootScope result ([], [rootScope])
                let scopes = renderScopeReport res
                let coreExp = res.ToString()
                let scopesSection =
                    if String.IsNullOrWhiteSpace scopes then
                        "No scopes collected."
                    else
                        scopes
                $"Input:\n{input}\n\nCompilation succeeded.\n\nCoreExp:\n{coreExp}\n\nScopes:\n{scopesSection}"
            | Failure(errorMsg, _, _) ->
                $"Input:\n{input}\n\nCompilation failed.\nError: {errorMsg}"

        File.WriteAllText(outputPath, output)
        printfn "Wrote %s" outputFileName

    0
