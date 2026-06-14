module Program

open System
open System.IO
open FParsec
open Parser
open Binder
open CoreExp
open CoreExpDiagnostics

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
                let res = markDeclarations result ([], [])
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
