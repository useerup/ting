### The `\` "such that" operator

The `\` operator is *declarative* and *right-referential* meaning that it introduces a new local scope which is declarative in its left operand and referential in its right operand.

A *such-that* expression evaluates to the value of the left operator, provided that the right operator is satisfied (evaluates to `true`) for the value.

    int x \ x != 0      // A value that is not equal to 0

The operator is frequently used in combination with set- and list definitions.

    { int x \ x != 0 } 

This example reads as "the set of `int` members such that the `int` member is greater than zero". 