module ExampleNotebookTests

open System
open System.Collections.Generic
open System.IO
open Xunit
open CompilerEngine

type ExampleNotebookTests() =
    static let examplesDir = 
        let baseDir = AppContext.BaseDirectory
        let rec findExamplesDir (dir: string) : string option =
            let examplesPath = Path.Combine(dir, "examples", "notebook", "diagnostics")
            if Directory.Exists examplesPath then
                Some examplesPath
            elif dir = Path.GetPathRoot(dir) then
                None
            else
                findExamplesDir (Path.GetDirectoryName dir)
        
        match findExamplesDir baseDir with
        | Some path -> path
        | None -> 
            Path.Combine(__SOURCE_DIRECTORY__, "..", "..", "examples", "notebook", "diagnostics")

    static member GetExampleFiles() : string[] =
        if Directory.Exists examplesDir then
            Directory.GetFiles(examplesDir, "*.ting.md") |> Array.sort
        else
            [||]

    [<Fact>]
    member _.``Examples directory exists`` () =
        Assert.True(Directory.Exists examplesDir, $"Examples directory not found at {examplesDir}")

    [<Fact>]
    member _.``Example files exist`` () =
        let files = ExampleNotebookTests.GetExampleFiles()
        Assert.NotEmpty(files)

    [<Theory>]
    [<MemberData("GetExampleFileData")>]
    member _.``Example compiles successfully`` (filePath: string) =
        Assert.True(File.Exists filePath, $"File not found: {filePath}")
        
        let content = File.ReadAllText(filePath)
        Assert.Contains("```ting", content)
        
        let isValid = verifyMarkdownFile filePath
        Assert.True(isValid, $"Example {Path.GetFileName filePath} failed verification")

    static member GetExampleFileData() : IEnumerable<obj[]> =
        ExampleNotebookTests.GetExampleFiles()
        |> Seq.map (fun f -> [| f :> obj |])
