module Evaluator

open System
open System.Collections.Generic
open Exp
open Core
open CoreScopeAnalysis

// Runtime values produced by evaluating a lowered core tree.
type Value =
    | VInt of int
    | VBool of bool
    | VFloat of float
    | VDouble of double
    | VDecimal of decimal
    | VString of string
    | VTuple of Value list
    /// A product with a named section: positional fields, then named fields.
    | VProduct of Value list * (string * Value) list
    | VClosure of Guid list * CoreNode * Env

and Env = Map<Guid, Value>

// The operator/identifier name carried by an identifier, when it is a plain
// (non-interpolated) name.
let private opName (e: CoreNode) : string option =
    match e.Core with
    | CoreExp.Identifier (Resolved (_, [ TextPart s ]))
    | CoreExp.Identifier (Referential (_, [ TextPart s ]))
    | CoreExp.Identifier (Declarative (_, [ TextPart s ])) -> Some s
    | _ -> None

// The declaration id introduced by the left-hand side of a binding, if any.
let rec private declIdOf (lhs: CoreNode) : Guid option =
    match lhs.Core with
    | CoreExp.Identifier (Declarative (id, _)) -> Some id
    | CoreExp.Project (_, m)
    | CoreExp.ProjectMembers (_, m) -> declIdOf m
    | CoreExp.Local (_, e) -> declIdOf e
    | _ -> None

// The parameter declaration ids bound by a lambda's parameter expression.
let rec private paramIds (param: CoreNode) : Guid list =
    match param.Core with
    | CoreExp.Identifier (Declarative (id, _)) -> [ id ]
    | CoreExp.Tuple xs -> xs |> List.collect paramIds
    | CoreExp.Local (_, e) -> paramIds e
    | _ -> []

let private valueEqual (a: Value) (b: Value) : bool =
    match a, b with
    | VInt x, VInt y -> x = y
    | VBool x, VBool y -> x = y
    | VString x, VString y -> x = y
    | VFloat x, VFloat y -> x = y
    | VDouble x, VDouble y -> x = y
    | VDecimal x, VDecimal y -> x = y
    | _ -> false

// Apply a primitive binary operator to two evaluated operands.
let private primOp (op: string) (a: Value) (b: Value) : Value option =
    match op, a, b with
    // Integer arithmetic.
    | "_+_", VInt x, VInt y -> Some(VInt(x + y))
    | "_-_", VInt x, VInt y -> Some(VInt(x - y))
    | "_*_", VInt x, VInt y -> Some(VInt(x * y))
    | "_/_", VInt x, VInt y when y <> 0 -> Some(VInt(x / y))
    | "_%_", VInt x, VInt y when y <> 0 -> Some(VInt(x % y))
    | "_^_", VInt x, VInt y when y >= 0 -> Some(VInt(pown x y))
    | "_<_", VInt x, VInt y -> Some(VBool(x < y))
    | "_>_", VInt x, VInt y -> Some(VBool(x > y))
    | "_<=_", VInt x, VInt y -> Some(VBool(x <= y))
    | "_>=_", VInt x, VInt y -> Some(VBool(x >= y))
    // Double arithmetic.
    | "_+_", VDouble x, VDouble y -> Some(VDouble(x + y))
    | "_-_", VDouble x, VDouble y -> Some(VDouble(x - y))
    | "_*_", VDouble x, VDouble y -> Some(VDouble(x * y))
    | "_/_", VDouble x, VDouble y when y <> 0.0 -> Some(VDouble(x / y))
    // Decimal arithmetic.
    | "_+_", VDecimal x, VDecimal y -> Some(VDecimal(x + y))
    | "_-_", VDecimal x, VDecimal y -> Some(VDecimal(x - y))
    | "_*_", VDecimal x, VDecimal y -> Some(VDecimal(x * y))
    | "_/_", VDecimal x, VDecimal y when y <> 0m -> Some(VDecimal(x / y))
    // String concatenation.
    | "_+_", VString x, VString y -> Some(VString(x + y))
    // Boolean logic.
    | "_&&_", VBool x, VBool y -> Some(VBool(x && y))
    | "_||_", VBool x, VBool y -> Some(VBool(x || y))
    | "_&_", VBool x, VBool y -> Some(VBool(x && y))
    | "_|_", VBool x, VBool y -> Some(VBool(x || y))
    | _ -> None

// Apply a primitive unary operator to its evaluated operand. Negation is a
// primitive rather than part of the literal grammar, so `-1` is an application
// that has to be folded here.
// TODO Figure out how to handle negation of integer ranges, where the negative range 
//      in 2nds complement has a larger magnitude than the positive range. The current
//      implementation will throw an exception if the negation overflows, 
//      which is not ideal. However, we will live with it for now.
let private primUnaryOp (op: string) (a: Value) : Value option =
    match op, a with
    | "-_", VInt x -> Some(VInt(-x))
    | "-_", VFloat x -> Some(VFloat(-x))
    | "-_", VDouble x -> Some(VDouble(-x))
    | "-_", VDecimal x -> Some(VDecimal(-x))
    | _ -> None

