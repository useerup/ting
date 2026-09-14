module CoreDiagnostics

open System
open Exp
open Core

// Render the compiler's internal core tree for snapshot output.

let private renderScope (s: Scope) = ExpDiagnostics.formatScope s

let rec private renderPart = function
    | CoreText s -> s
    | CoreHole e -> $"{{{renderCore e}}}"

and renderCore (node: CoreExp) : string =
    let r = renderCore

    match node with
    | CoreExp.Apply (_, f, a) -> $"({r f} {r a})"
    | CoreExp.Tuple (_, xs) -> xs |> List.map r |> String.concat ", " |> fun s -> $"({s})"
    | CoreExp.Product (_, positional, fusion, s, named) ->
        let mark =
            match fusion with
            | Fusing -> "@"
            | NonFusing -> "@@"
        let positionalText = positional |> List.map r |> String.concat ", "
        let namedText = named |> List.map r |> String.concat ", "
        let separator = if List.isEmpty positional then "" else " "
        $"({positionalText}{separator}{mark}[{renderScope s}] {namedText})"
    | CoreExp.Set (_, xs) -> xs |> List.map r |> String.concat ", " |> fun s -> $"{{{s}}}"
    | CoreExp.List (_, xs) -> xs |> List.map r |> String.concat ", " |> fun s -> $"[{s}]"
    | CoreExp.StringLiteral (_, parts) ->
        parts |> List.map renderPart |> String.concat "" |> fun s -> $"$\"{s}\""
    | CoreExp.Literal (_, lit) ->
        match lit with
        | Exp.Int32 i -> string i
        | Exp.Boolean b -> string b
        | Exp.Float32 f -> string f
        | Exp.Float64 d -> string d
        | Exp.Decimal m -> string m
        | Exp.String s -> $"\"{s}\""
    | CoreExp.Identifier (_, role) -> $"id({ExpDiagnostics.renderBindingRole role})"
    | CoreExp.This (_, role) -> $"this[{ExpDiagnostics.renderBindingRole role}]"
    | CoreExp.Void _ -> "void"
    | CoreExp.Unknown _ -> "???"
    | CoreExp.Local (_, s, e) -> $"local[{renderScope s}] {r e}"
    | CoreExp.Scoped (_, s, e) -> $"scoped[{renderScope s}] {r e}"
    | CoreExp.Record (_, s, e) -> $"record[{renderScope s}] {r e}"
    | CoreExp.Publish (_, access, s, e) ->
        let accessText =
            match access with
            | Exp.AccessModifier.Public -> "public"
            | Exp.AccessModifier.Private -> "private"
            | Exp.AccessModifier.Internal -> "internal"
            | Exp.AccessModifier.Protected -> "protected"
        $"{accessText}[{renderScope s}] {r e}"
    | CoreExp.Project (_, receiver, m) -> $"project({r receiver}, {r m})"
    | CoreExp.ProjectMembers (_, receiver, m) -> $"projectMembers({r receiver}, {r m})"
    | CoreExp.Equals (_, a, b) -> $"equals({r a}, {r b})"
    | CoreExp.Primary (_, e) -> $"primary({r e})"
    | CoreExp.Candidate (_, e) -> $"candidate({r e})"
    | CoreExp.IsDefined (_, e) -> $"isDefined({r e})"
    | CoreExp.Throw (_, e) -> $"throw({r e})"
    | CoreExp.Catch (_, e, h) -> $"catch({r e}, {r h})"
