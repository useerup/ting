module CoreNotebookTests

open System.IO
open Xunit
open Ting.Syntax
open Ting.Binding
open Ting.Core

let private dir = Notebook.examplesDir "core"

let compile (code: string) =
    match Parser.parse code with
    | Ok expr -> Translate.render (Translate.translate expr (Binder.bind expr))
    | Error e -> $"syntax error {Source.Span.render e.Span}: {e.Message}"

let notebooks () = Notebook.files dir

[<Theory>]
[<MemberData(nameof notebooks)>]
let ``core notebook`` (file: string) = Notebook.verify compile (Path.Combine(dir, file))
