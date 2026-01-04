
### Fibonacci

    // Function `Fibonacci` calculates the n'th Fibonacci number
    Fibonacci = var f # func{ 0 -> 0, 1 -> 1, int n?>1 -> f(n-2)+f(n-1) }

Example:

    Fibonacci 6     // returns 5


### Quicksort

    Quicksort = func {---}
        [] -> []
        [x] + xs -> Quicksort(xs??<=x) + [x] + Quicksort(xs??>x)



Example:

    Quicksort [2,3,1]       // returns [1,2,3]


### Factorial

    Factorial = func { 
        0;1 -> 1
        int n?>1 -> n*Factorial(n-1) 
    }



### Product

    Product = func { [n] -> n, [n]+list -> n * Product list }

    Product = Lists..(FoldLeft(*,1))


### Folds

    foldl = (f,z) => 
        ( [] => z) ||
        ( [x]+xs => var zz=f(z,x) in foldl )



    Lists..FoldLeft = list => (f,z) => list |> func{ 
        [] -> z 
        [x]+xs -> xs.FoldLeft( f, f(z,x) )
    }

    Lists..FoldRight = list => (f,z) => list |> func{ 
        [] -> z 
        [x]+xs -> f( x, xs.FoldLeft(f,z) )
    }

    