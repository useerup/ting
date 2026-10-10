/// Simplification and normal forms: NNF, prenex, Skolem, DNF and CNF clause sets.
module Ting.Logic.Normal

open Ting.Logic.Formula

let private simplify1 (fm: Formula) =
    match fm with
    | Forall(x, p)
    | Exists(x, p) -> if Set.contains x (fv p) then fm else p
    | Not False -> True
    | Not True -> False
    | Not(Not p) -> p
    | And(_, False)
    | And(False, _) -> False
    | And(p, True)
    | And(True, p) -> p
    | Or(p, False)
    | Or(False, p) -> p
    | Or(_, True)
    | Or(True, _) -> True
    | _ -> fm

let rec simplify (fm: Formula) =
    match fm with
    | Not p -> simplify1 (Not(simplify p))
    | And(p, q) -> simplify1 (And(simplify p, simplify q))
    | Or(p, q) -> simplify1 (Or(simplify p, simplify q))
    | Forall(x, p) -> simplify1 (Forall(x, simplify p))
    | Exists(x, p) -> simplify1 (Exists(x, simplify p))
    | _ -> fm

let rec nnf (fm: Formula) =
    match fm with
    | And(p, q) -> And(nnf p, nnf q)
    | Or(p, q) -> Or(nnf p, nnf q)
    | Not(Not p) -> nnf p
    | Not(And(p, q)) -> Or(nnf (Not p), nnf (Not q))
    | Not(Or(p, q)) -> And(nnf (Not p), nnf (Not q))
    | Forall(x, p) -> Forall(x, nnf p)
    | Exists(x, p) -> Exists(x, nnf p)
    | Not(Forall(x, p)) -> Exists(x, nnf (Not p))
    | Not(Exists(x, p)) -> Forall(x, nnf (Not p))
    | _ -> fm

let rec private pullQuantifiers (fm: Formula) =
    let pull (left, right) quant op x y p q =
        let z = variant x (fv fm)
        let p' = if left then subst (Map.ofList [ x, Var z ]) p else p
        let q' = if right then subst (Map.ofList [ y, Var z ]) q else q
        quant z (pullQuantifiers (op p' q'))
    let forall' x p = Forall(x, p)
    let exists' x p = Exists(x, p)
    let and' p q = And(p, q)
    let or' p q = Or(p, q)
    match fm with
    | And(Forall(x, p), Forall(y, q)) -> pull (true, true) forall' and' x y p q
    | Or(Exists(x, p), Exists(y, q)) -> pull (true, true) exists' or' x y p q
    | And(Forall(x, p), q) -> pull (true, false) forall' and' x x p q
    | And(p, Forall(y, q)) -> pull (false, true) forall' and' y y p q
    | Or(Forall(x, p), q) -> pull (true, false) forall' or' x x p q
    | Or(p, Forall(y, q)) -> pull (false, true) forall' or' y y p q
    | And(Exists(x, p), q) -> pull (true, false) exists' and' x x p q
    | And(p, Exists(y, q)) -> pull (false, true) exists' and' y y p q
    | Or(Exists(x, p), q) -> pull (true, false) exists' or' x x p q
    | Or(p, Exists(y, q)) -> pull (false, true) exists' or' y y p q
    | _ -> fm

let rec private prenex (fm: Formula) =
    match fm with
    | Forall(x, p) -> Forall(x, prenex p)
    | Exists(x, p) -> Exists(x, prenex p)
    | And(p, q) -> pullQuantifiers (And(prenex p, prenex q))
    | Or(p, q) -> pullQuantifiers (Or(prenex p, prenex q))
    | _ -> fm

let pnf (fm: Formula) = prenex (nnf (simplify fm))

/// Replaces existentials by Skolem functions of the enclosing universals (input in NNF).
let rec private skolem (fm: Formula) (fns: Set<string>) : Formula * Set<string> =
    match fm with
    | Exists(y, p) ->
        let xs = fv fm
        let f = variant (if xs.IsEmpty then "c_" + y else "f_" + y) fns
        let fx = Fn(f, xs |> Set.toList |> List.map Var)
        skolem (subst (Map.ofList [ y, fx ]) p) (Set.add f fns)
    | Forall(x, p) ->
        let p', fns' = skolem p fns
        Forall(x, p'), fns'
    | And(p, q) ->
        let p', fns' = skolem p fns
        let q', fns'' = skolem q fns'
        And(p', q'), fns''
    | Or(p, q) ->
        let p', fns' = skolem p fns
        let q', fns'' = skolem q fns'
        Or(p', q'), fns''
    | _ -> fm, fns

let askolemize (fm: Formula) =
    fst (skolem (nnf (simplify fm)) (functions fm |> Set.map fst))

let skolemize (fm: Formula) = specialize (pnf (askolemize fm))

type Clause = Set<Formula>

let rec private purednf (fm: Formula) : Set<Clause> =
    match fm with
    | And(p, q) -> set [ for a in purednf p do for b in purednf q -> Set.union a b ]
    | Or(p, q) -> Set.union (purednf p) (purednf q)
    | _ -> Set.singleton (Set.singleton fm)

/// A clause or conjunct containing a literal and its negation.
let trivial (lits: Clause) =
    lits |> Set.exists (fun l -> isNegative l && lits.Contains(negate l))

let private subsumedBySmaller (all: Set<Clause>) (c: Clause) =
    all |> Set.exists (fun c' -> Set.isProperSubset c' c)

/// Disjunctive normal form as a list of conjunct sets.
let simpdnf (fm: Formula) : Clause list =
    match fm with
    | False -> []
    | True -> [ Set.empty ]
    | _ ->
        let djs = purednf (nnf fm) |> Set.filter (trivial >> not)
        djs |> Set.filter (subsumedBySmaller djs >> not) |> Set.toList

/// Conjunctive normal form as a list of clauses (sets of literals).
let simpcnf (fm: Formula) : Clause list =
    match fm with
    | False -> [ Set.empty ]
    | True -> []
    | _ ->
        let cjs = purednf (nnf (Not fm)) |> Set.map (Set.map negate) |> Set.filter (trivial >> not)
        cjs |> Set.filter (subsumedBySmaller cjs >> not) |> Set.toList
