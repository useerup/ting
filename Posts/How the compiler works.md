Your program in Ting is an expression. The compiler will (generate a program which will) try to find a valuation that makes your program (expression) *defined.* 

So the core of the compiler is basically a theorem prover. But unlike a regular theorem prover, Ting utilized the concept of free or bound variables. 

Consider a simple proposition:

    let answer = 42

here `answer` is initially free. The compiler recognizes that it has an equality relation between a free variable and a bound expression. That is one of the intrinsic problems it can solve on its own: It assigns (or rather generate code to assign) the bound value to the fraa variable, which then is bound.

A slightly more complicated example:

    0 = 42 - answer

In this case the left side is bound and the right side is *partially* bound. In this case the compiler relies on a library which will rewrite to a *normal form*. The library rewrites the equation into 

    0 + answer = 42

and then (by reduction)

    answer = 42

So, how does the library rewrite into a normal form? By means of *rewriting rules*. A rewriting rule is one which matches a certain pattern in the target expression and then rewrites it into something equivalent but closer to a normal form. Rewriting rules are applied recursively and repeatedly until no rewrite rule applies any more. If the rewrite rukes have been designed carefully, the expression is now in a normal form.

How does it match a pattern? By axiomatic pattern matching. rewrite rules take into account the free/bound status of variables. You can think of it as they always try to isolate an unbound variable on the left hand side and make the right hand side bound.

So, how does this relate to function inversion?

Functions are just relations. Consider this:

    let f y = 84
    let f = int x -> x * 2

This can be rewritten into

    (x * 2) = 84
    x = y

Again (rewrite):

    x = 84 / 2
    x = y

And finally

    y = 42

Now these are all *constant* programs. They are quite uninteresting, as they can be reduced/solved at compile time. But consider the above, but start with

    let bound value     // a value is known at runtime, but not at compile time
    let f y = bound value

Now the compiler can go through the same steps, but it will have to generate code to "redo" them at runtime. I call this *pseudo-evaluation*.

