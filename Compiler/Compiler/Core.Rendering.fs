module CoreDiagnostics

open System
open Exp
open Core

// Render a core tree for the snapshot output. Deliberately close to the
// notation used in docs/specification/core-expression.md, so a snapshot can be
// read against the specification.

let private renderScope (s: Scope) = ExpDiagnostics.formatScope s

let rec private renderPart = function
    | CoreText s -> s
    | CoreHole e -> $"{{{renderCore e}}}"

and renderCore (node: CoreNode) : string =
    let r = renderCore

    match node.Core with
    | CoreExp.Apply (f, a) -> $"({r f} {r a})"
    | CoreExp.Tuple xs -> xs |> List.map r |> String.concat ", " |> fun s -> $"({s})"
    | CoreExp.Product (positional, fusion, s, named) ->
        let mark =
            match fusion with
            | Fusing -> "@"
            | NonFusing -> "@@"
        let positionalText = positional |> List.map r |> String.concat ", "
        let namedText = named |> List.map r |> String.concat ", "
        let separator = if List.isEmpty positional then "" else " "
        $"({positionalText}{separator}{mark}[{renderScope s}] {namedText})"
    | CoreExp.Set xs -> xs |> List.map r |> String.concat ", " |> fun s -> $"{{{s}}}"
    | CoreExp.List xs -> xs |> List.map r |> String.concat ", " |> fun s -> $"[{s}]"
    | CoreExp.StringLiteral parts ->
        parts |> List.map renderPart |> String.concat "" |> fun s -> $"$\"{s}\""
    | CoreExp.Literal lit ->
        match lit with
        | Exp.Int32 i -> string i
        | Exp.Boolean b -> string b
        | Exp.Float32 f -> string f
        | Exp.Float64 d -> string d
        | Exp.Decimal m -> string m
        | Exp.String s -> $"\"{s}\""
    | CoreExp.Identifier role -> $"id({ExpDiagnostics.renderBindingRole role})"
    | CoreExp.This role -> $"this[{ExpDiagnostics.renderBindingRole role}]"
    | CoreExp.Void -> "void"
    | CoreExp.Unknown -> "???"
    | CoreExp.Local (s, e) -> $"local[{renderScope s}] {r e}"
    | CoreExp.Scoped (s, e) -> $"scoped[{renderScope s}] {r e}"
    | CoreExp.Record (s, e) -> $"record[{renderScope s}] {r e}"
    | CoreExp.Publish (access, s, e) ->
        let accessText =
            match access with
            | Exp.AccessModifier.Public -> "public"
            | Exp.AccessModifier.Private -> "private"
            | Exp.AccessModifier.Internal -> "internal"
            | Exp.AccessModifier.Protected -> "protected"
        $"{accessText}[{renderScope s}] {r e}"
    | CoreExp.Project (receiver, m) -> $"project({r receiver}, {r m})"
    | CoreExp.ProjectMembers (receiver, m) -> $"projectMembers({r receiver}, {r m})"
    | CoreExp.Equals (a, b) -> $"equals({r a}, {r b})"
    | CoreExp.Primary e -> $"primary({r e})"
    | CoreExp.Candidate e -> $"candidate({r e})"
    | CoreExp.IsDefined e -> $"isDefined({r e})"
    | CoreExp.Throw e -> $"throw({r e})"
    | CoreExp.Catch (e, h) -> $"catch({r e}, {r h})"
