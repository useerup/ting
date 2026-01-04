## Fibonacci

The following is an infinite list of Fibonacci numbers:

    // Fibonacci sequence
    [ (0,1) |> (var f # (a,b) => b ; f(b,a+b)) ]

Explanations:

- `[` `]` delimits a list. Between the delimiters, the list is defined by a comma-delimioted list of *candidate expressions*. Each candidate expression gives rise to zero or more elements. The candidate expression must be a fixed value *or* it must be an ordered choice of values. If a candidate expression is *nondeterministic* - as in this case - the list constructor will enumerate the choices and append them to the list before considering the next candidate expression.
- `(0,1)` is a simple tuple
- `|>` applies the argument on the left hand side to the function on the right hand side.
- `#` unifies the left hand side  with the right hand side. In declarative scope - as in this case - it declares the left hand side for the scope of the expression. Thus here it declares `f` as an alias for the function `(a,b) => b;f(b,a+b)`
- `(a,b)` is a tuple because it here is in declarative context, it declares the identifiers `a` and `b` as well.
- `=>` constructs a function from the argument onm the left hand side and the function expression on the right hand side. The operator declares the left hand side for the scope of the expression, i.e. both left hand side and right hand side.
- `;` is an *ordered choice*. The value of a `;` expression is nondeterministic, unless one of the operands is undefined. The `;` expression will first choose the left hand value, and when queried for an alternative value it will then return the right hand value. Thus, `b;f(b,a+b)` will have both the values `b` and `f(b,a+b)`, in that order.


    // Fibonacci function
    Fibonacci = var f # int x?>=0 => { 0->0, 1->1, n->(f(n-1)+f(n-2)) }

