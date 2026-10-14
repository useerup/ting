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

/// Every value of the program satisfies the property.
let private holds (code: string) (property: Formula) =
    match Parser.parse code with
    | Ok expr ->
        let t = Translate.translate expr (Binder.bind expr)
        Assert.Empty t.Diagnostics
        Resolution.prove 2000 (Forall(Translate.valueName, implies t.Value property))
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
