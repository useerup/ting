module Ting.Syntax.Render

open System
open System.Text
open Token
open Syntax

type private Doc =
    | Leaf of string
    | Node of string * Doc list

let private width = 72

let private escapeText (text: string) =
    let sb = StringBuilder()
    for rune in text.EnumerateRunes() do
        match rune.Value with
        | 0x5C -> sb.Append "\\\\" |> ignore
        | 0x22 -> sb.Append "\\\"" |> ignore
        | 0x0A -> sb.Append "\\n" |> ignore
        | 0x0D -> sb.Append "\\r" |> ignore
        | 0x09 -> sb.Append "\\t" |> ignore
        | v when v > 0xFFFF -> sb.Append($"\\U{v:x8}") |> ignore
        | v when Rune.IsControl rune -> sb.Append($"\\u{v:x4}") |> ignore
        | _ -> sb.Append(rune.ToString()) |> ignore
    sb.ToString()

let private suffix (digits: string) (kind: NumberKind) =
    match kind with
    | Int -> ""
    | Float -> if digits.Contains '.' then "" else "f"
    | UInt -> "u"
    | Short -> "s"
    | UShort -> "us"
    | Long -> "L"
    | ULong -> "UL"
    | Double -> "d"
    | Decimal -> "m"

let rec private flat (doc: Doc) =
    match doc with
    | Leaf text -> text
    | Node(head, children) -> "(" + String.concat " " (head :: List.map flat children) + ")"

let rec private toDoc (expr: Expr) : Doc =
    match expr with
    | Identifier(_, name) -> Leaf name
    | This _ -> Leaf "this"
    | Void _ -> Leaf "void"
    | Bool(_, value) -> Leaf(if value then "true" else "false")
    | Number(_, digits, kind) -> Leaf(digits + suffix digits kind)
    | String(_, parts) -> Leaf("\"" + renderParts escapeText parts + "\"")
    | Group(_, inner) -> toDoc inner
    | Tuple(_, items) -> Node("tuple", List.map toDoc items)
    | Product(_, positional, fusion, named) ->
        let mark = if fusion = Fusing then "@" else "@@"
        Node("product", List.map toDoc positional @ [ Leaf mark ] @ List.map toDoc named)
    | Set(_, items) -> Node("set", List.map toDoc items)
    | Multiset(_, items) -> Node("multiset", List.map toDoc items)
    | List(_, items) -> Node("list", List.map toDoc items)
    | Apply(_, callee, argument) -> Node("apply", [ toDoc callee; toDoc argument ])
    | Prefix(_, op, operand) when op.Text = "-" -> Node("negate", [ toDoc operand ])
    | Prefix(_, op, operand) -> Node(op.Text, [ toDoc operand ])
    | Infix(_, op, left, right) -> Node(op.Text, [ toDoc left; toDoc right ])

and private renderParts (text: string -> string) (parts: StringPart list) =
    parts
    |> List.map (function
        | Text s -> text s)
    |> String.concat ""

let rec private layout (sb: StringBuilder) (indent: int) (doc: Doc) =
    let line = flat doc
    match doc with
    | Node(head, children) when indent + line.Length > width ->
        sb.Append('(').Append(head) |> ignore
        for child in children do
            sb.Append('\n').Append(' ', indent + 2) |> ignore
            layout sb (indent + 2) child
        sb.Append(')') |> ignore
    | _ -> sb.Append(line) |> ignore

/// An S-expression view of the tree. Grouping parentheses are not shown.
let render (expr: Expr) : string =
    let sb = StringBuilder()
    layout sb 0 (toDoc expr)
    sb.ToString()
