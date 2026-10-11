/// Formulas written as F# expressions, loosest first: `<=>` and `===`, `^=>` (right-associative), `+` (or), `*` (and), `~~` (not).
/// `===` binds as loosely as `<=>`, so equations need parentheses.
module Dsl

open System
open Ting.Logic.Formula

let x, y, z, w, u = Var "x", Var "y", Var "z", Var "w", Var "u"
let a, b, c, d, e = Fn("a", []), Fn("b", []), Fn("c", []), Fn("d", []), Fn("e", [])
let agatha, butler, charles = Fn("agatha", []), Fn("butler", []), Fn("charles", [])
let p, q, r, s = Atom("p", []), Atom("q", []), Atom("r", []), Atom("s", [])

let private atom name (args: Term[]) = Atom(name, List.ofArray args)

type Symbols =
    static member P([<ParamArray>] args: Term[]) = atom "P" args
    static member Q([<ParamArray>] args: Term[]) = atom "Q" args
    static member R([<ParamArray>] args: Term[]) = atom "R" args
    static member U([<ParamArray>] args: Term[]) = atom "U" args
    static member V([<ParamArray>] args: Term[]) = atom "V" args
    static member F([<ParamArray>] args: Term[]) = atom "F" args
    static member G([<ParamArray>] args: Term[]) = atom "G" args
    static member H([<ParamArray>] args: Term[]) = atom "H" args
    static member J([<ParamArray>] args: Term[]) = atom "J" args
    static member L([<ParamArray>] args: Term[]) = atom "L" args
    static member M([<ParamArray>] args: Term[]) = atom "M" args
    static member lives([<ParamArray>] args: Term[]) = atom "lives" args
    static member killed([<ParamArray>] args: Term[]) = atom "killed" args
    static member hates([<ParamArray>] args: Term[]) = atom "hates" args
    static member richer([<ParamArray>] args: Term[]) = atom "richer" args
    static member f([<ParamArray>] args: Term[]) = Fn("f", List.ofArray args)
    static member g([<ParamArray>] args: Term[]) = Fn("g", List.ofArray args)
    static member m([<ParamArray>] args: Term[]) = Fn("m", List.ofArray args)
    static member i([<ParamArray>] args: Term[]) = Fn("i", List.ofArray args)

let private names (vars: Term list) =
    vars
    |> List.map (function
        | Var v -> v
        | t -> invalidArg (nameof vars) $"{termToString t} is not a variable")

let forall (vars: Term list) (body: Formula) = List.foldBack (fun v fm -> Forall(v, fm)) (names vars) body

let exists (vars: Term list) (body: Formula) = List.foldBack (fun v fm -> Exists(v, fm)) (names vars) body

let (~~) (fm: Formula) = Not fm
let ( * ) (left: Formula) (right: Formula) = And(left, right)
let (+) (left: Formula) (right: Formula) = Or(left, right)
let (^=>) (left: Formula) (right: Formula) = implies left right
let (<=>) (left: Formula) (right: Formula) = iff left right
let (===) (left: Term) (right: Term) = Atom("=", [ left; right ])
