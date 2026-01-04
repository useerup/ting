### Identity operators

#### The `=` "equals" operator

#### The `?=` "unify" operator

Unifies the left operand with the right operand. If unification is not possible, then the expression is undefined.

The `?=` operator is right-referential. This is frequently used to refer to an expression in a 
recursive subexpression, like in this anonymous definition of the factorial function:

    let f ?= fn{ 0 -> 1, (int??>0) n => n*f(n-1) }

`f` is *declared* by the LHS of `?=` and *referenced* in the RHS. 


