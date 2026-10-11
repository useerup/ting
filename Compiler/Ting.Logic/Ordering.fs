/// The Knuth-Bendix ordering on terms, with every symbol weighing 1 and precedence by arity, then name.
module Ting.Logic.Ordering

open Ting.Logic.Formula

let rec private size (t: Term) =
    match t with
    | Var _ -> 1
    | Fn(_, args) -> 1 + List.sumBy size args

let rec private occurrences (t: Term) (acc: Map<string, int>) =
    match t with
    | Var x -> acc |> Map.add x ((Map.tryFind x acc |> Option.defaultValue 0) + 1)
    | Fn(_, args) -> List.foldBack occurrences args acc

/// s > t: a well-founded ordering stable under substitution, so rewriting with s -> t terminates.
let rec greater (s: Term) (t: Term) : bool =
    let inS = occurrences s Map.empty
    let enoughVariables = occurrences t Map.empty |> Map.forall (fun x n -> (Map.tryFind x inS |> Option.defaultValue 0) >= n)
    enoughVariables
    && (let ws, wt = size s, size t
        ws > wt
        || ws = wt
           && match s, t with
              | Fn(f, ss), Fn(g, ts) ->
                  let c = compare (ss.Length, f) (ts.Length, g)
                  c > 0 || c = 0 && lexGreater ss ts
              | _ -> false)

and private lexGreater (ss: Term list) (ts: Term list) =
    match ss, ts with
    | s :: srest, t :: trest -> if s = t then lexGreater srest trest else greater s t
    | _ -> false

let private literalTerm (lit: Formula) =
    match lit with
    | Atom(p, args)
    | Not(Atom(p, args)) -> Fn(p, args)
    | _ -> Fn("", [])

/// No other literal of the clause is greater; then no instance of another literal can be greater either.
let isMaximal (cl: Ting.Logic.Normal.Clause) (lit: Formula) =
    let t = literalTerm lit
    cl |> Set.forall (fun other -> other = lit || not (greater (literalTerm other) t))
