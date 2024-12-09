## Functions


### Lambda expressions

Functions can be constructed through a *lambda expression*: 

    x => x * 2

The `=>` *lambda arrow* delimits the function *argument expression* from the function *definition expression*. The argument expression is *declarative*, meaning that identifiers of this expression are *declared*. The scope of these identifiers is *local* for the function definition. In the above example `x` is the argument expression (declares the identifier `x`) and `x * 2` is the definition expression (referencing `x`).

The argument expression can be a complex expression. The argument expression *constrains* the argument, which effectively limits the arguments for which the function is defined (the *domain* of the function). 

    int x => x * 2

Here `int x` is the argument expression. This *references* `int` and *declares* `x`. When a set like `int` is applied like a function, it is implicitly converted to the *identity function* for the set. An identity function is simply a function that returns the argument it is invoked with. An identity function *for a set* is an identity function which is *only defined for* members of that set. Such identity functions are often very useful - as in this example - when you want to constrain a value to one of a specific set of values.


### Set function notation

Internally, every function is a set of ordered pairs. Any set of ordered pairs can act as a function.

    { 1->2, 2->4, 3->6 }.AsFunc

Note that this is different from converting a set to a function (`IdentityFunc`).

The `AsFunc` uses the term "As" to convey the fact that it is the same set, but we request the *function* behavior. Similarly you can treat any function as a set of ordered pair:

    Math.Sin.AsSet      // The set of all ordered pairs x -> Math.Sin x


### The `Func` type

`Func` is the set of all functions. It is defined as:

    Func = { Any^^Any }


#### `Func..AsSet`: Functions as sets

Functions can also be viewed as sets of *function points* where each function point is an ordered pair of members from the domain and codomain, respectively.

You can convert a function into a set by referencing the property `AsSet`

    Double  =  x => x * 2
    Doubles  =  Double.AsSet

Some operators will implicitly convert a function to a set:

    `_:_`  =  (x,Func f) => x:f.AsSet
    `_::_`  =  (x,Func f) => x::f.AsSet


#### `Func..Domain` and `Func..Range`: Domain and range of functions

The `Domain` property of a function returns the *domain* of the function. The domain is the set of arguments that are accepted by the function.

Likewise, `Func..Range` returns the *range* of the function. The range of a function is the set of possible return values


#### `Append` and `Prepend`: Extending a function to create a new function

    Func..Append  =  f => Func g => f || g

    Func..Prepend  =  f => Func g => g || f

These are equivalent:

    f.Append g
    f |> Func..Append <| g
    (f |> Func..Append) g
    Func..Append f g
    f |> .Append g
    .Append g f

