module Exp

open System
open System.Collections.Generic

// Represents a scope with a unique Id and a set of identifiers

type Scope =
    | RootScope
    | LocalScope of Guid



// Represents expressions in the language

type Identifier = 
    | Unresolved of string
    | Resolved of string * Scope

type ExportType =
    | Public
    | Private
    | Protected
    | Internal
    | Local

type Exp =
    | Apply of Exp * Exp
    | Export of ExportType * Scope * Exp
    | Scoped of Scope * Exp
    | Declare of Scope * Exp
    | Reference of Scope * Exp
    | Referential of Exp
    | Define of Exp * Exp
    | Number of int
    | Identifier of Identifier
    | SetLiteral of Exp list
    | ListLiteral of Exp list
    | Tuple of Exp list
    | MemberAccess of Exp * Identifier
    | ItemMemberAccess of Exp * Identifier
    | Unit
    | StringLiteral of string

// --- Formatting ---

let private formatScope = function
    | RootScope -> "root"
    | LocalScope g -> g.ToString("N").Substring(0, 8)

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
        | Tuple items ->
            "(" + String.Join(", ", items |> List.map (fmt 0)) + ")"
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
        | Define (l, r) -> 
            let result = sprintf "%s = %s" (fmt 10 l) (fmt 9 r)
            if 10 < precedence then "(" + result + ")" else result
        | Scoped (s, x) -> sprintf "(scoped[%s] %s)" (formatScope s) (fmt 0 x)
        | Declare (s, x) -> sprintf "(declare[%s] %s)" (formatScope s) (fmt 0 x)
        | Reference (s, x) -> sprintf "reference[%s] %s" (formatScope s) (fmt 0 x)
        | Referential x -> sprintf "ref(%s)" (fmt 0 x)
        | MemberAccess (obj, id) -> sprintf "%s.%s" (fmt 95 obj) (formatIdentifier id)
        | ItemMemberAccess (obj, id) -> sprintf "%s::%s" (fmt 95 obj) (formatIdentifier id)
        | Export (et, s, e) ->
            let etStr =
                match et with
                | Public -> "public"
                | Private -> "private"
                | Protected -> "protected"
                | ProtectedInternal -> "protected internal"
                | Internal -> "internal"
                | Local -> "local"
            sprintf "(export %s [%s] %s)" etStr (formatScope s) (fmt 0 e)
        
    fmt 0 e
