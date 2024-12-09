## Basic examples

    // Set of integers (contrived; it is equivalent to just `int`)
    { int _ }

    // Set of positive integers
    { int _ ? > 0 }
    int ?? > 0

    // Set of strings longer than 10 characters
    { string _ ?.(Length>10) }
    string ?? .Length >> >10
    string ?? ./(Length>10)
    string ?? (s\s.Length>10)

    // Set of even integers
    { int _ ? %2 >> =0 }
    { int _ ? (x\x%2=0) }
    int ?? %2 >> =0
    int ?? (x\x%2=0)

    // Function of Fibonacci numbers
    Fibonacci = { 0->1, 1->1, n?>1->This(n-1)+This(n-2) }


## Roots

    Root = { 
        (float a, float b, float c)
    -> 
        b^2-4*a*c ; { 
            0       ->  ( -b) / 2 / a
            d?>0    ->  _ ? ( -b+Sqrt(d) ) / 2 / a  ~|~  ( -b-Sqrt(d) ) / 2 / a
            _       ->  void
        }
    }


## Factorial

    Factorial = { 0 -> 1, n?>0 -> n * This(n-1) }


## Compose

    Compose = (Sets f, Sets g) \ x \ f (g x)


## Fizzbuzz

    FizzBuzz = x?>0 \ (x % 3, x % 5) |> 
    {
        (0,0) -> "FizzBuzz"
        (0,_) -> "Fizz"
        (_,0) -> "Buzz"
        _ -> x.ToString
    }

    
## Find power

    // Find the
    LeastPowerOfTwo = 
        let lp = {
            (i,p) \\ p<i -> This(i,p*2)
            (_,p) -> p
            
        }
        in i ? >0 \ lp (i,1)

    LeastPowerOfTwo = ulong i \ 1 ; *2 while <i  

    (while) = ex T in Funcs.Of T a \ Preds.Of T c \ { T x -> c x then a x >> This else x } 

## Even odd

    Odd = { x -> Even(x-1) }
    Even = { 0 -> true, x -> Odd(x-1) }



## Players, weapons, stats (https://www.youtube.com/watch?v=9fhnJpCgtUw)


    Players = { [Strings Name] } && {
        Name = "Jimmer", Class = Warrior
        Name = "Sir Will", Class = Knight
        Name = "Dalgrith", Class = Rogue
    }
    Weapons = 
    {[
        Name = LongSword, AllowedClassed = ~{Warrior,Knight}, StandardUpgrade=Power
    ]}

    Players * Weapons * Stats 
        ?? (p,w,s)\p.Class:w.AllowedClasses & ... 
        @@ (p,w,s)\(. Name=p.Name, Weapon=w.Name  .)

        


## Polynomium

    { |- a*x^2 + b*x + c = 0d   ->  b^2-4*a*c ; d \ d switch { <0 -> undefined, =0 -> -b/2/a, >0 -> @:{b+} }



{ Points p with (. Strings name .) ?.(   )  }