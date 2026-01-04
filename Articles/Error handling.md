
Partial functions

A function which is not guaranteed to be defined for all values its domain is said to be *partial*. This is for instance a function (lets call it `OpenRead`) which accepts a file path and opens the file at that path for reading:

    OpenRead = string path => ...

OpenRead is only defined for values of 


The boolean `bool*bool-->bool` conditional-or operator `||` absorbs errors. 

If the LHS evaluates to `true`, operator either does not evaluate the RHS *or* it silently catches any `UndefinedException`s thrown from the evaluation of RHS. Thus it act as a guard. 

This resembles the semantics of conditional-or in many other languages.

Assume `x:(int|Null)` in the following

    !x.HasValue || y > x

