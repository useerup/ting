Inspired by [this blog post](https://www.47deg.com/blog/why-types/) from 47 degrees, I wondered how dependent types would look in my imaginary language *Ting* (it is still imaginary as I don't have a compiler yet - I am working on it!). Specifically this part:

> Consider an indexing function that is given a list or array, and a position to get: whether such a call is correct depends on whether the position is within the bounds of the array, a relation between both inputs.

In Haskell (LiquidHaskell), the blog post suggests this definition:

```haskell
get :: (xs : [a]) -> { n : Int | 0 <= n && n < length xs } -> { v : a | v `elem` xs }
```

As I understand this, `{ n : Int | 0 <= n && n < length xs }` denotes a type that constrains an integer to be between 0 and the length of `xs`.

In *Ting* I could write this function in this way:

    get  =  Lists xs  \  { int _ ? >=0 ? < xs.Length } n \  xs.ElementAt n

`\` is the "lambda" in Ting. It delimits the argument part and the result part. The argument (left operand) is *declarative* while the result (right operand) is *referential*. I have chosen this single-character symbol rather than the usual `=>` or `->` because I want to emphasize that this is a very important operator.

`Lists` is the *set of all lists*. A set in *Ting* is a type; not a data structure. All types are also the identity function on itself. Thus, `Lists xs` is an expression that "matches" a list and declares `xs` and binds it to that list.

`{` and `}` is a *set constructor*. `int` is the set of integers. `int _` is thus an element of that set (the identity function applied to an argument that is discarded). 

`?` restricts the left operand to those values that satisfy the right operand, which must be a predicate (essentially a function returning a `bool` element).

`>= 0` and `< xs.Length` are such predicates. Thus `{ int _ ? >=0 ? < xs.Length }` denotes the set of integers that can be used to index into the list. Used as an identity function it declares and binds `n` to be an element of that set.

All lists has a number of properties/methods. One such method is `ElementAt`. It accepts an index into the list and returns the element at that position. The method is defined *only* for the indexes of the list. This is checked at compile time. If the compiler cannot determine that the method will be invoked for supported values only, it will report it as an error.

However, Ting is also object-oriented, and as such the above definition can be shortened. 

All functions (and by extension all types since they are also identity functions) has a *domain* and *codomain*. These are exposed as member properties `Domain` and `Codomain`. These are already dependent types, so I can substitute the set constructor for the actual domain of the list identity function:

    get  =  Lists xs  \  xs.ElementAt.Domain n \  xs.ElementAt n

However, this is silly, because the last lambda is really the same as the `ElementAt` method itself. Thus I can simplify to:

    get  =  Lists xs \ xs.ElementAt

But I will be able to do better than this, thanks to the `..` selector. The ordinary `.` selector (as in the typical OO language; not as in Haskell) selects a named property from its argument. Technically, a single-property projection.

The `..` operator projects from the *members* of a set. When members of the `Lists` set all expose a property/method called `ElementAt`, I can denote a function that projects from a list to a property/method of the elements by using `..`

    get  =  Lists..ElementAt

This `get` function is equivalent to all of the above versions.

All functions in *Ting* are inherently dependently typed. Rather than throwing exceptions, a function *should* restrict the domain to only values which are guaranteed to allow evaluation. A function that is guaranteed to compute for all values of it's domain is a *total function*. Division, for instance, is a total function because it *excludes* zero from the right argument.

Of course some function, like reading a file, will depend on a dynamic environment, i.e. it cannot be guaranteed that a given file exists when compiling the code, so a function to retrieve the contents of a file may be "undefined" for a given file path *at runtime*.

My plan is to allow function to be marked as *partial* functions. This instructs the compiler, that *some invocations* may fail when the evaluation reveals, that the function was not actually defined for the  arguments.