/// Given-clause resolution with factoring, subsumption and replacement (Harrison, section 3.11).
module Ting.Logic.Resolution

open Ting.Logic.Formula
open Ting.Logic.Normal
open Ting.Logic.Unify

type Outcome =
    /// The formula is valid.
    | Proved
    /// The clauses saturated without the empty clause: the formula is not valid.
    | NotValid
    /// The step limit was reached first.
    | Unknown

let rec private mgu (lits: Formula list) (env: Env) : Env option =
    match lits with
    | a :: (_ :: _ as rest) -> unifyLiterals env (a, List.head rest) |> Option.bind (mgu rest)
    | _ -> Some(solve env)

let private unifiable (p: Formula) (q: Formula) = (unifyLiterals Map.empty (p, q)).IsSome

let private rename (prefix: string) (cl: Clause) : Clause =
    let env = cl |> Seq.map fv |> Set.unionMany |> Seq.map (fun x -> x, Var(prefix + x)) |> Map.ofSeq
    cl |> Set.map (subst env)

let rec private allSubsets (xs: 'a list) : 'a list list =
    match xs with
    | [] -> [ [] ]
    | x :: rest ->
        let s = allSubsets rest
        s @ (s |> List.map (fun t -> x :: t))

/// Resolvents on literal p of cl1, factoring both sides by resolving several literals at once.
let private resolvents (cl1: Clause) (cl2: Clause) (p: Formula) (acc: Clause list) =
    let ps2 = cl2 |> Set.filter (unifiable (negate p)) |> Set.toList
    if ps2.IsEmpty then
        acc
    else
        let ps1 = cl1 |> Set.filter (fun q -> q <> p && unifiable p q) |> Set.toList
        [ for s1 in allSubsets ps1 do
              let s1 = p :: s1
              for s2 in allSubsets ps2 |> List.filter (List.isEmpty >> not) do
                  match mgu (s1 @ List.map negate s2) Map.empty with
                  | Some env ->
                      yield Set.union (Set.difference cl1 (set s1)) (Set.difference cl2 (set s2)) |> Set.map (subst env)
                  | None -> () ]
        @ acc

let private resolveClauses (cl1: Clause) (cl2: Clause) =
    let c1 = rename "x" cl1
    let c2 = rename "y" cl2
    Set.foldBack (fun p acc -> resolvents c1 c2 p acc) c1 []

let private subsumes (cl1: Clause) (cl2: Clause) =
    let targets = Set.toList cl2
    let rec go env lits =
        match lits with
        | [] -> true
        | l1 :: rest ->
            targets
            |> List.exists (fun l2 ->
                match matchLiterals env (l1, l2) with
                | Some env' -> go env' rest
                | None -> false)
    go Map.empty (Set.toList cl1)

/// Adds cl, replacing the first clause it subsumes.
let private replace (cl: Clause) (clauses: Clause list) =
    match List.tryFindIndex (subsumes cl) clauses with
    | Some i -> clauses |> List.mapi (fun j c -> if j = i then cl else c)
    | None -> clauses @ [ cl ]

let private incorporate (used: Clause list) (cl: Clause) (unused: Clause list) =
    if trivial cl || List.exists (fun c -> subsumes c cl) used || List.exists (fun c -> subsumes c cl) unused then
        unused
    else
        replace cl unused

let rec private termWeight (t: Term) =
    match t with
    | Var _ -> 1
    | Fn(_, args) -> 1 + List.sumBy termWeight args

let private weight (cl: Clause) =
    cl |> Seq.sumBy (fun lit ->
        match lit with
        | Atom(_, args)
        | Not(Atom(_, args)) -> 1 + List.sumBy termWeight args
        | _ -> 1)

/// Every few steps the oldest clause is chosen so that heavy clauses are not starved.
let private fairness = 5

/// Picks the next given clause: usually the lightest, sometimes the oldest.
let private select (steps: int) (unused: Clause list) =
    if steps % fairness = fairness - 1 then
        List.head unused, List.tail unused
    else
        let i = unused |> List.mapi (fun i c -> weight c, i) |> List.min |> snd
        unused[i], List.removeAt i unused

let private resolutionLoop (limit: int) (clauses: Clause list) =
    let rec loop used unused steps =
        match unused with
        | [] -> NotValid
        | _ when steps >= limit -> Unknown
        | _ ->
            let cl, rest = select steps unused
            let used' = if List.contains cl used then used else cl :: used
            let news = used' |> List.collect (resolveClauses cl)
            if news |> List.exists Set.isEmpty then Proved
            else loop used' (List.foldBack (incorporate used') news rest) (steps + 1)
    loop [] clauses 0

/// Refutes a formula's clause form; Proved means the formula is unsatisfiable.
let refute (limit: int) (fm: Formula) = resolutionLoop limit (simpcnf (specialize (pnf fm)))

/// Proves validity by refuting each disjunct of the Skolemized negation.
let prove (limit: int) (fm: Formula) : Outcome =
    let outcomes =
        askolemize (Not(generalize fm))
        |> simpdnf
        |> List.map (fun d -> refute limit (listConj (Set.toList d)))
    if outcomes |> List.forall ((=) Proved) then Proved
    elif outcomes |> List.contains NotValid then NotValid
    else Unknown
