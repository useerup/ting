

## Types (sets)

The types of the language are *sets*. Thus, a *set* is **not** a data structure; ratherf it is the set of values that inhabits the type. Examples of built-in types are:
- `int`
- `float`
- `string`
- `char`

Sets can be used as functions in function applications. When a set is used as a function, it is converted into its own *identity function*. This means than an expression like `int a` denotes a member of the type `int` bound to the value of `a`. This also implies that `a` is a member of `int` or can be *promoted* to a member of `int`.

Sets can be defined intensionally, extensionally and through set expressions. Some Examples:

- `{ 1, 2, 3 }`: Set of the numbers `1`, `2` and `3`.
- `{ 42, "flollop", 'A', 1.23 }`: A set of heterogenous values
- `{ (0,1) |> f ~ (a,b) -> b;f(b,a+b) }`: The set of Fibonacci numbers

- `int.Map(.ToString)`: The set of the string representations of all `int` members.
- `int*float`: Set of tuples of `int` and `float`
- `int ?? > 0`: The set of positive integers
- `float.Map( x -> (x,x*2) )`: The set of tuples of floats and their doubles.

## Tuples

Tuples are created by enclosing an expression list within parenthesis, e.g. `(1,2,3)`.

Sets of tuples can be defined `{` ... `}`

## Records

Records are created by enclosing an expression list in between `rec (` and `)`. The expression list must be a list of propositions (expressions of type `bool`) which declares the fields of the record.

When an expression appears in a *declarative scope* (as opposed to *referential scope*), it declares the identifiers of the expression in that scope.

The expressions of the expression list in a `rec(` ... `)` record literal declares the fields of the record. A number of expressions switch one or both of the argument back to referential. For instance, in a function application, like `f x`, the function `f` is referential while the argument continues in the declarative scope.

