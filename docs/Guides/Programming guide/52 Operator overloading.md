## Operator overloading

### Overloading existing operators

Every operator is mapped to an underlying function. When this function is virtual, you can override the function to effectively overload the operator.

The following example overloads the `+` operator so that it can be used to add complex numbers (represented by the `Complex` type). The `+` operator is mapped to the function named `_+_`[^operatornames]:

    `_+_` override .Append
        ( Complex a, Complex b ) => Complex.New( a.Real+b.Real, a.Imaginary+b.Imaginary )

(`.Append` above is a reference to `Func..Append` because the right operand of `override` is a function (member of the `Func` set). Used like this with just the leading period, it is a curried function which accepts its argument and returns a function which accepts the function and applies it to the argument)

[^operatornames]: The underscores around the operator symbol represents the operands. By convention, a binary infix operator is  mapped to a function with underscores around the operator symbol (example: `_+_`), a unary prefix operator has a single trailing underscore (example: `-_`) and a unary postfix operator has a single leading underscore (example: `_!!`).

### Creating new operators

    