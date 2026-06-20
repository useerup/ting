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
        $"Compilation succeeded.\n\n{coreExp}\n\nScopes:\n{scopesSection}"
    | Failure(errorMsg, _, _) ->
        $"Compilation failed.\nError: {errorMsg}"

let private updateMarkdownWithCompilerOutput (markdown: string) =
    let lines = markdown.Replace("\r\n", "\n").Split('\n')
    let result = ResizeArray<string>()

    let rec findClosingFence idx =
        if idx >= lines.Length then
            idx
        elif lines[idx].TrimStart() = "```" then
            idx
        else
            findClosingFence (idx + 1)

    let rec findNextTing idx =
        if idx >= lines.Length then
            lines.Length
        elif extractFenceInfo lines[idx] = "ting" then
            idx
        else
            findNextTing (idx + 1)

    let appendOutputBlock (compilerOutput: string) =
        result.Add("```ting-output")
        for line in compilerOutput.Replace("\r\n", "\n").Split('\n') do
            result.Add(line)
        result.Add("```")

    let rec copyFenceBlock idx =
        let closeIdx = findClosingFence (idx + 1)
        for i in idx .. min closeIdx (lines.Length - 1) do
            result.Add(lines[i])
        min (closeIdx + 1) lines.Length

    let rec rewriteRegion idx endIdx hasOutput compilerOutput =
        if idx >= endIdx then
            if not hasOutput then appendOutputBlock compilerOutput
        else
            let line = lines[idx]
            let info = extractFenceInfo line
            if info = "ting-output" then
                let closeIdx = findClosingFence (idx + 1)
                if not hasOutput then
                    appendOutputBlock compilerOutput
                    rewriteRegion (min (closeIdx + 1) lines.Length) endIdx true compilerOutput
                else
                    for i in idx .. min closeIdx (lines.Length - 1) do
                        result.Add(lines[i])
                    rewriteRegion (min (closeIdx + 1) lines.Length) endIdx true compilerOutput
            elif isFenceLine line then
                let nextIdx = copyFenceBlock idx
                rewriteRegion nextIdx endIdx hasOutput compilerOutput
            else
                result.Add(line)
                rewriteRegion (idx + 1) endIdx hasOutput compilerOutput

    let rec process idx =
        if idx >= lines.Length then
            ()
        else
            let line = lines[idx]
            if extractFenceInfo line = "ting" then
                let closeIdx = findClosingFence (idx + 1)
                for i in idx .. min closeIdx (lines.Length - 1) do
                    result.Add(lines[i])

                let tingContent =
                    if closeIdx > idx + 1 then
                        lines.[idx + 1 .. closeIdx - 1]
                    else
                        [||]

                let compilerOutput = compileTingProgram (String.concat Environment.NewLine tingContent)
                let regionEnd = findNextTing (min (closeIdx + 1) lines.Length)
                rewriteRegion (min (closeIdx + 1) lines.Length) regionEnd false compilerOutput
                process regionEnd
            else
                result.Add(line)
                process (idx + 1)

    process 0
    String.concat Environment.NewLine result

[<EntryPoint>]
let main argv =
    let examplesDir = Path.GetFullPath(Path.Combine(__SOURCE_DIRECTORY__, "..", "..", "..", "test-examples"))

    if not (Directory.Exists examplesDir) then
        Directory.CreateDirectory examplesDir |> ignore

    let exampleFiles =
        Directory.GetFiles(examplesDir, "*.ting.md")
        |> Array.sort

    if Environment.UserInteractive then
        try
            Console.Clear()
        with
        | :? IOException -> ()

    for exampleFile in exampleFiles do
        let markdown = File.ReadAllText(exampleFile)
        let updated = updateMarkdownWithCompilerOutput markdown
        File.WriteAllText(exampleFile, updated)
        File.SetLastWriteTimeUtc(exampleFile, DateTime.UtcNow)

    0
