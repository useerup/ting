/// First-order terms and formulas after Harrison, "Handbook of Practical Logic and Automated Reasoning",
/// without implication and equivalence (they are expressed with `Not`, `And` and `Or`).
module Ting.Logic.Formula

type Term =
    | Var of string
    | Fn of string * Term list

type Formula =
    | False
    | True
    | Atom of string * Term list
    | Not of Formula
    | And of Formula * Formula
    | Or of Formula * Formula
    | Forall of string * Formula
    | Exists of string * Formula

let implies (p: Formula) (q: Formula) = Or(Not p, q)

let iff (p: Formula) (q: Formula) = And(implies p q, implies q p)

let listConj (fms: Formula list) =
    match List.rev fms with
    | [] -> True
    | last :: rest -> List.fold (fun acc p -> And(p, acc)) last rest

let listDisj (fms: Formula list) =
    match List.rev fms with
    | [] -> False
    | last :: rest -> List.fold (fun acc p -> Or(p, acc)) last rest

let rec fvt (t: Term) : Set<string> =
    match t with
    | Var x -> Set.singleton x
    | Fn(_, args) -> args |> List.map fvt |> Set.unionMany

let rec fv (fm: Formula) : Set<string> =
    match fm with
    | False
    | True -> Set.empty
    | Atom(_, args) -> args |> List.map fvt |> Set.unionMany
    | Not p -> fv p
    | And(p, q)
    | Or(p, q) -> Set.union (fv p) (fv q)
    | Forall(x, p)
    | Exists(x, p) -> Set.remove x (fv p)

let rec private functionsOfTerm (t: Term) : Set<string * int> =
    match t with
    | Var _ -> Set.empty
    | Fn(f, args) -> Set.add (f, args.Length) (args |> List.map functionsOfTerm |> Set.unionMany)

/// Function symbols with their arities.
let rec functions (fm: Formula) : Set<string * int> =
    match fm with
    | False
    | True -> Set.empty
    | Atom(_, args) -> args |> List.map functionsOfTerm |> Set.unionMany
    | Not p
    | Forall(_, p)
    | Exists(_, p) -> functions p
    | And(p, q)
    | Or(p, q) -> Set.union (functions p) (functions q)

let generalize (fm: Formula) = Set.foldBack (fun x p -> Forall(x, p)) (fv fm) fm

let rec specialize (fm: Formula) =
    match fm with
    | Forall(_, p) -> specialize p
    | _ -> fm

let rec tsubst (env: Map<string, Term>) (t: Term) : Term =
    match t with
    | Var x -> env |> Map.tryFind x |> Option.defaultValue t
    | Fn(f, args) -> Fn(f, List.map (tsubst env) args)

/// A name like x not in vars, made by appending a number so it stays a valid Ting identifier.
let variant (x: string) (vars: Set<string>) =
    if Set.contains x vars then
        Seq.initInfinite (fun n -> x + string (n + 1)) |> Seq.find (fun v -> not (Set.contains v vars))
    else
        x

/// Capture-avoiding substitution of terms for free variables.
let rec subst (env: Map<string, Term>) (fm: Formula) : Formula =
    match fm with
    | False
    | True -> fm
    | Atom(p, args) -> Atom(p, List.map (tsubst env) args)
    | Not p -> Not(subst env p)
    | And(p, q) -> And(subst env p, subst env q)
    | Or(p, q) -> Or(subst env p, subst env q)
    | Forall(x, p) -> substQuantified env (fun (x, p) -> Forall(x, p)) x p
    | Exists(x, p) -> substQuantified env (fun (x, p) -> Exists(x, p)) x p

and private substQuantified env quant x p =
    let captures =
        Set.remove x (fv p)
        |> Set.exists (fun y -> Set.contains x (fvt (tsubst env (Var y))))
    let x' = if captures then variant x (fv (subst (Map.remove x env) p)) else x
    quant (x', subst (Map.add x (Var x') env) p)

let isNegative (lit: Formula) =
    match lit with
    | Not _ -> true
    | _ -> false

let negate (lit: Formula) =
    match lit with
    | Not p -> p
    | p -> Not p

// ---- Printing in Ting syntax ----

/// The set quantifiers range over when printed; a placeholder until the specification names a universe (SET-01).
let universe = "any"

let private comparisons = set [ "<"; "<="; ">"; ">="; ":" ]

let private paren (b: bool) (s: string) = if b then "(" + s + ")" else s

/// Binding power of the arithmetic operators printed infix; `^` is right-associative.
let private arithmetic = Map [ "+", 1; "-", 1; "*", 2; "/", 2; "%", 2; "^", 3 ]

// Term binding power, loosest first: `+ -` 1, `* / %` 2, `^` 3, juxtaposition 5, negation 6, atoms 7.
let rec private showTerm (prec: int) (t: Term) =
    match t with
    | Var x
    | Fn(x, []) -> x
    | Fn("()", args) ->
        // A leading `-` right after `(` would be subtraction, so it gets a space.
        let items = args |> List.map (showTerm 0) |> List.map (fun s -> if s.StartsWith "-" then " " + s else s)
        "(" + String.concat ", " items + ")"
    | Fn("-", [ a ]) -> paren (prec > 6) ("-" + showTerm 7 a)
    | Fn(op, [ a; b ]) when arithmetic.ContainsKey op ->
        let p = arithmetic[op]
        let left, right = if op = "^" then p + 1, p else p, p + 1
        paren (prec > p) (showTerm left a + " " + op + " " + showTerm right b)
    | Fn(f, [ a ]) -> paren (prec > 5) (f + " " + showTerm 6 a)
    | Fn(f, args) -> paren (prec > 5) (f + "(" + (args |> List.map (showTerm 0) |> String.concat ", ") + ")")

let termToString (t: Term) = showTerm 0 t

// Binding power, loosest first: `=` 0, `all` 1, `|` 2, `&` 3, `!` 4, comparisons 5, juxtaposition 6.
let rec private show (prec: int) (fm: Formula) =
    match fm with
    | False -> "false"
    | True -> "true"
    | Atom("=", [ a; b ]) -> paren (prec > 0) (termToString a + " = " + termToString b)
    | Atom(p, [ a; b ]) when comparisons.Contains p -> paren (prec > 5) (termToString a + " " + p + " " + termToString b)
    | Atom(p, args) -> termToString (Fn(p, args))
    | Not p -> "!" + show 6 p
    | And(Exists _ as p, q) -> paren (prec > 3) (paren true (show 0 p) + " & " + show 3 q)
    | And(p, q) -> paren (prec > 3) (show 3 p + " & " + show 3 q)
    | Or(p, q) -> paren (prec > 2) (show 2 p + " | " + show 2 q)
    | Forall _ -> paren (prec > 1) (quantified fm)
    | Exists _ -> paren (prec > 3) (quantified fm)

/// `forall x y. p` prints as `x : any & y : any all p`, and `exists x y. p` as `x : any & y : any & p`.
and private quantified (fm: Formula) =
    let rec gather fm acc =
        match fm with
        | Forall(x, (Forall _ as p))
        | Exists(x, (Exists _ as p)) -> gather p (x :: acc)
        | Forall(x, p)
        | Exists(x, p) -> List.rev (x :: acc), p
        | _ -> List.rev acc, fm
    let vars, body = gather fm []
    let declarations = vars |> List.map (fun v -> v + " : " + universe) |> String.concat " & "
    match fm with
    | Forall _ -> declarations + " all " + show 1 body
    | _ -> declarations + " & " + show 3 body

let toString (fm: Formula) = show 0 fm
