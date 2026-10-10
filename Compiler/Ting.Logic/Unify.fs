/// Syntactic unification with an occurs check, and one-way matching.
module Ting.Logic.Unify

open Ting.Logic.Formula

type Env = Map<string, Term>

/// Some true: t is x under env. Some false: x does not occur in t. None: x occurs inside t (cyclic).
let rec private isTrivial (env: Env) (x: string) (t: Term) : bool option =
    match t with
    | Var y when y = x -> Some true
    | Var y ->
        match Map.tryFind y env with
        | Some t' -> isTrivial env x t'
        | None -> Some false
    | Fn(_, args) -> if args |> List.exists (fun a -> isTrivial env x a <> Some false) then None else Some false

let rec unify (env: Env) (eqs: (Term * Term) list) : Env option =
    match eqs with
    | [] -> Some env
    | (Fn(f, fa), Fn(g, ga)) :: rest ->
        if f = g && fa.Length = ga.Length then unify env (List.zip fa ga @ rest) else None
    | (Var x, t) :: rest
    | (t, Var x) :: rest ->
        match Map.tryFind x env with
        | Some bound -> unify env ((bound, t) :: rest)
        | None ->
            match isTrivial env x t with
            | None -> None
            | Some true -> unify env rest
            | Some false -> unify (Map.add x t env) rest

/// Fully applies the triangular substitution produced by unify.
let rec solve (env: Env) : Env =
    let env' = env |> Map.map (fun _ t -> tsubst env t)
    if env' = env then env else solve env'

let fullUnify (eqs: (Term * Term) list) = unify Map.empty eqs |> Option.map solve

let rec unifyLiterals (env: Env) (p: Formula, q: Formula) : Env option =
    match p, q with
    | Atom(p1, a1), Atom(p2, a2) -> unify env [ Fn(p1, a1), Fn(p2, a2) ]
    | Not p, Not q -> unifyLiterals env (p, q)
    | False, False -> Some env
    | _ -> None

/// One-way matching: binds only variables of the left-hand terms.
let rec termMatch (env: Env) (eqs: (Term * Term) list) : Env option =
    match eqs with
    | [] -> Some env
    | (Fn(f, fa), Fn(g, ga)) :: rest when f = g && fa.Length = ga.Length -> termMatch env (List.zip fa ga @ rest)
    | (Var x, t) :: rest ->
        match Map.tryFind x env with
        | None -> termMatch (Map.add x t env) rest
        | Some bound when bound = t -> termMatch env rest
        | Some _ -> None
    | _ -> None

let matchLiterals (env: Env) (p: Formula, q: Formula) : Env option =
    match p, q with
    | Atom(p1, a1), Atom(p2, a2)
    | Not(Atom(p1, a1)), Not(Atom(p2, a2)) -> termMatch env [ Fn(p1, a1), Fn(p2, a2) ]
    | _ -> None
