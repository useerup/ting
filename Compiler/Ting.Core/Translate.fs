/// Translation of a bound program into a first-order formula (plan phase 3).
/// `Value e v` is the formula "e can yield v": several solutions are several v, undefinedness is no v.
/// `Goal e` and `Refute e` are the formulas "e is true" and "e is false" for propositions.
module Ting.Core.Translate

open System.Collections.Generic
open Ting.Syntax.Source
open Ting.Syntax.Syntax
open Ting.Binding.Model
open Ting.Logic.Formula

type Diagnostic = { Message: string; Span: Span }

/// For all `Vars`, `Head` holds exactly when `Body` does, or, when not `Exact`, `Head` implies `Body`.
/// Defines `app` for function values and `:` for sets, and states injectivity of constructors.
type Axiom =
    { Vars: string list
      Head: Formula
      Body: Formula
      Exact: bool }

    member this.Formula =
        let statement = if this.Exact then iff this.Head this.Body else implies this.Head this.Body
        List.foldBack (fun x p -> Forall(x, p)) this.Vars statement

type Translation =
    { Value: Formula
      Axioms: Axiom list
      /// Literals that denote different values.
      Literals: Term list
      Diagnostics: Diagnostic list }

    /// What the prover may assume about the symbols in `Value`.
    member this.Assumptions =
        [ for a in this.Axioms -> a.Formula
          for i, a in List.indexed this.Literals do
              for b in List.skip (i + 1) this.Literals -> Not(Atom("=", [ a; b ])) ]

/// The free variable standing for the value of the whole program.
let valueName = "value"

let private builtinSets =
    set [ "bool"; "char"; "string"; "byte"; "sbyte"; "short"; "ushort"; "int"; "uint"; "long"; "ulong"
          "float"; "double"; "decimal"; "unit"; "Sets"; "Funcs" ]

let private relations = set [ "="; "=="; "!="; "<"; "<="; ">"; ">="; ":"; "!:"; "?=" ]
let private logical = set [ "&"; "&&"; "&&&"; "|"; "||"; "|||" ]
let private arithmetic = set [ "+"; "-"; "*"; "/"; "%"; "^" ]
let private claims = set [ "total"; "partial"; "determ"; "nondeterm" ]

let rec private isPair (e: Expr) =
    match e with
    | Group(_, inner) -> isPair inner
    | Prefix(_, op, inner) when op.Text = "let" -> isPair inner
    | Infix(_, op, _, _) -> op.Text = "->"
    | _ -> false

let private tTrue = Fn("true", [])
let private tFalse = Fn("false", [])

/// Distinct integer or boolean literals, which can never be equal.
let private distinctLiterals (a: Term) (b: Term) =
    let integer (s: string) = s <> "" && Seq.forall System.Char.IsDigit s
    match a, b with
    | Fn(x, []), Fn(y, []) when integer x && integer y -> x.TrimStart '0' <> y.TrimStart '0'
    | Fn(x, []), Fn(y, []) when (x = "true" || x = "false") && (y = "true" || y = "false") -> x <> y
    | _ -> false

let private neg (fm: Formula) =
    match fm with
    | True -> False
    | False -> True
    | Not p -> p
    | p -> Not p

let private conj (fms: Formula list) =
    if List.contains False fms then False
    else
        match fms |> List.filter ((<>) True) with
        | [] -> True
        | fs -> List.reduceBack (fun p q -> And(p, q)) fs

let private disj (fms: Formula list) =
    if List.contains True fms then True
    else
        match fms |> List.filter ((<>) False) with
        | [] -> False
        | fs -> List.reduceBack (fun p q -> Or(p, q)) fs

/// Ordered pairs and tuples, whose values are equal exactly when their components are.
let private injective = set [ "->"; "()" ]

let rec private eq (a: Term) (b: Term) =
    match a, b with
    | _ when a = b -> True
    | _ when distinctLiterals a b -> False
    | Fn(f, xs), Fn(g, ys) when f = g && injective.Contains f && xs.Length = ys.Length -> conj (List.map2 eq xs ys)
    | _ -> Atom("=", [ a; b ])

