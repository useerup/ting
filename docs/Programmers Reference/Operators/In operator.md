### The `in` operator

Accepts a proposition as its left operand and any expression as its right operand. The value is that of the right operand, provided that the lefty operand evaluatess to `true`. If the left operand evaluates to `false` or is undefined, then the value of the `in` expression is undefined.

The `in` operator is *right-referential*, which means that when it appears in a declarative scope, then the left operand is *declarative* and the right operand is *referential* in that scope.