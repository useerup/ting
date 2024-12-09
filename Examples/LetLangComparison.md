See https://www.reddit.com/r/ProgrammingLanguages/comments/143a6qa/letlang_roadblocks_and_how_to_overcome_them/


    // Integers divisible by 2
    IntsDivisibleBy2 = int i => i%2==0

    // Integers divisible by 3
    IntsDivisibleBy3 = int i => i%3==0 

    // Integers divisible by 4
    IntsDivisibleBy4 = int i => i%4==0 

    IntsDivisbleBy2 < IntsDivisibleBy4 // true

    IntsDivisibleBy2 < IntsDivisibleBy3 // false
   

    // Function which divides by 4
    // accepts only integers divisible by 4
    DivideBy4 = DivisibleBy4 n -> n / 4

    // Higher order function accepts a function `f` which accepts 
    // integers divisible by 2 and returns `f` applied to `2`
    ApplyTo2 = (DivisibleBy2=>int) f -> f 2

Now consider

    DivisibleBy4ApplyTo2 = ApplyTo2 DivisibleBy4

Here we apply Apply2 to DivisibleBy4. Apply2 expects a function `DivisibleBy2=>int` (i.e. )

The intersection of DivisibleBy4 and DivisibleBy2 is just DivisibleBy4, as DivisibleBy4 is a subset of DivisibleBy2. To be precise, the *identity* of a is (like in math) the *members* of the set. Two sets are equal (are the same sets) if and only if they contain the exact same members. 

In this case `mod2=>int` is unified with `div4`. The latter is really `mod4 n -> n / 4`. The former can be rewritten to `mod2 n -> int _`. The right sides of `->` can be unified into `n / 4`. The left sides `mod2 n` and `mod4 n` can be unify `{ int i \ i%2==0} n` and `{int i \ i%3==0} n` into `{ int i \ i%2==0 & i%4==0 }`. By arithmetic reduction this reduces to `{ int i \ i%4==0 }`

So `( {int i\i%4==0} n -> n / 4) 2` will fail because `2` does not satisfy the expression `{2%4==0}`.



    f3 = operation mod3
    