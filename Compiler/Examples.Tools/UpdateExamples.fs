module UpdateExamples

open System
open System.IO
open CompilerEngine

[<EntryPoint>]
let main argv =
    let rec findExamplesDir (dir: string) : string option =
        let examplesPath = Path.Combine(dir, "examples", "notebook", "diagnostics")
        if Directory.Exists examplesPath then
            Some examplesPath
        elif dir = Path.GetPathRoot(dir) then
            None
        else
            findExamplesDir (Path.GetDirectoryName dir)
    
    let examplesDir =
        match findExamplesDir AppContext.BaseDirectory with
        | Some path -> path
        | None -> 
            // Fallback
            Path.Combine(__SOURCE_DIRECTORY__, "..", "..", "examples", "notebook", "diagnostics")

    if not (Directory.Exists examplesDir) then
        eprintfn "Examples directory not found at %s" examplesDir
        1
    else
        let files = Directory.GetFiles(examplesDir, "*.ting.md") |> Array.sort

        if files.Length = 0 then
            eprintfn "No .ting.md files found"
            1
        else
            let mutable updated = 0
            let mutable total = 0

            for file in files do
                total <- total + 1
                try
                    if processMarkdownFile file then
                        updated <- updated + 1
                        printfn "Updated: %s" (Path.GetFileName file)
                    else
                        printfn "No changes: %s" (Path.GetFileName file)
                with
                | ex ->
                    eprintfn "Error processing %s: %s" (Path.GetFileName file) ex.Message

            printfn ""
            printfn "Updated %d of %d files" updated total
            0
