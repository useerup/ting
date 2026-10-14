/// The translated formula is what the prover reasons about: these check facts about program values.
module ProverTests

open Xunit
open Ting.Syntax
open Ting.Binding
open Ting.Core
open Ting.Logic
open Ting.Logic.Formula

let private value = Var Translate.valueName
let private num (n: int) = Fn(string n, [])

/// Every value of the program satisfies the property, given the axioms of the translation.
let private holds (code: string) (property: Formula) =
    match Parser.parse code with
    | Ok expr ->
        let t = Translate.translate expr (Binder.bind expr)
        Assert.Empty t.Diagnostics
        let claim = Forall(Translate.valueName, implies t.Value property)
        Resolution.proveFrom 2000 t.Assumptions claim
    | Error e -> failwith e.Message

[<Fact>]
let ``local binding computes its body`` () =
    Assert.Equal(Resolution.Proved, holds "x = 1 in x + 1" (Atom("=", [ value; Fn("+", [ num 1; num 1 ]) ])))

[<Fact>]
let ``alternative on undefined skips an undefined division`` () =
    Assert.Equal(Resolution.Proved, holds "1 / 0 ;; 7" (Atom("=", [ value; num 7 ])))

[<Fact>]
let ``guarded disjunction ignores an undefined right operand`` () =
    Assert.Equal(Resolution.Proved, holds "x = 3 in x > 0 || x / 0 > 1" (Atom("=", [ value; Fn("true", []) ])))

[<Fact>]
let ``a choice has more than one value`` () =
    Assert.Equal(Resolution.NotValid, holds "0 ; 1" (Atom("=", [ value; num 0 ])))

[<Fact>]
let ``applying a lambda evaluates its body`` () =
    Assert.Equal(Resolution.Proved, holds "f = (x => x + 1) in f 2" (Atom("=", [ value; Fn("+", [ num 2; num 1 ]) ])))

[<Fact>]
let ``a set definition has its elements as members`` () =
    Assert.Equal(Resolution.Proved, holds "S = { 1, 2 } in 2 : S" (Atom("=", [ value; Fn("true", []) ])))

[<Fact>]
let ``the first contribution at an origin wins`` () =
    Assert.Equal(Resolution.Proved, holds "f = fn { 0 -> 1, 0 -> 2 } in f 0" (Atom("=", [ value; num 1 ])))

[<Fact>]
let ``a later contribution at the same origin is ignored`` () =
    Assert.Equal(Resolution.Proved, holds "f = fn { 0 -> 1, 0 -> 2 } in f 0" (Not(Atom("=", [ value; num 2 ]))))
