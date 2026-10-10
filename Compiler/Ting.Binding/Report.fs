/// A readable view of a binding result, for snapshot notebooks.
module Ting.Binding.Report

open System.Text
open Ting.Syntax.Source
open Ting.Binding.Model

let private pos (p: Position) = $"{p.Line},{p.Column}"

let private frameLabel (frames: Frame list) (id: FrameId) =
    let f = frames[id]
    let name =
        match f.Kind with
        | Root -> "root"
        | Let -> "let"
        | Publish access -> access
        | Local op
        | Fields op
        | TupleScope op
        | ExpressionScope op -> op
    $"{name} at {pos f.At}"

let render (source: string) (bound: Bound) : string =
    let sb = StringBuilder()
    let line (s: string) = sb.Append(s).Append('\n') |> ignore
    let text (span: Span) = source.Substring(span.Start.Offset, span.End.Offset - span.Start.Offset)
    let builtin (id: DeclId) = id < 0
    let frame = frameLabel bound.Frames

    let rec binding (b: Binding) =
        match b with
        | Declares id -> $"declares #{id}"
        | References id when builtin id -> "references builtin"
        | References id -> $"references #{id}"
        | FieldShorthand(field, value) -> $"declares field #{field}, value {binding value}"
        | Anonymous -> "anonymous"
        | Deferred(ScopeContents f) -> $"deferred: names of {frame f}"
        | Deferred(ImplicitThis f) -> $"deferred: implicit this of {frame f}"
        | Deferred PropertyName -> "deferred: property"
        | Deferred InterpolatedName -> "deferred: interpolated name"
        | Deferred Blocked -> "deferred: blocked"
        | Unresolved -> "unresolved"

    let kind (d: Decl) =
        match d.Kind with
        | Builtin -> "builtin"
        | LocalName -> "local"
        | Published -> "published"
        | Field -> "field"
        | ThisBinding -> "this"
        | Hosted(false, host) -> $"hosted on {text host}"
        | Hosted(true, host) -> $"hosted on members of {text host}"

    if not bound.Decls.IsEmpty then
        line "Declarations:"
        for d in bound.Decls do
            let where = d.Frames |> List.map frame |> String.concat "; "
            line $"  #{d.Id} {d.Name}: {kind d} ({where})"
    line "Identifiers:"
    for o in bound.Occurrences do
        line $"  {pos o.Span.Start} {o.Name}: {binding o.Binding}"
    if not bound.Diagnostics.IsEmpty then
        line "Errors:"
        for d in bound.Diagnostics do
            line $"  {Span.render d.Span}: {d.Message}"
    sb.ToString().TrimEnd('\n')
