I am still developing my **Ting** programming language. Ting is (will be) a pure logical/functional, object-oriented language.

Early in the design process I fully expected, that at some point I would have to come up with a syntax for pattern matching. 

However, I have now come to realize, that a generalized form of pattern matching may actually come for free. Let me explain...

In Ting, a non-function type is also it's own identity function. For instance, `int` is set of all 32-bit integers (a type). `int` is thus also a function which accepts an `int` member and returns it.

## Declarative and referential scopes

Instead of having separate variable declaration statements with or without initializers, in Ting an expression can be in either *declarative scope* or in *referential scope*. Identifiers are declared when they appear in declarative scope. When they appear in referential scope they are considered a reference to a declaration which must be in scope.

For compound expressions in declarative scope, one or more of the expression parts may continue in the declarative scope, while other parts may be in referential scope.

One such rule is that when function application appears in declarative scope, then the *function part* is evaluated in referential scope while the *argument part* continues in the declarative scope.

Thus, assuming declarative scope, the following expression

    int x

declares the identifier `x`, because `int` is evaluated in *referential scope* while `x` continues in the declarative scope. As `x` is the an identifier in declarative scope, it is declared by the expression.

The *value* of the above expression is actually the value of `x` because `int` is an identity function. At the same time, `x` is restricted to be a member of `int`, as those are the only values that is accepted buy the `int` identity function.

So while the above may (intentionally) *look* like declarations as they are known from languages such as C or Java where the type precedes the identifier, it is actually a generalization of that concept.

    (int*int) tuple                         // tuple of 2 ints
    (int^3) triple                          // tuple of 3 ints
    { [string Name, int age] } person       // `person` is an instance of a set of records

Functions can be declared by *lambdas* or through a *set construction*. A function is actually just a set of *function points* (sometimes called *relations*), as this example shows: 

    Double = { int x -> x * 2 }

This function (`Double`) is the set (`{`...`}`) of all relations (`->`) between an integer (`int x`) and the double of that integer (`x * 2`).


## Declaring through function argument binding

Now consider this:

    Double x = 42

For relational operators, such as `=`, in declarative scope, the *left* operand continues in the declarative scope while the *right* operand is in referential scope. 

This unifies `42` with the result of the function application `Double x`. Because it is known that the result is unified with `x * 2`, the compiler can infer that `x = 21`.
    
In the next example we see that `Double` can even be used within the definition of function points of a function:

    Half = { Double x -> x }

Here, `Half` is a function which accepts the double of an integer and returns the integer. This works because a function application merely establishes a relation between the argument and the result. If for instance `Half` is invoked with a bound value of 42 as in `Half 42` the result will be `21`.

Binding to the output/result of a function is in effect using the *inverse* of the function.


## Function through function points

As described above, functions can be defined as sets of function points. Those function points can be listed (extensional definition) or described through some formula which includes free variables (intensional definition) or through some combination of these.

    Map = { 1 -> "One", 2 -> "Two", 3 -> "Three" }                  // extensional
    
    Double = { int x -> x * 2 }                                     // intensional
    
    Fibonacci = { 0 -> 1, 1 -> 1, x?>1 -> This(x-1) + This(x-2) }   // combined

In essense, a function is built from the function points of the expression list between the set constructor `{` and `}`. If an expression is *non-deterministic* (i.e. it can be evaluated to one of a number of values), the set construction "enumerates" this non-determinism and the set will contain all of these possible values.


## Pattern matching

The following example puts all of these together:

    Square = class { [float Side] }
    
    Circle = class { [float Radius] }
    
    Rectangle = class { [float SideA, float SideB] }

    Area = {
        Square s -> s.Side^2
        Circle c -> Math.Pi * c.Radius^2
        Rectangle r -> r.SideA * r.SideB
    }


Note how the `Area` function looks *a lot like* it is using pattern matching. However, it is merely the consequence of using the types identity functions to define function points of a function. But, because it is just defining function argument through the result of function applications, these can be made arbitraily complex. The "patterns" are not restricted to just type constructors (or deconstructors).

*Any* function for which the compiler can derive the inverse can be used. For identity functions these are obviously trivial. For more complex functions the compiler will rely on user libraries to help inverting functions.

Finally, the implementation of a non-in-place quicksort demonstrates that this "pattern matching" also works for lists:

    Quicksort = { () -> (), (x,) -> (x,), (p,a) -> This(t??<=p) + (p,) + This(t??>p) }

