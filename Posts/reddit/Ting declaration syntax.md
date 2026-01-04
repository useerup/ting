# Ting declaration syntax

    rec{ Number:int, Name:string, Amount:float }

    { int x \ x>10 }

    x => x * 2

    int x => x * 2

    enum { Hearts, Clubs, Spades, Diamonds }

    (a,b) => a+b

    rec(Name:string, Age:int) => $"{Name} is {Age} years old"

This is structural typing. 

    { (0,1) |> f ~ (a,b) => a;f(b,a+b) }

In this expression 
- `f` is declared locally for `f` and referenced in `(a,b) => a;f(b,a+b)`
- `a` and `b` are declared in `(a,b)` and referenced in `a;f(b,a+b)` 