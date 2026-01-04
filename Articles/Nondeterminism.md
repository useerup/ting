
The `;` if the *choice* operator. The value of a `;` expression is non-determiniscally either the LHS value *or* the RHS value.

This can be used to genererate a sequence or infinite set:

    { (1,1) |> f ~ (a,b) => b;f(b,a+b) }

This set is defined by an argument `(1,1)` used to invoke `|>` a function `f ~ (a,b) => b;f(b,a+b)`. To be able to define the function recursively by itself, the function is aliased `~` by the name `f`. The actual function definition accepts a pair `(a,b)` and returns a choice between `b` and the function applied to `(b,a+b)`. 

The first invocation using `(1,1)` therefore returns a choice between `1` and the function itself applied to `(1,2)`. This second invocation returns a choice between `2` and the function itself applied to `(2,3)` and so forth.


The `;;` is the *undefined coalescing* operator. It evaluates to the LHS value unless this value is undefined, in which case it evaluates to the RHS value.

