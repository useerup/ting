## Functions


When applied, a function establish a relation between the actual argument and the result

### The `Func` set

`Func` is the set of all functions.


### Sets as functions

There exists an implicit conversion from every set to its identity function. This means that if a set is used where a function is expected then it is converted to an identity function which is defined only for the members of the set.

    Sets..Identity = set => set x => x

This means that every set has an `Identity` property.
 

### The `fn` operator

Functions can be created from collections of mappings through the `fn` operator. A mapping `x -> y` is an ordinary product fusing the argument with a named `Result` field. When `fn` is written with case patterns, the cases lower to an ordered choice of lambdas — `(p1 => r1) || (p2 => r2) || …` — so earlier cases take priority.

    let fib ?= fn{ 0->0, 1->1, int n?>1 -> fib(n-2) + fib(n-1) }

    