// The fields a value contributes to a product that fuses it one level deep. A
// value that is not a product is a one-field product, so it contributes itself.
let private fusedFields (v: Value) : Value list * (string * Value) list =
    match v with
    | VTuple vs -> vs, []
    | VProduct (positional, named) -> positional, named
    | single -> [ single ], []

let private literalValue (lit: Exp.Literal) : Value =
    match lit with
    | Exp.Int32 i -> VInt i
    | Exp.Boolean b -> VBool b
    | Exp.Float32 f -> VFloat f
    | Exp.Float64 d -> VDouble d
    | Exp.Decimal d -> VDecimal d
    | Exp.String s -> VString s

// Evaluate a lowered core tree to a value, when it denotes a computable value.
// Returns None when evaluation cannot proceed (unresolved names, sets, records
// without values, unsupported forms). Evaluation is intentionally partial.
let evaluate (rootScope: Scope) (exp: CoreNode) : Value option =
    // Map declaration id -> name entry, so hosted members can be distinguished
    // (direct property vs member property) at projection use sites.
    let registry = Dictionary<Guid, NameEntry>()

    for kvp in rootScope.Names do
        registry[kvp.Key] <- kvp.Value

    for s in collectAllScopes exp do
        for kvp in s.Names do
            registry[kvp.Key] <- kvp.Value

    let hostOf (id: Guid) : NameHost option =
        match registry.TryGetValue id with
        | true, e -> e.Host
        | _ -> None

    // The plain (non-interpolated) name a declaration was given.
    let nameOf (id: Guid) : string option =
        match registry.TryGetValue id with
        | true, e ->
            match e.NameParts with
            | [ TextPart s ] -> Some s
            | _ -> None
        | _ -> None

    let rec eval (env: Env) (e: CoreNode) : Value option =
        match e.Core with
        | CoreExp.Literal lit -> Some(literalValue lit)
        | CoreExp.Local (_, inner)
        | CoreExp.Scoped (_, inner)
        | CoreExp.Publish (_, _, inner) -> eval env inner
        | CoreExp.Identifier (Resolved (id, _)) -> Map.tryFind id env
        | CoreExp.Tuple xs -> evalList env xs |> Option.map VTuple
        | CoreExp.Product (positional, fusion, _, named) -> evalProduct env positional fusion named
        | CoreExp.Project (receiver, m) -> evalProjection env receiver m
        | CoreExp.Apply (f, arg) -> evalApply env f arg
        // Attribute erasure is the identity while there is no attribute layer
        // to erase. It stays a distinct core form so that the pass that adds
        // attributes has a place to hook into.
        | CoreExp.Primary inner
        | CoreExp.Candidate inner -> eval env inner
        // `=` as a value rather than as a binding: it asserts that its operands
        // denote the same value.
        | CoreExp.Equals (a, b) ->
            match eval env a, eval env b with
            | Some va, Some vb -> Some(VBool(valueEqual va vb))
            | _ -> None
        | _ -> None

    and evalList (env: Env) (xs: CoreNode list) : Value list option =
        match xs with
        | [] -> Some []
        | h :: t ->
            match eval env h with
            | Some v -> evalList env t |> Option.map (fun vs -> v :: vs)
            | None -> None

    // A product literal `(p1, p2 @ n1 = v1, ...)`. Under `@` the positional
    // section is fused one level deep, so a single product-valued element
    // contributes its own fields; under `@@` the section stays one field.
    and evalProduct (env: Env) (positional: CoreNode list) (fusion: Fusion) (named: CoreNode list) : Value option =
        match evalList env positional, evalNamedFields env named with
        | Some positionalValues, Some namedFields ->
            let fields, fusedNamed =
                match fusion, positionalValues with
                | Fusing, [ single ] -> fusedFields single
                | Fusing, values -> values, []
                | NonFusing, [ single ] -> [ single ], []
                | NonFusing, values -> [ VTuple values ], []
            Some(VProduct(fields, fusedNamed @ namedFields))
        | _ -> None

    and evalNamedFields (env: Env) (fields: CoreNode list) : (string * Value) list option =
        match fields with
        | [] -> Some []
        | h :: t ->
            match evalNamedField env h with
            | Some f -> evalNamedFields env t |> Option.map (fun fs -> f :: fs)
            | None -> None

    // One element of a named section: `name = value` declares `name` as a field.
    and evalNamedField (env: Env) (field: CoreNode) : (string * Value) option =
        match field.Core with
        | CoreExp.Local (_, inner)
        | CoreExp.Scoped (_, inner)
        | CoreExp.Publish (_, _, inner) -> evalNamedField env inner
        | CoreExp.Equals (lhs, rhs) ->
            match declIdOf lhs |> Option.bind nameOf, eval env rhs with
            | Some name, Some v -> Some(name, v)
            | _ -> None
        | _ -> None

    and evalApply (env: Env) (f: CoreNode) (arg: CoreNode) : Value option =
        match opName f with
        | Some "_in_" ->
            match arg.Core with
            | CoreExp.Tuple [ binding; body ] ->
                match evalBinding env binding with
                | Some env' -> eval env' body
                | None -> None
            | _ -> None
        | Some "_=>_" ->
            match arg.Core with
            | CoreExp.Tuple [ param; body ] -> Some(VClosure(paramIds param, body, env))
            | _ -> None
        | Some op when isPrimOp op ->
            match arg.Core with
            | CoreExp.Tuple [ a; b ] ->
                match eval env a, eval env b with
                | Some va, Some vb -> primOp op va vb
                | _ -> None
            | _ -> None
        | Some op when isPrimUnaryOp op -> eval env arg |> Option.bind (primUnaryOp op)
        | _ ->
            // Ordinary function application: head must evaluate to a closure.
            match eval env f with
            | Some (VClosure (ps, body, cenv)) ->
                match eval env arg with
                | Some av -> applyClosure ps body cenv av
                | None -> None
            | _ -> None

    and evalProjection (env: Env) (receiver: CoreNode) (m: CoreNode) : Value option =
        match m.Core with
        | CoreExp.Identifier (Resolved (id, _)) ->
            match Map.tryFind id env with
            | Some v ->
                match hostOf id with
                | Some (MemberPropertyHost _) ->
                    // Extension member: apply the member function to the receiver.
                    match eval env receiver with
                    | Some rv -> applyValue v rv
                    | None -> None
                | _ -> Some v
            | None -> None
        | _ -> None

    and applyValue (fn: Value) (argv: Value) : Value option =
        match fn with
        | VClosure (ps, body, cenv) -> applyClosure ps body cenv argv
        | _ -> None

    and applyClosure (ps: Guid list) (body: CoreNode) (cenv: Env) (argv: Value) : Value option =
        let binds =
            match ps, argv with
            | [ single ], v -> [ single, v ]
            | many, VTuple vs when List.length many = List.length vs -> List.zip many vs
            | _ -> []
        if List.isEmpty binds && not (List.isEmpty ps) then
            None
        else
            let env' = (cenv, binds) ||> List.fold (fun e (id, v) -> Map.add id v e)
            eval env' body

    and evalBinding (env: Env) (binding: CoreNode) : Env option =
        match binding.Core with
        | CoreExp.Local (_, inner) -> evalBinding env inner
        | CoreExp.Equals (lhs, rhs) ->
            match eval env rhs with
            | Some v ->
                match declIdOf lhs with
                | Some id -> Some(Map.add id v env)
                | None -> Some env
            | None -> None
        | _ -> None

    and isPrimOp (op: string) : bool =
        match op with
        | "_+_" | "_-_" | "_*_" | "_/_" | "_%_" | "_^_"
        | "_<_" | "_>_" | "_<=_" | "_>=_"
        | "_&&_" | "_||_" | "_&_" | "_|_" -> true
        | _ -> false

    and isPrimUnaryOp (op: string) : bool = op = "-_"

    eval Map.empty exp

