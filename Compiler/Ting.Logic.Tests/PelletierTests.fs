/// Pelletier's problems 1-46 without equality (p38 omitted), from "Seventy-Five Problems for Testing Automatic Theorem Provers" (1986).
module PelletierTests

open Xunit
open Ting.Logic
open Dsl
open type Dsl.Symbols

let private problems =
    Map
        [ "p1", p ^=> q <=> ~~q ^=> ~~p
          "p2", ~~(~~p) <=> p
          "p3", ~~(p ^=> q) ^=> q ^=> p
          "p4", ~~p ^=> q <=> ~~q ^=> p
          "p5", (p + q ^=> p + r) ^=> p + (q ^=> r)
          "p6", p + ~~p
          "p7", p + ~~(~~(~~p))
          "p8", ((p ^=> q) ^=> p) ^=> p
          "p9", (p + q) * (~~p + q) * (p + ~~q) ^=> ~~(~~q + ~~q)
          "p10", (q ^=> r) * (r ^=> p * q) * (p ^=> q * r) ^=> (p <=> q)
          "p11", p <=> p
          "p12", ((p <=> q) <=> r) <=> (p <=> (q <=> r))
          "p13", p + q * r <=> (p + q) * (p + r)
          "p14", (p <=> q) <=> (q + ~~p) * (~~q + p)
          "p15", p ^=> q <=> ~~p + q
          "p16", (p ^=> q) + (q ^=> p)
          "p17", p * (q ^=> r) ^=> s <=> (~~p + q + s) * (~~p + ~~r + s)
          "p18", exists [ y ] (forall [ x ] (P(y) ^=> P(x)))
          "p19", exists [ x ] (forall [ y; z ] ((P(y) ^=> Q(z)) ^=> P(x) ^=> Q(x)))
          "p20",
          forall [ x; y ] (exists [ z ] (forall [ w ] (P(x) * Q(y) ^=> R(z) * U(w))))
          ^=> exists [ x; y ] (P(x) * Q(y))
          ^=> exists [ z ] (R(z))
          "p21", exists [ x ] (p ^=> F(x)) * exists [ x ] (F(x) ^=> p) ^=> exists [ x ] (p <=> F(x))
          "p22", forall [ x ] (p <=> F(x)) ^=> (p <=> forall [ x ] (F(x)))
          "p23", forall [ x ] (p + F(x)) <=> p + forall [ x ] (F(x))
          "p24",
          ~~(exists [ x ] (U(x) * Q(x)))
          * forall [ x ] (P(x) ^=> Q(x) + R(x))
          * ~~(exists [ x ] (P(x) ^=> exists [ x ] (Q(x))))
          * forall [ x ] (Q(x) * R(x) ^=> U(x))
          ^=> exists [ x ] (P(x) * R(x))
          "p25",
          exists [ x ] (P(x))
          * forall [ x ] (U(x) ^=> ~~G(x) * R(x))
          * forall [ x ] (P(x) ^=> G(x) * U(x))
          * (forall [ x ] (P(x) ^=> Q(x)) + exists [ x ] (Q(x) * P(x)))
          ^=> exists [ x ] (Q(x) * P(x))
          "p26",
          (exists [ x ] (P(x)) <=> exists [ x ] (Q(x)))
          * forall [ x; y ] (P(x) * Q(y) ^=> (R(x) <=> U(y)))
          ^=> (forall [ x ] (P(x) ^=> R(x)) <=> forall [ x ] (Q(x) ^=> U(x)))
          "p27",
          exists [ x ] (P(x) * ~~Q(x))
          * forall [ x ] (P(x) ^=> R(x))
          * forall [ x ] (U(x) * V(x) ^=> P(x))
          * exists [ x ] (R(x) * ~~Q(x))
          ^=> forall [ x ] (U(x) ^=> ~~R(x))
          ^=> forall [ x ] (U(x) ^=> ~~V(x))
          "p28",
          forall [ x ] (P(x) ^=> forall [ x ] (Q(x)))
          * (forall [ x ] (Q(x) + R(x)) ^=> exists [ x ] (Q(x) * R(x)))
          * (exists [ x ] (R(x)) ^=> forall [ x ] (L(x) ^=> M(x)))
          ^=> forall [ x ] (P(x) * L(x) ^=> M(x))
          "p29",
          exists [ x ] (P(x)) * exists [ x ] (G(x))
          ^=> (forall [ x ] (P(x) ^=> H(x)) * forall [ x ] (G(x) ^=> J(x))
               <=> forall [ x; y ] (P(x) * G(y) ^=> H(x) * J(y)))
          "p30",
          forall [ x ] (P(x) + G(x) ^=> ~~H(x)) * forall [ x ] ((G(x) ^=> ~~U(x)) ^=> P(x) * H(x))
          ^=> forall [ x ] (U(x))
          "p31",
          ~~(exists [ x ] (P(x) * (G(x) + H(x))))
          * exists [ x ] (Q(x) * P(x))
          * forall [ x ] (~~H(x) ^=> J(x))
          ^=> exists [ x ] (Q(x) * J(x))
          "p32",
          forall [ x ] (P(x) * (G(x) + H(x)) ^=> Q(x))
          * forall [ x ] (Q(x) * H(x) ^=> J(x))
          * forall [ x ] (R(x) ^=> H(x))
          ^=> forall [ x ] (P(x) * R(x) ^=> J(x))
          "p33",
          forall [ x ] (P(a) * (P(x) ^=> P(b)) ^=> P(c))
          <=> forall [ x ] (P(a) ^=> P(x) + P(c)) * (P(a) ^=> P(b) ^=> P(c))
          "p34",
          (exists [ x ] (forall [ y ] (P(x) <=> P(y))) <=> (exists [ x ] (Q(x)) <=> forall [ y ] (Q(y))))
          <=> (exists [ x ] (forall [ y ] (Q(x) <=> Q(y))) <=> (exists [ x ] (P(x)) <=> forall [ y ] (P(y))))
          "p35", exists [ x; y ] (P(x, y) ^=> forall [ x; y ] (P(x, y)))
          "p36",
          forall [ x ] (exists [ y ] (P(x, y)))
          * forall [ x ] (exists [ y ] (G(x, y)))
          * forall [ x; y ] (P(x, y) + G(x, y) ^=> forall [ z ] (P(y, z) + G(y, z) ^=> H(x, z)))
          ^=> forall [ x ] (exists [ y ] (H(x, y)))
          "p37",
          forall [ z ] (
              exists [ w ] (
                  forall [ x ] (exists [ y ] ((P(x, z) ^=> P(y, w)) * P(y, z) * (P(y, w) ^=> exists [ u ] (Q(u, w)))))
              )
          )
          * forall [ x; z ] (~~P(x, z) ^=> exists [ y ] (Q(y, z)))
          * (exists [ x; y ] (Q(x, y)) ^=> forall [ x ] (R(x, x)))
          ^=> forall [ x ] (exists [ y ] (R(x, y)))
          "p39", ~~(exists [ x ] (forall [ y ] (P(y, x) <=> ~~P(y, y))))
          "p40",
          exists [ y ] (forall [ x ] (P(x, y) <=> P(x, x)))
          ^=> ~~(forall [ x ] (exists [ y ] (forall [ z ] (P(z, y) <=> ~~P(z, x)))))
          "p41",
          forall [ z ] (exists [ y ] (forall [ x ] (P(x, y) <=> P(x, z) * ~~P(x, x))))
          ^=> ~~(exists [ z ] (forall [ x ] (P(x, z))))
          "p42", ~~(exists [ y ] (forall [ x ] (P(x, y) <=> ~~(exists [ z ] (P(x, z) * P(z, x))))))
          "p43",
          forall [ x; y ] (Q(x, y) <=> forall [ z ] (P(z, x) <=> P(z, y)))
          ^=> forall [ x; y ] (Q(x, y) <=> Q(y, x))
          "p44",
          forall [ x ] (P(x) ^=> exists [ y ] (G(y) * H(x, y)) * exists [ y ] (G(y) * ~~H(x, y)))
          * exists [ x ] (J(x) * forall [ y ] (G(y) ^=> H(x, y)))
          ^=> exists [ x ] (J(x) * ~~P(x))
          "p45",
          forall [ x ] (P(x) * forall [ y ] (G(y) * H(x, y) ^=> J(x, y)) ^=> forall [ y ] (G(y) * H(x, y) ^=> R(y)))
          * ~~(exists [ y ] (L(y) * R(y)))
          * exists [ x ] (P(x) * forall [ y ] (H(x, y) ^=> L(y)) * forall [ y ] (G(y) * H(x, y) ^=> J(x, y)))
          ^=> exists [ x ] (P(x) * ~~(exists [ y ] (G(y) * H(x, y))))
          "p46",
          forall [ x ] (P(x) * forall [ y ] (P(y) * H(y, x) ^=> G(y)) ^=> G(x))
          * (exists [ x ] (P(x) * ~~G(x))
             ^=> exists [ x ] (P(x) * ~~G(x) * forall [ y ] (P(y) * ~~G(y) ^=> J(x, y))))
          * forall [ x; y ] (P(x) * P(y) * H(x, y) ^=> ~~J(y, x))
          ^=> forall [ x ] (P(x) ^=> G(x)) ]

let problemNames: obj[] list = problems |> Map.keys |> Seq.map (fun n -> [| box n |]) |> List.ofSeq

[<Theory>]
[<MemberData(nameof problemNames)>]
let ``Pelletier problem is proved`` (name: string) =
    Assert.Equal(Resolution.Proved, Resolution.prove 5000 problems[name])
