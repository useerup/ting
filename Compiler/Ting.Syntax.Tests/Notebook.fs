/// Snapshot notebooks: markdown files whose ```ting blocks are each followed by a
/// ```ting-output block holding the expected output.
module Notebook

open System
open System.IO

/// Set TING_UPDATE_SNAPSHOTS=1 to rewrite the output blocks instead of checking them.
let updateMode = Environment.GetEnvironmentVariable "TING_UPDATE_SNAPSHOTS" = "1"

/// Locates a folder under Git/Compiler/examples, relative to the test assembly.
let examplesDir (name: string) =
    Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "examples", name))

/// Recomputes every output block. Missing output blocks are inserted.
let regenerate (compile: string -> string) (text: string) : string =
    let lines = text.Replace("\r\n", "\n").Split('\n')
    let output = ResizeArray<string>()
    let mutable i = 0
    while i < lines.Length do
        if lines[i].TrimEnd() = "```ting" then
            output.Add lines[i]
            i <- i + 1
            let code = ResizeArray<string>()
            while i < lines.Length && lines[i].TrimEnd() <> "```" do
                code.Add lines[i]
                output.Add lines[i]
                i <- i + 1
            if i >= lines.Length then failwith "unterminated ```ting block"
            output.Add lines[i]
            i <- i + 1
            let mutable j = i
            while j < lines.Length && lines[j].Trim() = "" do
                j <- j + 1
            if j < lines.Length && lines[j].TrimEnd() = "```ting-output" then
                j <- j + 1
                while j < lines.Length && lines[j].TrimEnd() <> "```" do
                    j <- j + 1
                i <- j + 1
            output.Add ""
            output.Add "```ting-output"
            output.Add(compile (String.Join("\n", code)))
            output.Add "```"
        else
            output.Add lines[i]
            i <- i + 1
    String.Join("\n", output)

/// Checks (or, in update mode, rewrites) one notebook file.
let verify (compile: string -> string) (path: string) =
    let original = File.ReadAllText(path).Replace("\r\n", "\n")
    let expected = regenerate compile original
    if updateMode then
        if expected <> original then File.WriteAllText(path, expected)
    else
        Xunit.Assert.Equal(expected, original)

let files (dir: string) : seq<obj[]> =
    Directory.GetFiles(dir, "*.ting.md") |> Array.sort |> Seq.map (fun f -> [| box (Path.GetFileName f) |])
