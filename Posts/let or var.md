Many programming languages use types or type annotations to describe for which values a function is *defined* and what type of values it may return. These are the *domain* of the and the *co-domain* of the function. Some languages will *infer* the domain and co-domain (or even the *range*) of a function.

The declared (or inferred) domain is often used check that the type of an argument matches the type of the corresponding parameter of a function. 

I am in the *infer* camp with the **Ting** language. In fact, through refinement types and dependent types I plan to go *really* tight around *definedness*. 

Take the operator division `/`. This operator is defined for all numeric types (floating point, decimals, signed/unsigned integers etc.), but it *is not defined* for right operands of zero:

    // The function behind the `/` operator
    (/) = Ints.Divide || Floats.Divide || ...

    // Type-specific division functions
    Ints.Divide = { (Ints a, Ints.Where(!=0) b ) --> ... }
    Floats.Divide = { (Floats a, Floats.Where(!=0) b ) --> ... }
    ...

 `...` are details that I left out.

`Floats.Where(!=0)` is an example of a *refinement type*. Types in Ting are called *sets* and are "first class citizens" of the language - which means that we can do set-arithmetic and filtering etc on them. In this case the `Where` method *filters* the set to only those members that satisfy the predicate `!=0`[^1].

In Ting we do not have arrays. There's a very good reason for this: If we leave mutability out of it, what is an array other than just a function from an integer index to some value?

You may argue that an array is also about memory layout. Yes, but Ting is a high-level language which is concerned about the *logic*. The actual representation as a vector in memory, I consider that an implementation detail[^2].




[^1]`!=` is a binary operator, but in Ting we are allowed to use binary operators in prefix position. When used like that, the operator is converted to a curried function. When applying only one argument the result is a predicate, because the resulting function will accept an argument and returns a `bool` value.

[^2] This is not to say that I will not support using arrays as a data structure - but I decouple this from the logical shape of an array. The choice of an array as a data structure is a compiler decision - with the help of libraries - when certain criteria are met).

