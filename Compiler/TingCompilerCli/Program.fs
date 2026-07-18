module Program

open System
open System.IO
open CompilerEngine

let printUsage () =
    fprintfn stderr "Usage: tingc [options] [file ...]"
    fprintfn stderr ""
    fprintfn stderr "Options:"
    fprintfn stderr "  --check              Compare compiler output against snapshots without writing"
    fprintfn stderr "  --help               Display this help message"
    fprintfn stderr ""
    fprintfn stderr "Arguments:"
    fprintfn stderr "  file                 Ting markdown file(s) to process"
    fprintfn stderr "                       If no files specified, reads from stdin"
    fprintfn stderr ""
    fprintfn stderr "Examples:"
    fprintfn stderr "  tingc example.ting.md    Compile and update example.ting.md with output"
    fprintfn stderr "  tingc --check *.ting.md  Verify all files match expected output"
    fprintfn stderr "  cat code.ting | tingc    Compile stdin and print output to stdout"

let compileStdin () : int =
    try
        let input = Console.In.ReadToEnd()
        let output = compileTingProgram input
        printfn "%s" output
        0
    with
    | ex ->
        eprintfn "Error reading from stdin: %s" ex.Message
        1

let compileMarkdownFile (filePath: string) : bool =
    try
        processMarkdownFile filePath
    with
    | ex ->
        eprintfn "Error processing file %s: %s" filePath ex.Message
        false

let verifyMarkdownFile (filePath: string) : bool =
    try
        verifyMarkdownFile filePath
    with
    | ex ->
        eprintfn "Error verifying file %s: %s" filePath ex.Message
        false

let expandGlob (pattern: string) : string[] =
    let dirPath = Path.GetDirectoryName pattern
    let filePattern = Path.GetFileName pattern
    let dir = if String.IsNullOrEmpty dirPath then "." else dirPath

    if Directory.Exists dir then
        try
            Directory.GetFiles(dir, filePattern) |> Array.sort
        with
        | _ -> [||]
    else
        [||]

[<EntryPoint>]
let main argv =
    let check = argv |> Array.exists (fun a -> a = "--check")
    let help = argv |> Array.exists (fun a -> a = "--help" || a = "-h")

    if help then
        printUsage()
        0
    else
        let files =
            argv
            |> Array.filter (fun a -> not (a.StartsWith "-"))
            |> Array.collect (fun pattern ->
                if pattern.Contains("*") || pattern.Contains("?") then
                    expandGlob pattern
                else
                    [| pattern |])
            |> Array.distinct
            |> Array.sort

        if files.Length = 0 then
            // Read from stdin
            compileStdin()
        elif check then
            // Verify mode: check files match expected output
            let mutable failures = 0
            for file in files do
                if not (File.Exists file) then
                    eprintfn "File not found: %s" file
                    failures <- failures + 1
                elif not (verifyMarkdownFile file) then
                    failures <- failures + 1
                    printfn "FAIL %s" (Path.GetFileName file)

            printfn ""
            if failures = 0 then
                printfn "All %d file(s) passed." files.Length
                0
            else
                printfn "%d of %d file(s) failed." failures files.Length
                1
        else
            // Update mode: compile and rewrite output snapshots
            let mutable changed = 0
            for file in files do
                if not (File.Exists file) then
                    eprintfn "File not found: %s" file
                elif compileMarkdownFile file then
                    changed <- changed + 1
                    printfn "updated %s" (Path.GetFileName file)

            printfn ""
            printfn "Regenerated %d of %d file(s)" changed files.Length
            0
