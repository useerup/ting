/// Paramodulation: rewriting a clause with an equation from another clause.
module Ting.Logic.Paramodulation

open Ting.Logic.Formula
open Ting.Logic.Normal
open Ting.Logic.Unify

/// The clause `x = x`, which paramodulation needs in place of the other equality axioms.
let reflexivity: Clause = Set.singleton (Atom("=", [ Var "x"; Var "x" ]))

let rec usesEquality (fm: Formula) =
    match fm with
    | Atom("=", _) -> true
    | Not p
    | Forall(_, p)
    | Exists(_, p) -> usesEquality p
    | And(p, q)
    | Or(p, q) -> usesEquality p || usesEquality q
    | _ -> false

/// Each way a non-variable subterm of tm unifies with l, as the unifier and tm with that subterm replaced by r.
let rec private overlaps (l: Term, r: Term) (tm: Term) : (Env * Term) list =
    match tm with
    | Var _ -> []
    | Fn(f, args) ->
        let here =
            match unify Map.empty [ l, tm ] with
            | Some env -> [ env, r ]
            | None -> []
        here @ (overlapArgs (l, r) args |> List.map (fun (env, args') -> env, Fn(f, args')))

and private overlapArgs (l: Term, r: Term) (args: Term list) : (Env * Term list) list =
    args
    |> List.mapi (fun i arg -> overlaps (l, r) arg |> List.map (fun (env, arg') -> env, List.updateAt i arg' args))
    |> List.concat

let private overlapLiteral (l: Term, r: Term) (lit: Formula) : (Env * Formula) list =
    match lit with
    | Atom(p, args) -> overlapArgs (l, r) args |> List.map (fun (env, args') -> env, Atom(p, args'))
    | Not(Atom(p, args)) -> overlapArgs (l, r) args |> List.map (fun (env, args') -> env, Not(Atom(p, args')))
    | _ -> []

let private isMaximal = Ordering.isMaximal

/// Rewrites into `target` with each equation of `source`, in both directions, never replacing a term by a greater one
/// (ordered paramodulation). Only maximal literals take part. The clauses must not share variables.
let paramodulate (source: Clause) (target: Clause) : Clause list =
    [ for eq in source do
          match eq with
          | Atom("=", [ s; t ]) when isMaximal source eq ->
              let rest = Set.remove eq source
              for l, r in [ s, t; t, s ] do
                  for lit in target do
                      if isMaximal target lit then
                          for env, lit' in overlapLiteral (l, r) lit do
                              let env = solve env
                              if not (Ordering.greater (tsubst env r) (tsubst env l)) then
                                  yield Set.union rest (Set.add lit' (Set.remove lit target)) |> Set.map (subst env)
          | _ -> () ]

/// From `s = t | s' = t' | C` with s and s' unifiable, derives `t != t' | s' = t' | C`.
let equalityFactors (cl: Clause) : Clause list =
    let equations = cl |> Set.toList |> List.filter (function Atom("=", [ _; _ ]) -> true | _ -> false)
    [ for e1 in equations do
          for e2 in equations do
              match e1, e2 with
              | Atom(_, [ s1; t1 ]), Atom(_, [ s2; t2 ]) when e1 <> e2 ->
                  for s, t in [ s1, t1; t1, s1 ] do
                      for s', t' in [ s2, t2; t2, s2 ] do
                          match fullUnify [ s, s' ] with
                          | Some env -> yield Set.remove e1 cl |> Set.add (Not(Atom("=", [ t; t' ]))) |> Set.map (subst env)
                          | None -> ()
              | _ -> () ]

/// Both directions of every unit equation, for rewriting.
let rewriteRules (clauses: Clause list) : (Term * Term) list =
    [ for cl in clauses do
          if cl.Count = 1 then
              match Seq.head cl with
              | Atom("=", [ s; t ]) when s <> t ->
                  if not s.IsVar then yield s, t
                  if not t.IsVar then yield t, s
              | _ -> () ]

/// Rewrites innermost first, applying an instance of a rule only where it makes the term smaller.
let rec private normalize (rules: (Term * Term) list) (t: Term) =
    match t with
    | Var _ -> t
    | Fn(f, args) ->
        let t' = Fn(f, List.map (normalize rules) args)
        let step (l: Term, r: Term) =
            match termMatch Map.empty [ l, t' ] with
            | Some env ->
                let r' = tsubst env r
                if Ordering.greater t' r' then Some r' else None
            | None -> None
        match List.tryPick step rules with
        | Some r' -> normalize rules r'
        | None -> t'

/// Demodulates a clause. None when it becomes a tautology; literals `t != t` are dropped.
let simplify (rules: (Term * Term) list) (cl: Clause) : Clause option =
    let rewrite lit =
        match lit with
        | Atom(p, args) -> Atom(p, List.map (normalize rules) args)
        | Not(Atom(p, args)) -> Not(Atom(p, List.map (normalize rules) args))
        | _ -> lit
    let cl' = if rules.IsEmpty then cl else Set.map rewrite cl
    let isTautology lit =
        match lit with
        | Atom("=", [ s; t ]) -> s = t
        | _ -> false
    let isContradiction lit =
        match lit with
        | Not(Atom("=", [ s; t ])) -> s = t
        | _ -> false
    if Set.exists isTautology cl' then None else Some(Set.filter (isContradiction >> not) cl')
