module Program

open System
open System.IO
open FParsec
open Parser
open Binder
open CoreExp
open CoreExpDiagnostics
open CoreExpScopes

// Declarations receive a fresh Guid on every compilation, so raw output is not
// stable across runs. Rewrite each distinct non-empty Guid to a sequential
// placeholder (#0, #1, ...) in order of first appearance. This keeps snapshots
// stable while preserving identity relationships (same Guid -> same placeholder).
let private guidRegex =
    System.Text.RegularExpressions.Regex(
        "[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}")

let private normalizeGuids (text: string) : string =
    let mapping = System.Collections.Generic.Dictionary<string, string>()
    let empty = Guid.Empty.ToString()
    guidRegex.Replace(
        text,
        fun m ->
            let g = m.Value
            if g = empty then
                g
            else
                match mapping.TryGetValue g with
                | true, placeholder -> placeholder
                | _ ->
                    let placeholder = $"#{mapping.Count}"
                    mapping.Add(g, placeholder)
                    placeholder)

let private extractFenceInfo (line: string) =
    let trimmed = line.TrimStart()
    if trimmed.StartsWith("```") then
        trimmed.Substring(3).Trim().ToLowerInvariant()
    else
        ""

let private isFenceLine (line: string) =
    line.TrimStart().StartsWith("```")

let private compileTingProgramRaw (input: string) : string =
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
        let resolved, diagnostics = Resolver.resolve rootScope res
        let scopes = renderScopeReport resolved
        let coreExp = resolved.ToString()
        let scopesSection =
            if String.IsNullOrWhiteSpace scopes then
                "No scopes collected."
            else
                scopes
        let diagnosticsSection =
            if List.isEmpty diagnostics then
                ""
            else
                let lines =
                    diagnostics
                    |> List.map (fun d -> $"- {Resolver.formatDiagnostic d}")
                    |> String.concat "\n"
                $"\n\nDiagnostics:\n{lines}"
        let valueSection =
            match Evaluator.evaluate rootScope resolved with
            | Some v when Evaluator.isDisplayable v -> $"\n\nValue:\n{Evaluator.render v}"
            | _ -> ""
        $"Compilation succeeded.\n\n{coreExp}\n\nScopes:\n{scopesSection}{diagnosticsSection}{valueSection}"
    | Failure(errorMsg, _, _) ->
        $"Compilation failed.\nError: {errorMsg}"

let private compileTingProgram (input: string) : string =
    normalizeGuids (compileTingProgramRaw input)

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

    let rec rewriteMarkdown idx =
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
                rewriteMarkdown regionEnd
            else
                result.Add(line)
                rewriteMarkdown (idx + 1)

    rewriteMarkdown 0
    String.concat "\n" result

[<EntryPoint>]
let main argv =
    // Modes:
    //   (default)  check  - compile each example and compare against the committed
    //                       ```ting-output``` snapshot; report mismatches, fail on any.
    //   --update          - regenerate snapshots in place (previous behaviour).
    // An optional positional argument overrides the examples directory.
    let update = argv |> Array.exists (fun a -> a = "--update" || a = "-u")
    let pathArg = argv |> Array.tryFind (fun a -> not (a.StartsWith "-"))

    let examplesDir =
        match pathArg with
        | Some p -> Path.GetFullPath p
        | None -> Path.GetFullPath(Path.Combine(__SOURCE_DIRECTORY__, "..", "..", "..", "test-examples"))

    if not (Directory.Exists examplesDir) then
        eprintfn "Examples directory not found: %s" examplesDir
        1
    else

    let exampleFiles =
        Directory.GetFiles(examplesDir, "*.ting.md")
        |> Array.sort

    if update then
        for exampleFile in exampleFiles do
            let markdown = File.ReadAllText(exampleFile)
            let updated = updateMarkdownWithCompilerOutput markdown
            if updated <> markdown.Replace("\r\n", "\n") then
                File.WriteAllText(exampleFile, updated)
                File.SetLastWriteTimeUtc(exampleFile, DateTime.UtcNow)
        printfn "Updated %d example(s) in %s" exampleFiles.Length examplesDir
        0
    else
        let mutable failures = 0
        for exampleFile in exampleFiles do
            let markdown = File.ReadAllText(exampleFile)
            let expected = markdown.Replace("\r\n", "\n")
            let actual = updateMarkdownWithCompilerOutput markdown
            if actual <> expected then
                failures <- failures + 1
                printfn "FAIL %s" (Path.GetFileName exampleFile)

        printfn ""
        if failures = 0 then
            printfn "All %d example(s) passed." exampleFiles.Length
            0
        else
            printfn "%d of %d example(s) failed. Run with --update to regenerate snapshots." failures exampleFiles.Length
            1
