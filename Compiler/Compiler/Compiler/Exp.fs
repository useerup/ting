module Exp

open System
open System.Collections.Generic

// Represents a scope with a unique Id and a set of identifiers

type Scope =
    | RootScope
    | LocalScope of Guid

type Names = HashSet<string>

// Represents expressions in the language

type Identifier = 
    | Unresolved of string
    | Resolved of string * Scope

type ExportType =
    | Public
    | Private
    | Protected
    | ProtectedInternal
    | Internal
    | Local

type Exp =
    | Apply of Exp * Exp
    | Declare of Names * Exp
    | Reference of Exp
    | Tuple of Exp list
    | Record of Exp list
    | Identifier of Identifier
    | Number of int
    | SetLiteral of Exp list
    | ListLiteral of Exp list
    | StringLiteral of string
    | Project of Exp * Exp
    | MemberProject of Exp * Exp
    | Unit

// --- Formatting ---

let private formatScope = function
    | RootScope -> "root"
    | LocalScope g -> g.ToString("N").Substring(0, 8)

let private formatNames (names:Names) = $""" ({String.Join(",", names)}) """

let private formatIdentifier = function
    | Unresolved s -> s
    | Resolved (s, _) -> s

let rec formatExp (e: Exp) : string =
    
    let operatorPrecedence = function
        | "in" -> 5
        | "=" -> 10
        | "+" | "-" -> 20
        | "*" | "/" -> 30
        | _ -> 100 // default high precedence for unknown operators 
    
    let rec fmt (precedence: int) e =
        match e with
        | Number n -> string n
        | Identifier id -> formatIdentifier id
        | Unit -> "()"
        | Tuple items ->
            "(" + String.Join(", ", items |> List.map (fmt 0)) + ")"
        | ListLiteral items ->
            "[" + String.Join(", ", items |> List.map (fmt 0)) + "]"
        | SetLiteral items ->
            "{" + String.Join(", ", items |> List.map (fmt 0)) + "}"
        | StringLiteral s -> sprintf "\"%s\"" s
        // Render encoded infix operators like Apply(Identifier Resolved("_+_", RootScope), Tuple [l; r]) as l + r
        | Apply(Identifier (Resolved (name, RootScope)), Tuple [l; r]) when name.Length >= 3 && name.[0] = '_' && name.[name.Length - 1] = '_' ->
            let op = name.Substring(1, name.Length - 2)
            let opPrec = operatorPrecedence op
            let result = sprintf "%s %s %s" (fmt opPrec l) op (fmt opPrec r)
            if opPrec < precedence then "(" + result + ")" else result
        | Apply(Identifier (Resolved (name, RootScope)), v) when name.EndsWith '_' ->
            let op = name.Substring(0, name.Length - 1)
            let opPrec = operatorPrecedence op
            let result = sprintf "%s%s" op (fmt opPrec v) 
            if opPrec < precedence then "(" + result + ")" else result
        | Apply (f, arg) -> 
            let result = sprintf "%s %s" (fmt 90 f) (fmt 91 arg)
            if 90 < precedence then "(" + result + ")" else result
        | Declare (names, x) -> sprintf "(declare[%s] %s)" (formatNames names) (fmt 0 x)
        | Reference x -> sprintf "reference[%s] %s" (formatScope s) (fmt 0 x)
        | Project (obj, id) -> sprintf "%s.%s" (fmt 95 obj) (formatIdentifier id)
        | MemberProject (obj, id) -> sprintf "%s::%s" (fmt 95 obj) (formatIdentifier id)
        
    fmt 0 e
