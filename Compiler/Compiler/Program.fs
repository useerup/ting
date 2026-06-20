module Program

open System
open System.IO
open FParsec
open Parser
open Binder
open CoreExp
open CoreExpDiagnostics
open CoreExpScopes

let private extractFenceInfo (line: string) =
    let trimmed = line.TrimStart()
    if trimmed.StartsWith("```") then
        trimmed.Substring(3).Trim().ToLowerInvariant()
    else
        ""

let private isFenceLine (line: string) =
    line.TrimStart().StartsWith("```")

let private compileTingProgram (input: string) : string =
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
        rootScope.defineName(Guid.NewGuid(), "_\\_", Unknown)
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

let private updateMarkdownWithCompilerOutput (markdown: string) =
    let lines = markdown.Replace("\r\n", "\n").Split('\n') |> Array.toList

    let rec readUntilClosingFence acc idx =
        if idx >= lines.Length then
            List.rev acc, idx
        elif lines[idx].TrimStart() = "```" then
            List.rev acc, idx
        else
            readUntilClosingFence (lines[idx] :: acc) (idx + 1)

    let rec process idx (acc: string list) =
        if idx >= lines.Length then
            acc |> List.rev |> String.concat Environment.NewLine
        else
            let line = lines[idx]
            let info = extractFenceInfo line
            if info = "ting" then
                let tingOpen = line
                let tingContent, tingCloseIndex = readUntilClosingFence [] (idx + 1)
                let afterTingClose = tingCloseIndex + 1

                let hasExistingOutputBlock =
                    afterTingClose < lines.Length && extractFenceInfo lines[afterTingClose] = "ting-output"

                let input = tingContent |> String.concat Environment.NewLine
                let output = compileTingProgram input
                let outputLines = output.Replace("\r\n", "\n").Split('\n') |> Array.toList

                if hasExistingOutputBlock then
                    let outputContent, outputCloseIndex = readUntilClosingFence [] (afterTingClose + 1)
                    let rewritten =
                        [
                            tingOpen
                            yield! tingContent
                            "```"
                            "```ting-output"
                            yield! outputLines
                            "```"
                        ]
                    process (outputCloseIndex + 1) ((List.rev rewritten) @ acc)
                else
                    let inserted =
                        [
                            tingOpen
                            yield! tingContent
                            "```"
                            "```ting-output"
                            yield! outputLines
                            "```"
                        ]
                    process afterTingClose ((List.rev inserted) @ acc)
            else
                process (idx + 1) (line :: acc)

    process 0 []

[<EntryPoint>]
let main argv =
    let examplesDir = Path.Combine(__SOURCE_DIRECTORY__, "examples")

    if not (Directory.Exists examplesDir) then
        Directory.CreateDirectory examplesDir |> ignore

    let exampleFiles =
        Directory.GetFiles(examplesDir, "*.ting.md")
        |> Array.sort

    Console.Clear()

    for exampleFile in exampleFiles do
        let markdown = File.ReadAllText(exampleFile)
        let updated = updateMarkdownWithCompilerOutput markdown
        File.WriteAllText(exampleFile, updated)
        printfn "Updated %s" (Path.GetFileName(exampleFile))

    0
