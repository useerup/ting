module BindingNotebookTests

open System.IO
open Xunit
open Ting.Syntax
open Ting.Binding

let private dir = Notebook.examplesDir "binding"

let compile (code: string) =
    match Parser.parse code with
    | Ok expr -> Report.render code (Binder.bind expr)
    | Error e -> $"syntax error {Source.Span.render e.Span}: {e.Message}"

let notebooks () = Notebook.files dir

[<Theory>]
[<MemberData(nameof notebooks)>]
let ``binding notebook`` (file: string) = Notebook.verify compile (Path.Combine(dir, file))
