## Functions


When applied, a function establish a relation between the actual argument and the result

### The `Func` set

`Func` is the set of all functions.


### Sets as functions

There exists an implicit conversion from every set to its identity function. This means that is a set is used where a function is expected then it is converted to an identity function which is defined only for the members of the set.

    Sets..Identity = set => set x => x

This means that every set has an `Identity` property which is a 


### The `fn` operator

Functions can be created from sets of ordered pairs through the `fn` operator.

    let fib ?= fn{ 0->0, 1->1, int n?>1 -> fib(n-2) + fib(n-1) }

    