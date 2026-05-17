This is interesting and something that I have already spent (too much) time pondering.

In Ting I try to decouple representation and delay the decision on how to actually represent a structure for as long as possible. Ting, being a logic language, I try to focus on the *semantics*. And (barring mutation) an array certainly *semantically* looks like a function *whose domain is a contiguous subset of the integers* as the Haskell documentation so eloquently describes it.

So in Ting I turn it upside down: Any function whose domain is a contiguous subset of  integers is a candidate to be represented as an array.

In Ting I also have ranges like Futhark `i..<k`. It is also very similar syntactically: `i...<k` (I *really* needed that `..` token for another operator ;-) ). 

However, Ting is not an array language like Futhark, so in Ting that expression is actually a *nondeterministic* value. Thus `i...<k` is an expression which may assume any of the values in the range, nondeterministically (or as *choices*).

So, given that `f` is a function over integers, the expression `f (i...< k)` is formally allowed in Ting. However, it is a *nondeterministic* expression because it can assume any value that `f` produces when applied to one of the possible values of `i...<k`. In a sense the nondeterminism of the argument **spreads** to the entire expression. Nondeterminism has that tendency, as anyone who has ever programmed in Prolog will attest to.

However, In Ting we can make this into a *list* by embedding the expression within `[` and `]`. Like in many other languages the `[` `]` *list literal* accepts a list of expressions which then forms the list. Unlike most other languages it also **unwinds nondeterminism** of its expressions. When the nondeterminism is *countable* then the actual list will be deterministic, because there is an ordered way to unwind the nondeterminism.

The following expressions are all examples of lists:

    // simple list
    [ 1, 2, 3 ]

    // list of even integers 0, 2, -2, 4, -4 ...
    [ int n \ n % 2 == 0 ]   

    // list of quadratic integers 0, 1, 2, 4, 16 ...
    [ n^2 \ n:int & n >= 0 ]

    // list of Fibonacci numbers 0, 1, 1, 2, 3, 5 ...
    [ (0,1) |> let f ?= ( (a,b) => a; f(b,a+b) ) ]   

Back to the examples of the article: In Ting `[f (i...< k)]` is the *image* of `f` over the range `i...<k` captured in a list. 

Now, if one thinks about `f` as an *array* (a function from `int` to some value) instead, all of the above still holds. Furthermore `[f (i...< k)]` then returns a list containing a *slice* of the "array" `f`. This list is not itself a *slice*, but it does contain all the members of what would be an array slice in the same order. 

Ting does not have *array* or *slice* as concepts as that is (in Ting) a representation detail. But I will argue that if `f` is *represented* using an array, then `[f (i...< k)]` could be represented as a slice (or span?) of that array.

