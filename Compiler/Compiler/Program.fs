module CompilerEngine

open System
open System.IO
open Parser
open ParseResult
open Exp
open ExpDiagnostics
open ExpScopes

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
    // The root scope declares the ambient names a program can refer to. The
    // functions behind operators are *not* among them: the parser declares those
    // in its own operator scope and binds operator heads to it directly.
    let rootScope = Exp.LocalScope(Guid.Empty) :> Exp.Scope

    rootScope.defineName(Guid.NewGuid(), "bool", Exp.unknown)

    rootScope.defineName(Guid.NewGuid(), "string", Exp.unknown)
    rootScope.defineName(Guid.NewGuid(), "char", Exp.unknown)

    rootScope.defineName(Guid.NewGuid(), "float", Exp.unknown)
    rootScope.defineName(Guid.NewGuid(), "double", Exp.unknown)

    rootScope.defineName(Guid.NewGuid(), "int", Exp.unknown)
    rootScope.defineName(Guid.NewGuid(), "uint", Exp.unknown)
    rootScope.defineName(Guid.NewGuid(), "byte", Exp.unknown)
    rootScope.defineName(Guid.NewGuid(), "sbyte", Exp.unknown)
    rootScope.defineName(Guid.NewGuid(), "short", Exp.unknown)
    rootScope.defineName(Guid.NewGuid(), "ushort", Exp.unknown)
    rootScope.defineName(Guid.NewGuid(), "long", Exp.unknown)
    rootScope.defineName(Guid.NewGuid(), "ulong", Exp.unknown)
    rootScope.defineName(Guid.NewGuid(), "half", Exp.unknown)

    rootScope.defineName(Guid.NewGuid(), "decimal", Exp.unknown)

    rootScope.defineName(Guid.NewGuid(), "DateTime", Exp.unknown)
    rootScope.defineName(Guid.NewGuid(), "DateTimeOffset", Exp.unknown)

    rootScope.defineName(Guid.NewGuid(), "set", Exp.unknown)
    rootScope.defineName(Guid.NewGuid(), "any", Exp.unknown)
    rootScope.defineName(Guid.NewGuid(), "Func", Exp.unknown)

    rootScope.defineName(Guid.NewGuid(), "_all_", Exp.unknown)

    match parse rootScope input with
    | ParseSucceeded analyzed ->
        // Resolve first: classification of deferred bindings populates scopes,
        // so the reports below reflect the finished walk.
        let resolved, diagnostics = Resolver.resolve rootScope [ Parser.operatorScope ] analyzed

        // The root scope is ambient: nothing in the tree points at it.
        let scopes = renderScopeReportWith [ rootScope ] resolved
        let identifiers = renderIdentifierReportWith [ rootScope ] resolved

        let scopesSection =
            if String.IsNullOrWhiteSpace scopes then
                "No scopes collected."
            else
                scopes

        let identifiersSection =
            if String.IsNullOrWhiteSpace identifiers then
                "No identifier occurrences found."
            else
                identifiers

        let diagnosticsSection =
            if List.isEmpty diagnostics then
                ""
            else
                let lines =
                    diagnostics
                    |> List.map (fun d -> $"- {Resolver.formatDiagnostic d}")
                    |> String.concat "\n"
                $"\n\nDiagnostics:\n{lines}"

        $"Compilation succeeded.\n\nScopes:\n{scopesSection}\n\nIdentifiers:\n{identifiersSection}{diagnosticsSection}"
    | ParseFailed error ->
        $"Compilation failed.\n({error.Span.Start.Line},{error.Span.Start.Column}) {error.Message}"

let compileTingProgram (input: string) : string =
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

let updateMarkdownExamples (markdown: string) : string =
    updateMarkdownWithCompilerOutput markdown

let processMarkdownFile (filePath: string) : bool =
    let markdown = File.ReadAllText(filePath)
    let updated = updateMarkdownWithCompilerOutput markdown
    if updated <> markdown.Replace("\r\n", "\n") then
        File.WriteAllText(filePath, updated)
        File.SetLastWriteTimeUtc(filePath, DateTime.UtcNow)
        true
    else
        false

let verifyMarkdownFile (filePath: string) : bool =
    let markdown = File.ReadAllText(filePath)
    let expected = markdown.Replace("\r\n", "\n")
    let actual = updateMarkdownWithCompilerOutput markdown
    actual = expected
