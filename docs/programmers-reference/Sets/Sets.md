## Sets

A Ting *set* is a collection of unique values. As such, Ting sets are closely modeled after the sets you know from mathematics. 

A set may be defined by listing its elements (extensional set definition) and/or by describing some rule or expression which defines which element are members of the set.



### Materialized set

A *materialized set* is one where all the members are known and identified.



### Set operators

Several operators accept a set as one or both operands:

- `:` (*member-of*) returns a `bool` indicating whether the left operand is a member of the right-hand set.
- `::` (*collection-of*) returns a `bool` indicating whether the left operand is a collection whose elements are all members of the right-hand set.
- `:::` (*sequence-of*) returns a `bool` indicating whether the left operand is an ordered sequence of members drawn from the right-hand set.
- `<` and `<=` return `true` when the left operand is a proper subset or subset, respectively, of the right-hand set.
- `>` and `>=` return `true` when the left operand is a proper superset or superset, respectively, of the right-hand set.
- `==` returns true when two sets contains the same elements.



### Set definitions

One way to create a set is through a *set definition*. A set definition consists of a comma-separated list of expressions surrounded by `{` and `}`—mirroring the mathematical notation for sets.

Examples:

    { }                                 // Empty

    { 1, 2, 3 }                         // Extensional

    { x \ x:int && x >= 0 }             // Intensional

    { 1, "abc", Math.Pi, {39, 42} }     // Heterogenous set of an integer, string, double and a set

Each expression in the list may itself be [nondeterministic](Nondeterminism.md). The set definition expands ("unwinds") those possibilities to include every resulting value, which is why the construct works equally well for both *extensional* and *intensional* style definitions familiar from mathematics.

    

    



    { x \ IsOrbitingSun(x) }

    IsOrbitingSun = Mercury;Venus;Earth;Mars;Jupiter;Saturn;Uranus;Neptune --> true



### Set-valued expressions

Some expressions evaluate to sets even though they are not written with braces.

Subsets

    int.Where(>=0)         // The set of 32-bit integers greater than or equal to zero

Product sets

    int*string




### Ordered sets

A set definition returns an ordered set by default. The default order is the order of definition. The set `{ 1, 4, 2, 8, 5, 7 }` is thus ordered **not** numerically but rather in that sequence.

An ordered set has the following extension methods:

- `Following`
- `FollowingCyclic`
- `Previous`
- `PreviousCyclic`

Example: ordered set from a Collatz sequence

    IsEven = x => x%2==0

    CollatzSet = int start?>0 => { 
        let f = fn { 
            1 -> 1,
            let n ? IsEven -> n ; f(n/2)
            let n -> n ; f(3*n+1) 
        } in f start
    }

For example, `CollatzSet 6` evaluates to the ordered set `{ 6, 3, 10, 5, 16, 8, 4, 2, 1 }`.


### Enumerable sets

An enumerable set is a set whose members can be enumerated.


### Infinite sets




### Multisets

Regular sets may only contain *unique* elements. *Multisets* on the other hand can contain the same element multiple times.

Multisets are defined using the `{*` ... `*}` set definition

    {* 0, 1, 1, 2, 3 *}     // multiset which contains 5 elements

### Ordered multisets

Multisets can also be ordered. If they are generated from *choices* they contain elements in the order they are generated.

Examples of ordered multisets:

    {* let fib = (int a, int b) => a ; fib(b,a+b) in fib(0,1) *}

This example defines an ordered multiset of the Fibonacci numbers. The set is infinite (in principle), but will throw an overflow exception is enumerated past the `int` range.

