module FormulaTests

open Xunit
open Ting.Logic
open Ting.Logic.Formula
open Dsl
open type Dsl.Symbols

[<Fact>]
let ``printing`` () =
    Assert.Equal("x : any & y : any all P x & Q y", toString (forall [ x; y ] (P(x) * Q(y))))
    Assert.Equal("!p | q", toString (p ^=> q))
    Assert.Equal("(p | q) & r", toString ((p + q) * r))
    Assert.Equal("(x : any & P x) & !(y : any & Q y)", toString (exists [ x ] (P(x)) * ~~(exists [ y ] (Q(y)))))
    Assert.Equal("p | (x : any all P(f(x, g x), a) | (x = a))", toString (p + forall [ x ] (P(f(x, g(x)), a) + Atom("=", [ x; a ]))))

[<Fact>]
let ``substitution avoids capture`` () =
    let result = subst (Map.ofList [ "y", x ]) (forall [ x ] (P(x, y)))
    Assert.Equal("x1 : any all P(x1, x)", toString result)

[<Fact>]
let ``unification solves and rejects cycles`` () =
    match Unify.fullUnify [ f(x, g(y)), f(g(z), w) ] with
    | Some env ->
        Assert.Equal("g z", termToString env["x"])
        Assert.Equal("g y", termToString env["w"])
    | None -> failwith "expected a unifier"
    Assert.True((Unify.fullUnify [ x, g(x) ]).IsNone)

[<Fact>]
let ``skolemization replaces existentials`` () =
    Assert.Equal("P(x, f_y x)", toString (Normal.skolemize (forall [ x ] (exists [ y ] (P(x, y))))))

[<Fact>]
let ``clause form`` () =
    let clauses = Normal.simpcnf ((p + q) * (p + ~~q) * (p + q))
    Assert.Equal(2, clauses.Length)

let private invalid =
    Map
        [ "disjunction", p + q
          "existential", exists [ x ] (P(x))
          "forall over or", forall [ x ] (P(x) + Q(x)) ^=> forall [ x ] (P(x)) + forall [ x ] (Q(x)) ]

let invalidNames: obj[] list = invalid |> Map.keys |> Seq.map (fun n -> [| box n |]) |> List.ofSeq

[<Theory>]
[<MemberData(nameof invalidNames)>]
let ``invalid formulas are not proved`` (name: string) =
    Assert.Equal(Resolution.NotValid, Resolution.prove 1000 invalid[name])
