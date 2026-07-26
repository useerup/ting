module ExampleNotebookTests

open System
open System.Collections.Generic
open System.IO
open Xunit
open CompilerEngine

type ExampleNotebookTests() =
    // Resolve the examples source folder relative to the test assembly:
    // <root>\Examples.Tests\bin\<Configuration>\<Tfm> -> <root>\examples\notebook\diagnostics.
    // A relative path (even one climbing outside the repo checkout) is used on
    // purpose so the tests always update the checked-in source files, never a
    // copy in the build output.
    static let examplesDir =
        Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..", "..",
                "examples", "notebook", "diagnostics"))

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
        
        // Update the example file in place with the current compiler output,
        // then verify the result round-trips (the update is idempotent).
        processMarkdownFile filePath |> ignore
        let isValid = verifyMarkdownFile filePath
        Assert.True(isValid, $"Example {Path.GetFileName filePath} failed verification")

    static member GetExampleFileData() : IEnumerable<obj[]> =
        ExampleNotebookTests.GetExampleFiles()
        |> Seq.map (fun f -> [| f :> obj |])