let private quantify (make: string * Formula -> Formula) (names: string list) (body: Formula) =
    match body with
    | True
    | False -> body
    | _ ->
        let free = fv body
        List.foldBack (fun x p -> if Set.contains x free then make (x, p) else p) names body

let private exists = quantify Exists
let private forall = quantify Forall

let private relation (op: string) (a: Term) (b: Term) =
    match op with
    | "!=" -> neg (eq a b)
    | "!:" -> neg (Atom(":", [ a; b ]))
    // Attributes are not modelled yet, so `==` and `?=` compare like `=`.
    | "==" | "?=" -> eq a b
    | _ -> Atom(op, [ a; b ])

let private frameKind (kind: FrameKind) =
    match kind with
    | Root -> "root"
    | Let -> "let"
    | Publish s
    | Local s
    | Fields s
    | TupleScope s
    | ExpressionScope s -> s

type private Translator(bound: Bound) =
    let bindings = Dictionary<Span, Binding>()
    do
        for o in bound.Occurrences do
            bindings[o.Span] <- o.Binding
    let decls = bound.Decls |> List.map (fun d -> d.Id, d) |> dict
    let frames = Dictionary<struct (int * string), FrameId>()
    do
        for f in bound.Frames do
            frames.TryAdd(struct (f.At.Offset, frameKind f.Kind), f.Id) |> ignore
    let diagnostics = ResizeArray<Diagnostic>()
    let axioms = ResizeArray<Axiom>()
    let literals = ResizeArray<Term>()
    /// Terms of lambdas and set definitions, so each gets its axioms once.
    let terms = Dictionary<Span, Term>()
    let constructors = HashSet<string * int>()
    let mutable counter = 0
    let mutable lambdas = 0
    let mutable sets = 0
    let mutable fnAxiom = false

    let fresh () =
        counter <- counter + 1
        $"v{counter}"

    let addAxiom (vars: string list) (head: Formula) (body: Formula) =
        axioms.Add { Vars = vars; Head = head; Body = body; Exact = true }

    let literal (t: Term) =
        if not (literals.Contains t) then literals.Add t
        t

    /// Equal values of a constructor have equal arguments. The converse is congruence of equality.
    let constructor (name: string) (args: Term list) =
        if not args.IsEmpty && constructors.Add((name, args.Length)) then
            let xs = [ for i in 1 .. args.Length -> $"x{i}" ]
            let ys = [ for i in 1 .. args.Length -> $"y{i}" ]
            axioms.Add
                { Vars = xs @ ys
                  Head = Atom("=", [ Fn(name, List.map Var xs); Fn(name, List.map Var ys) ])
                  Body = conj (List.map2 (fun x y -> Atom("=", [ Var x; Var y ])) xs ys)
                  Exact = false }
        Fn(name, args)

    let pair (origin: Term) (result: Term) = constructor "->" [ origin; result ]

    /// An existential over a fresh variable, eliminated when a conjunct defines the variable.
    let someFresh (x: string) (body: Formula) =
        let rec conjuncts fm =
            match fm with
            | And(p, q) -> conjuncts p @ conjuncts q
            | p -> [ p ]
        let parts = conjuncts body
        let definition =
            parts
            |> List.tryPick (fun p ->
                match p with
                | Atom("=", [ Var y; t ])
                | Atom("=", [ t; Var y ]) when y = x && not (Set.contains x (fvt t)) -> Some(p, t)
                | _ -> None)
        match definition with
        | Some(p, t) -> parts |> List.filter ((<>) p) |> List.map (subst (Map.ofList [ x, t ])) |> conj
        | None when Set.contains x (fv body) -> Exists(x, body)
        | None -> body

    let varName (d: Decl) = $"{d.Name}_{d.Id}"

    let unsupported (span: Span) (what: string) =
        diagnostics.Add { Message = $"not yet supported: {what}"; Span = span }
        Atom("unsupported", [])

    /// The variables bound by the frame an operator introduces.
    let declaredIn (kind: string) (at: Position) =
        match frames.TryGetValue(struct (at.Offset, kind)) with
        | true, f ->
            bound.Decls
            |> List.filter (fun d ->
                List.contains f d.Frames
                && match d.Kind with
                   | Hosted _ -> false
                   | _ -> true)
            |> List.map varName
        | _ -> []

    member _.Diagnostics = List.ofSeq diagnostics
    member _.Axioms = List.ofSeq axioms
    member _.Literals = List.ofSeq literals

    member _.Name(span: Span, name: string) : Term option =
        match bindings.TryGetValue span with
        | true, (Declares id | References id) -> Some(if id < 0 then Fn(name, []) else Var(varName decls[id]))
        | true, Unresolved -> Some(Fn(name, []))
        | true, Deferred _ ->
            unsupported span $"'{name}' needs semantic analysis" |> ignore
            Some(Fn(name, []))
        | _ -> None

    member this.IsProposition(e: Expr) =
        match e with
        | Group(_, inner) -> this.IsProposition inner
        | Bool _ -> true
        | Infix(_, op, _, _) -> relations.Contains op.Text || logical.Contains op.Text || op.Text = "all"
        | Prefix(_, op, _) -> op.Text = "!"
        | _ -> false

    /// Some (conditions, term) when e has exactly one value wherever the conditions hold.
    member this.Functional(e: Expr) : (Formula list * Term) option =
        match e with
        | Identifier(span, name) -> this.Name(span, name) |> Option.map (fun t -> [], t)
        | This span -> this.Name(span, "this") |> Option.map (fun t -> [], t)
        | Number(_, text, _) -> Some([], literal (Fn(text, [])))
        | Bool(_, b) -> Some([], literal (if b then tTrue else tFalse))
        | String(_, parts) when parts |> List.forall (function Text _ -> true | Hole _ -> false) ->
            let text = parts |> List.map (function Text s -> s | Hole _ -> "") |> String.concat ""
            Some([], literal (Fn("\"" + text + "\"", [])))
        | Group(_, inner) -> this.Functional inner
        | Tuple(_, items) ->
            let parts = items |> List.map this.Functional
            if List.forall Option.isSome parts then
                let parts = parts |> List.map Option.get
                Some(List.collect fst parts, constructor "()" (List.map snd parts))
            else None
        | Prefix(_, op, a) when op.Text = "-" -> this.Functional a |> Option.map (fun (cs, t) -> cs, Fn("-", [ t ]))
        | Prefix(_, op, a) when claims.Contains op.Text -> this.Functional a
        | Prefix(_, op, a) when op.Text = "fn" -> this.Functional a |> Option.map (fun (cs, t) -> cs, this.Fn t)
        | Infix(span, op, p, b) when op.Text = "=>" -> Some([], this.Lambda(span, op, p, b))
        | Set(span, items) -> Some([], this.SetDefinition(span, items))
        | Infix(_, op, a, b) when op.Text = "->" ->
            (match this.Functional a, this.Functional b with
             | Some(ca, ta), Some(cb, tb) -> Some(ca @ cb, pair ta tb)
             | _ -> None)
        | Infix(_, op, a, b) when arithmetic.Contains op.Text ->
            (match this.Functional a, this.Functional b with
             | Some(ca, ta), Some(cb, tb) -> Some(ca @ cb @ this.Guard(op.Text, tb), Fn(op.Text, [ ta; tb ]))
             | _ -> None)
        | _ -> None

    /// `fn s` is the function whose results at `a` are the `r` with `a -> r` in `s`.
    member _.Fn(set: Term) : Term =
        if not fnAxiom then
            fnAxiom <- true
            addAxiom [ "s"; "a"; "r" ] (Atom("app", [ Fn("fn", [ Var "s" ]); Var "a"; Var "r" ])) (Atom(":", [ pair (Var "a") (Var "r"); Var "s" ]))
        Fn("fn", [ set ])

    /// A lambda is a term over the variables it captures; applying it matches the pattern and evaluates the body.
    member this.Lambda(span: Span, op: Operator, pattern: Expr, body: Expr) : Term =
        match terms.TryGetValue span with
        | true, t -> t
        | _ ->
            lambdas <- lambdas + 1
            let name = $"lambda{lambdas}"
            let a, r = fresh (), fresh ()
            let definition =
                exists (declaredIn "=>" op.Span.Start) (conj [ this.Value pattern (Var a); this.Value body (Var r) ])
            let captured = fv definition |> Set.remove a |> Set.remove r |> Set.toList
            let term = Fn(name, List.map Var captured)
            terms[span] <- term
            addAxiom (captured @ [ a; r ]) (Atom("app", [ term; Var a; Var r ])) definition
            term

    /// A set definition is a term over the variables it captures, with its membership and its use as an identity function.
    /// In a set of ordered pairs the first contribution at an origin wins.
    member this.SetDefinition(span: Span, items: Expr list) : Term =
        match terms.TryGetValue span with
        | true, t -> t
        | _ ->
            sets <- sets + 1
            let name = $"set{sets}"
            // The members of a set of ordered pairs are stated on the pair, so the prover need not take members apart.
            let member', membership =
                if not items.IsEmpty && List.forall isPair items then
                    let o, r = fresh (), fresh ()
                    let alternatives =
                        items
                        |> List.mapi (fun i item ->
                            let earlier =
                                items
                                |> List.take i
                                |> List.map (fun e ->
                                    let r' = fresh ()
                                    neg (someFresh r' (this.Value e (pair (Var o) (Var r')))))
                            conj (this.Value item (pair (Var o) (Var r)) :: earlier))
                    [ o; r ], (pair (Var o) (Var r), disj alternatives)
                else
                    let y = fresh ()
                    [ y ], (Var y, disj (items |> List.map (fun item -> this.Value item (Var y))))
            let element, body = membership
            let captured = Set.difference (fv body) (set member') |> Set.toList
            let term = Fn(name, List.map Var captured)
            terms[span] <- term
            addAxiom (captured @ member') (Atom(":", [ element; term ])) body
            let a, r = fresh (), fresh ()
            addAxiom (captured @ [ a; r ]) (Atom("app", [ term; Var a; Var r ])) (conj [ Atom(":", [ Var a; term ]); eq (Var r) (Var a) ])
            term

    /// Definedness conditions of an arithmetic operator on its right operand.
    member _.Guard(op: string, divisor: Term) =
        if op = "/" || op = "%" then [ neg (eq divisor (Fn("0", []))) ] else []

    /// k applied to the value of e, introducing a variable only when e is not functional.
    member this.Operand (e: Expr) (k: Term -> Formula) : Formula =
        match this.Functional e with
        | Some(cs, t) -> conj (cs @ [ k t ])
        | None ->
            let x = fresh ()
            someFresh x (conj [ this.Value e (Var x); k (Var x) ])

    member this.Operands (es: Expr list) (k: Term list -> Formula) : Formula =
        match es with
        | [] -> k []
        | e :: rest -> this.Operand e (fun t -> this.Operands rest (fun ts -> k (t :: ts)))

    member this.Value (e: Expr) (v: Term) : Formula =
        match this.Functional e with
        | Some(cs, t) -> conj (cs @ [ eq v t ])
        | None ->
            match e with
            | Group(_, inner) -> this.Value inner v
            | Void _ -> False
            | Tuple(_, items) -> this.Operands items (fun ts -> eq v (constructor "()" ts))
            | Infix(_, op, a, b) when op.Text = "->" -> this.Operands [ a; b ] (fun ts -> eq v (pair ts[0] ts[1]))
            | Prefix(_, op, a) when op.Text = "fn" -> this.Operand a (fun t -> eq v (this.Fn t))
            | Prefix(_, op, a) when op.Text = "-" -> this.Operand a (fun t -> eq v (Fn("-", [ t ])))
            | Prefix(_, op, a) when op.Text = "let" -> exists (declaredIn "let" op.Span.Start) (this.Value a v)
            | Prefix(_, op, a) when claims.Contains op.Text -> this.Value a v
            | Infix(_, op, a, b) when arithmetic.Contains op.Text ->
                this.Operands [ a; b ] (fun ts -> conj (this.Guard(op.Text, ts[1]) @ [ eq v (Fn(op.Text, ts)) ]))
            | Infix(_, op, a, b) when op.Text = "in" ->
                exists (declaredIn "in" op.Span.Start) (conj [ this.Goal a; this.Value b v ])
            | Infix(_, op, a, b) when op.Text = "where" ->
                exists (declaredIn "where" op.Span.Start) (conj [ this.Value a v; this.Goal b ])
            | Infix(_, op, a, b) when op.Text = "\\" -> conj [ this.Value a v; this.Goal b ]
            | Infix(_, op, a, b) when op.Text = ";" -> disj [ this.Value a v; this.Value b v ]
            | Infix(_, op, a, b) when op.Text = ";;" -> disj [ this.Value a v; conj [ neg (this.Defined a); this.Value b v ] ]
            | Infix(_, op, a, b) when op.Text = "then" ->
                (match b with
                 | Infix(_, alt, yes, no) when alt.Text = "else" ->
                     disj [ conj [ this.Goal a; this.Value yes v ]; conj [ this.Refute a; this.Value no v ] ]
                 | _ -> conj [ this.Goal a; this.Value b v ])
            | Infix(_, op, a, b) when op.Text = "?" ->
                (match b with
                 | Section(_, rel, rhs) when relations.Contains rel.Text ->
                     conj [ this.Value a v; this.Operand rhs (fun t -> relation rel.Text v t) ]
                 | _ -> conj [ this.Value a v; this.Operand b (fun predicate -> Atom("app", [ predicate; v; tTrue ])) ])
            | Apply(_, callee, argument) ->
                (match callee with
                 | Identifier(cs, name) when builtinSets.Contains name && this.Name(cs, name) = Some(Fn(name, [])) ->
                     conj [ this.Value argument v; Atom(":", [ v; Fn(name, []) ]) ]
                 | _ -> this.Operands [ callee; argument ] (fun ts -> Atom("app", [ ts[0]; ts[1]; v ])))
            | _ when this.IsProposition e ->
                disj [ conj [ this.Goal e; eq v tTrue ]; conj [ this.Refute e; eq v tFalse ] ]
            | _ -> unsupported e.Span (this.Describe e)

    /// The proposition e holds.
    member this.Goal(e: Expr) : Formula =
        match e with
        | Group(_, inner) -> this.Goal inner
        | Bool(_, b) -> if b then True else False
        | Prefix(_, op, a) when op.Text = "!" -> this.Refute a
        | Prefix(_, op, a) when op.Text = "let" -> exists (declaredIn "let" op.Span.Start) (this.Goal a)
        | Prefix(_, op, a) when claims.Contains op.Text -> this.Goal a
        | Infix(_, op, a, b) when relations.Contains op.Text ->
            this.Operands [ a; b ] (fun ts -> relation op.Text ts[0] ts[1])
        | Infix(_, op, a, b) ->
            (match op.Text with
             | "&"
             | "&&"
             | "&&&" -> conj [ this.Goal a; this.Goal b ]
             | "|" -> disj [ conj [ this.Goal a; this.Defined b ]; conj [ this.Defined a; this.Goal b ] ]
             | "||" -> disj [ this.Goal a; conj [ this.Refute a; this.Goal b ] ]
             | "|||" -> disj [ this.Goal a; this.Goal b ]
             | "all" -> forall (declaredIn "all" op.Span.Start) (implies (this.Goal a) (this.Goal b))
             | "in" -> exists (declaredIn "in" op.Span.Start) (conj [ this.Goal a; this.Goal b ])
             | _ -> this.Value e tTrue)
        | _ -> this.Value e tTrue

    /// The proposition e is false.
    member this.Refute(e: Expr) : Formula =
        match e with
        | Group(_, inner) -> this.Refute inner
        | Bool(_, b) -> if b then False else True
        | Prefix(_, op, a) when op.Text = "!" -> this.Goal a
        | Prefix(_, op, a) when op.Text = "let" -> exists (declaredIn "let" op.Span.Start) (this.Refute a)
        | Prefix(_, op, a) when claims.Contains op.Text -> this.Refute a
        // Failed unification is undefined, not false.
        | Infix(_, op, _, _) when op.Text = "?=" -> False
        | Infix(_, op, a, b) when relations.Contains op.Text ->
            this.Operands [ a; b ] (fun ts -> neg (relation op.Text ts[0] ts[1]))
        | Infix(_, op, a, b) ->
            (match op.Text with
             | "&" -> disj [ conj [ this.Refute a; this.Defined b ]; conj [ this.Defined a; this.Refute b ] ]
             | "&&" -> disj [ this.Refute a; conj [ this.Goal a; this.Refute b ] ]
             | "&&&" -> disj [ this.Refute a; this.Refute b ]
             | "|"
             | "||"
             | "|||" -> conj [ this.Refute a; this.Refute b ]
             // A requirement that is undefined for some case counts as false.
             | "all" -> exists (declaredIn "all" op.Span.Start) (conj [ this.Goal a; neg (this.Goal b) ])
             | "in" -> exists (declaredIn "in" op.Span.Start) (conj [ this.Goal a; this.Refute b ])
             | _ -> this.Value e tFalse)
        | _ -> this.Value e tFalse

    /// e yields at least one value.
    member this.Defined(e: Expr) : Formula =
        match e with
        | Group(_, inner) -> this.Defined inner
        | Bool _ -> True
        | Prefix(_, op, a) when op.Text = "!" -> this.Defined a
        // A strict relation or connective is defined exactly where its operands are.
        | Infix(_, op, a, b) when relations.Contains op.Text && op.Text <> "?=" -> this.Operands [ a; b ] (fun _ -> True)
        | Infix(_, op, a, b) when op.Text = "&" || op.Text = "|" -> conj [ this.Defined a; this.Defined b ]
        | _ when this.IsProposition e -> disj [ this.Goal e; this.Refute e ]
        | _ ->
            match this.Functional e with
            | Some(cs, _) -> conj cs
            | None ->
                let x = fresh ()
                someFresh x (this.Value e (Var x))

    member _.Describe(e: Expr) =
        match e with
        | Infix(_, op, _, _) -> $"operator '{op.Text}'"
        | Prefix(_, op, _) -> $"operator '{op.Text}'"
        | Section(_, op, _) -> $"section of '{op.Text}'"
        | Set _ -> "set definition"
        | Multiset _ -> "multiset definition"
        | List _ -> "list"
        | Product _ -> "product"
        | String _ -> "interpolated string"
        | InterpolatedIdentifier _ -> "interpolated identifier"
        | _ -> "expression"

/// Translates a bound program into the formula relating it to its value.
let translate (expr: Expr) (bound: Bound) : Translation =
    let t = Translator(bound)
    let value = t.Value expr (Var valueName)
    let binding = bound.Diagnostics |> List.map (fun d -> { Message = d.Message; Span = d.Span })
    { Value = value
      Axioms = t.Axioms
      Literals = t.Literals
      Diagnostics = binding @ t.Diagnostics |> List.sortBy (fun d -> d.Span.Start.Offset) }

let private renderAxiom (a: Axiom) =
    let quantifier = a.Vars |> List.map (fun x -> x + " : " + universe) |> String.concat " & "
    if a.Exact then
        let statement = toString a.Head + " == (" + toString a.Body + ")"
        if a.Vars.IsEmpty then statement else quantifier + " all " + statement
    else
        let condition = "(" + toString a.Head + ")"
        (if a.Vars.IsEmpty then condition else quantifier + " & " + condition) + " all " + toString a.Body

let render (translation: Translation) =
    let lines =
        [ yield "Value:"
          yield "  " + toString translation.Value
          if not translation.Axioms.IsEmpty then
              yield "Axioms:"
              for a in translation.Axioms do
                  yield "  " + renderAxiom a
          if not translation.Diagnostics.IsEmpty then
              yield "Errors:"
              for d in translation.Diagnostics do
                  yield $"  {Span.render d.Span}: {d.Message}" ]
    String.concat "\n" lines