// Render a value for the snapshot output.
let rec render (v: Value) : string =
    match v with
    | VInt i -> string i
    | VBool b -> if b then "true" else "false"
    | VFloat f -> string f
    | VDouble d -> string d
    | VDecimal d -> string d
    | VString s -> "\"" + s + "\""
    | VTuple vs -> "(" + (vs |> List.map render |> String.concat ", ") + ")"
    | VProduct (positional, named) ->
        let positionalText = positional |> List.map render |> String.concat ", "
        let namedText = named |> List.map (fun (n, v) -> $"{n} = {render v}") |> String.concat ", "
        let separator = if List.isEmpty positional then "" else " "
        "(" + positionalText + separator + "@ " + namedText + ")"
    | VClosure _ -> "<function>"

// Whether a value is a concrete first-order datum worth showing in snapshots.
// Closures and tuples containing closures are not shown, to keep the snapshot
// focused on computed data results.
let rec isDisplayable (v: Value) : bool =
    match v with
    | VInt _
    | VBool _
    | VFloat _
    | VDouble _
    | VDecimal _
    | VString _ -> true
    | VTuple vs -> vs |> List.forall isDisplayable
    | VProduct (positional, named) ->
        List.forall isDisplayable positional && named |> List.forall (snd >> isDisplayable)
    | VClosure _ -> false
