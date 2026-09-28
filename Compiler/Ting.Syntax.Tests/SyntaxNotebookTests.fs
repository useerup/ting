module SyntaxNotebookTests

open System.IO
open Xunit
open Ting.Syntax

let private dir = Notebook.examplesDir "syntax"

let compile (code: string) =
    match Parser.parse code with
    | Ok expr -> Render.render expr
    | Error e -> $"error {Source.Span.render e.Span}: {e.Message}"

let notebooks () = Notebook.files dir

[<Theory>]
[<MemberData(nameof notebooks)>]
let ``syntax notebook`` (file: string) = Notebook.verify compile (Path.Combine(dir, file))
