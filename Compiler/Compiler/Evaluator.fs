module Evaluator

open System
open System.Collections.Generic
open Exp
open ExpScopeAnalysis

// Runtime values produced by evaluating a resolved Exp.
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
    | VClosure of Guid list * Node * Env

and Env = Map<Guid, Value>

// The operator/identifier name carried by an identifier, when it is a plain
// (non-interpolated) name.
let private opName (e: Node) : string option =
    match e.Exp with
    | Identifier (Resolved (_, [ TextPart s ]))
    | Identifier (Referential (_, [ TextPart s ]))
    | Identifier (Declarative (_, [ TextPart s ])) -> Some s
    | _ -> None

// The declaration id introduced by the left-hand side of a binding, if any.
let rec private declIdOf (lhs: Node) : Guid option =
    match lhs.Exp with
    | Identifier (Declarative (id, _)) -> Some id
    | Projection (_, _, m)
    | MemberProjection (_, _, m) -> declIdOf m
    | Local (_, e) -> declIdOf e
    | _ -> None

// The parameter declaration ids bound by a lambda's parameter expression.
let rec private paramIds (param: Node) : Guid list =
    match param.Exp with
    | Identifier (Declarative (id, _)) -> [ id ]
    | Tuple xs -> xs |> List.collect paramIds
    | Local (_, e) -> paramIds e
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
    // Equality across primitives.
    | "_==_", x, y -> Some(VBool(valueEqual x y))
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

// Evaluate a resolved Exp to a value, when it denotes a computable value.
// Returns None when evaluation cannot proceed (unresolved names, sets, records
// without values, unsupported forms). Evaluation is intentionally partial.
let evaluate (rootScope: Scope) (exp: Node) : Value option =
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

    let rec eval (env: Env) (e: Node) : Value option =
        match e.Exp with
        | Literal lit -> Some(literalValue lit)
        | Local (_, inner)
        | ScopeExp (_, inner)
        | Publish (_, _, inner) -> eval env inner
        | Identifier (Resolved (id, _)) -> Map.tryFind id env
        | Tuple xs -> evalList env xs |> Option.map VTuple
        | Product (positional, fusion, _, named) -> evalProduct env positional fusion named
        | Projection (_, receiver, m) -> evalProjection env receiver m
        | Apply (f, arg) -> evalApply env f arg
        | _ -> None

    and evalList (env: Env) (xs: Node list) : Value list option =
        match xs with
        | [] -> Some []
        | h :: t ->
            match eval env h with
            | Some v -> evalList env t |> Option.map (fun vs -> v :: vs)
            | None -> None

    // A product literal `(p1, p2 @ n1 = v1, ...)`. Under `@` the positional
    // section is fused one level deep, so a single product-valued element
    // contributes its own fields; under `@@` the section stays one field.
    and evalProduct (env: Env) (positional: Node list) (fusion: Fusion) (named: Node list) : Value option =
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

    and evalNamedFields (env: Env) (fields: Node list) : (string * Value) list option =
        match fields with
        | [] -> Some []
        | h :: t ->
            match evalNamedField env h with
            | Some f -> evalNamedFields env t |> Option.map (fun fs -> f :: fs)
            | None -> None

    // One element of a named section: `name = value` declares `name` as a field.
    and evalNamedField (env: Env) (field: Node) : (string * Value) option =
        match field.Exp with
        | Local (_, inner)
        | ScopeExp (_, inner)
        | Publish (_, _, inner) -> evalNamedField env inner
        | Apply (f, E (Tuple [ lhs; rhs ])) when opName f = Some "_=_" ->
            match declIdOf lhs |> Option.bind nameOf, eval env rhs with
            | Some name, Some v -> Some(name, v)
            | _ -> None
        | _ -> None

    and evalApply (env: Env) (f: Node) (arg: Node) : Value option =
        match opName f with
        | Some "_in_" ->
            match arg.Exp with
            | Tuple [ binding; body ] ->
                match evalBinding env binding with
                | Some env' -> eval env' body
                | None -> None
            | _ -> None
        | Some "_=>_" ->
            match arg.Exp with
            | Tuple [ param; body ] -> Some(VClosure(paramIds param, body, env))
            | _ -> None
        | Some op when isPrimOp op ->
            match arg.Exp with
            | Tuple [ a; b ] ->
                match eval env a, eval env b with
                | Some va, Some vb -> primOp op va vb
                | _ -> None
            | _ -> None
        | _ ->
            // Ordinary function application: head must evaluate to a closure.
            match eval env f with
            | Some (VClosure (ps, body, cenv)) ->
                match eval env arg with
                | Some av -> applyClosure ps body cenv av
                | None -> None
            | _ -> None

    and evalProjection (env: Env) (receiver: Node) (m: Node) : Value option =
        match m.Exp with
        | Identifier (Resolved (id, _)) ->
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

    and applyClosure (ps: Guid list) (body: Node) (cenv: Env) (argv: Value) : Value option =
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

    and evalBinding (env: Env) (binding: Node) : Env option =
        match binding.Exp with
        | Local (_, inner) -> evalBinding env inner
        | Apply (f, E (Tuple [ lhs; rhs ])) when opName f = Some "_=_" ->
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
        | "_<_" | "_>_" | "_<=_" | "_>=_" | "_==_"
        | "_&&_" | "_||_" | "_&_" | "_|_" -> true
        | _ -> false

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
    | VClosure _ -> false
