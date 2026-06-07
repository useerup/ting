### Arithmetic operators

#### `+` addition

The `+` operator adds two numbers or concatenates two lists (inclusing `string` as `string` is a list)

Signatures: 

| left   | right  | result |
| ------ | ------ | ------ |
| byte   | byte   | byte   |
| sbyte  | sbyte  | sbyte  |
| short  | short  | short  |
| ushort | ushort | ushort |
| int    | int    | int    |
| uint   | uint   | uint   |
| long   | long   | long   |
| ulong  | ulong  | ulong  |
| string | string | string |

    

#### `-` subtraction

The `-` operator subtracts the right operand from the left operand.


#### `*` multiplication

For **numeric operands** the `*` operator returns the product of the two numeric operands.

For **set operands** the operator creates a set of tuples of the two sets. `int*float` creates a tuple

#### `/` division

For **numeric operands** the `/` operator divides the left operand by the right operand numerically.

For **set operands**, divides a tuple set (left operand) by the right operand set. 

For the following examples assume that `S = int*float*string`

    S/string                // equivalent to `int*float`
    S/(float*string)        // equivalent to `int`
    S/float                 // error
    S/(int*string)          // error



#### `^` eksponentiation

The `^` operator raises the left operand to the power of the right operand.


#### `-->` reverse eksponentiation

The `-->` operator raises the right operand to the power of the left operand.

It is commonly used to define sets of functions

Examples

    int --> int         // The set of functions from `int` to `int`
    int*int --> int     // The set of functions from `int*int` to `int`


#### `%` remainder

The `%` operator returns the remainder from dividing the left operand by the right operand.

#### `/%` division with remainder

The `/%` operator performs division and returns both quotient and remainder as a tuple.



