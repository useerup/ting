/// Problems with equality, proved by resolution with paramodulation. Most are Pelletier's problems 48-61.
module EqualityTests

open Xunit
open Ting.Logic
open Dsl
open type Dsl.Symbols

let private valid =
    Map
        [ "p48", ((a === b) + (c === d)) * ((a === c) + (b === d)) ^=> (a === d) + (b === c)
          "p49", exists [ x; y ] (forall [ z ] ((z === x) + (z === y))) * P(a) * P(b) * ~~(a === b) ^=> forall [ x ] (P(x))
          "p51",
          exists [ z; w ] (forall [ x; y ] (P(x, y) <=> (x === z) * (y === w)))
          ^=> exists [ z ] (forall [ x ] (exists [ w ] (forall [ y ] (P(x, y) <=> (y === w)) <=> (x === z))))
          "p52",
          exists [ z; w ] (forall [ x; y ] (P(x, y) <=> (x === z) * (y === w)))
          ^=> exists [ w ] (forall [ y ] (exists [ z ] (forall [ x ] (P(x, y) <=> (x === z)) <=> (y === w))))
          "p56", forall [ x ] (exists [ y ] (P(y) * (x === f(y))) ^=> P(x)) <=> forall [ x ] (P(x) ^=> P(f(x)))
          "p61",
          forall [ x; y; z ] (f(x, f(y, z)) === f(f(x, y), z))
          ^=> forall [ x; y; z; w ] (f(x, f(y, f(z, w))) === f(f(f(x, y), z), w))
          "symmetry", (a === b) ^=> (b === a)
          "congruence", (a === b) * P(f(a)) ^=> P(f(b))
          "group right inverse",
          forall [ x; y; z ] (m(x, m(y, z)) === m(m(x, y), z))
          * forall [ x ] (m(e, x) === x)
          * forall [ x ] (m(i(x), x) === e)
          ^=> forall [ x ] (m(x, i(x)) === e) ]

let validNames: obj[] list = valid |> Map.keys |> Seq.map (fun n -> [| box n |]) |> List.ofSeq

[<Theory>]
[<MemberData(nameof validNames)>]
let ``equality problem is proved`` (name: string) =
    Assert.Equal(Resolution.Proved, Resolution.prove 5000 valid[name])

[<Fact>]
let ``an unrelated equation is not proved`` () =
    Assert.Equal(Resolution.NotValid, Resolution.prove 1000 ((a === b) ^=> (f(a) === c)))
