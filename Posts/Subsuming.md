> The idea is that it should be possible to describe exactly every set of values with a type. 

This is what I am trying to do in the language I am designing. I am still struggling with writing a compiler for it.

It is a logic, functional and object-oriented language based loosely(!) on math notation. Sets are foundational and form the types of the language. A set is described by its *set condition* which is the predicate that determines whether a value is in the set or not.

So these are some examples of sets:

    { 1, 2, 3 }

    { "Arthur Dent", "Ford Prefect", "Zaphod Beeblebrox" }

The set condition for the first set is `x => x=1 || x=2 || x=3`. The set condition for the second set is likewise `x => x="Arthur Dent" || x="Ford Prefect" || x="Zaphod Beeblebrox`.

The set notation "unwinds" any non-determinism of the set elements. This corresponds to intensional set definition in math. The following is the set of all positive integers:

    { int _ ? >0 }

The expression `int _ ? >0` is a non-deterministic expression which can be bound to any integer that is greater than 0. 

So a set can be defined both intensionally and extensionally or by a combination of both.

Functions are sets of *function points*, i.e. functions are also sets/types. The following is the *Fibonacci* function:

    Fibonacci = { 0->0, 1->1, let int n?>1 -> these(n-1)+these(n-2) }

`->` is the *function point constructor*. `these` is a reference to the entire set (the  enclosing `{`...`}`).

How is this relevant to your question, you ask? The Fibonacci function is only defined for the numbers 0, 1, and any integer greater than 1. The *domain* of the function is thus a set with the set condition `x => x=0 || x=1 || x:int && x>1`.

So going back to your idea: 
> it should be possible to describe exactly every set of values with a type. 

This is what I do with the set condition predicates. The compiler will reason over these predicates. A type (set) A is a subtype (subset) of type (set) B, if and only if all values that satisfy the set condition of A also satisfy the set condition of B.

So a function application like `Fibonacci "Arthur Dent"` will yield a compile error, because there is no way to unify the predicates `x => x=0 || x=1 || x:int && x>1` and `x => x="Arthur Dent"`. 

Now consider a function

    let Double = float x => x * 2

This `Double` function can be used like this:

    let Double answer = 84

The compiler will simplify this to `let answer = 42` at compile time. 

Now consider a `Half` function

    let Half = Double x => x

This states that `Half` is a function that accepts an argument which must be equal to the result applying Double to some value `x`. The result of the function is `x`.

The compiler will simplify this to `let Half = x => x / 2` because it can use axioms/theorems defined for the function `*`.

This is not exactly pattern matching, but when you go beyond simple types and enter into structured types like records, it certainly starts to look like it:

Let's define a `Person` type (set):

    let Person = { (. Name:string, Age:int .) }

Now we can define a function to get the `Name` value of any record which has a `Name` which is a member of the `string` set:

    let GetName = (. Name:string .) => Name

This looks suspiciously like pattern matching. 

So how do we create an expression which will pattern match like the one in your example:

    match value with
    | Some(0) -> ...
    | Some(S(Nat)) -> ...
    | None -> ...

Consider this expression:

    value |> { 
        (. Some=0 .) -> ... 
        (. Some n .) given n:Nat -> ...
        (. None=() .) -> ...
    }

The `|>` operator is the "pipeline" operator. `x |> f` is the same as function application `f x`. The *function* here is defined through 3 function point expressions.
* `(. Some=0 .) -> ...` specifies that the set contains a function point which "maps" from a record which has a property `Some` the value which is 0.
* `(. Some n .) given n:Nat -> ...`. This function point is non-deterministic (i.e. defines multiple function points) and maps any record which has a property `Some` which has a value that is a member of the `Nat` set.
* `(. None=unit .) -> ...` defines a function point that maps any record which has a `None` peoperty which is the `unit` value.

Now, to address a few elephants: 

All of this does make type checking (potentially) undecidable. It will certainly be possible to write programs for which the compiler will have to give up.

For instance, it is a compile time error if the compiler cannot prove that `value` above is squarely within the domain of the function. In this case the programmer must intervene and help the compiler out, e.g. by being more specific about domains of function, values of sets etc.

Some programs will not just be undecidable but also non-terminating (evil programmers). To handle that I will have to put a limit on the "depth" of the analysis and report it as a compile time error that compilation seems to be non-terminating.