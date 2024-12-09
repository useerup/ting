## Scratchpad


The first place I will encounter this is in the compiler itself. It turns out that the language is really good for expressing grammar and associated semantics, so I am writing as much as possible in the language itself. The compiler will only compile a "core" language.

A *rule* function is a function which accepts the "result" of the parser (remember that functions are merely relations so they can be used both ways). This is the rule for parsing a set constructor:

    SetConstructor = {
        `{` >> ExpressionList exps >> `}`  <-  Core.Set.FromExps exps
    }

    ExpressionList = {
        Expression exp >> `,` >> ExpressionList exps <- (exp,,exps)
        Expression exp >> Not `,` <- exp
    }

## Hello World

    Console.Out $ +: "Hello World



    let l:int mandated lo
    let h:int mandated hi 

    Partition = (Arrays ar, int l, int h) \
        let 
            a:Arrays mandated ar 
        in
            while (l < h) do
                l := + 1 while (l < h && a l <= p)
                h := - 1 while (h > l && a h >= p)
                (l < h) then do
                    a $ SwapAt(l,h)
                    l $= + 1
                    h $= - 1 in a
                else
                
            


## Quicksort


    Arrays..Qsort = ar => { 
        ar.Range [i...<i) -> ar
        ar.Range r -> Partition( ar, ar (r.Middle), r.LowerBound, r.UpperBound ) r |> (m,lte,gte) \ m.Replace lte (q m lte).Replace gte (q m gte)
    }

    Partition = { (Array ar, p, lo, hi) -> 
        let
            l = lo |> (+ 1) while (x => x < hi && ar x <= p)
            h = hi |> (- 1) while (x => x > l && ar x >= p)
        in (l < h) 
            then This( ar.SwapAt(l,h), p, l + 1, h - 1)
            else ar.ReplaceAt(hi,ar l).ReplaceAt(l,p)
    }




    let (a=42 in b=a)

    let (int.x=42) in 