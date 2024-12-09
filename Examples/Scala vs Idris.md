

See also https://www.youtube.com/watch?v=fV2no1Rkzdw&list=PLcGKfGEEONaD7ncbh5S1qSlhVOxjGeITz&index=19



    Nat = type { Symbol Z, Symbol S -> Nat _ }

    List = Types T \ type { Symbol Nil, T*(List T) _ }

    Vector = (Types T) \ type ~{ Z -> Nil, S k -> This k }

    Vector..Cons = {
        Nil -> Vector Z T _
    }

    Vector..PairwiseAdd = 