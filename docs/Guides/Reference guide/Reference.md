## Reference


### Declarations

#### `let` declarations

#### `var` declarations

#### `private` declarations

#### `public` declarations

#### `protected` declarations

#### `internal` declarations



### Literals

#### Numeric literals

**TODO**

#### Structure literal

| Symbol          | Description                |
| --------------- | -------------------------- |
| `(` ... `)`     | Tuple                      |
| `{` ... `}`     | Set                        |
| `[` ... `]`     | List                       |
| `(.` ... `.)`   | Single record              |
| `{.` ... `.}`   | Record set                 |
| `(\|` ... `\|)` | Discriminated alternatives |
| `{\|` ... `\|}` | Discriminated union        |
| `{^` ... `^}`   | Function                   |


#### `{.` `.}` Record-structured set literal

#### `{|` `|}` Discriminated union literal

    Pieces = {| King, Queen, Rook, Bishop, Knight, Pawn |}

    Pieces.King a

#### `{^` `^}` Function literal


### Operators

| Symbol            | Description                            | Group            |
| ----------------- | -------------------------------------- | ---------------- |
| `+`               | Addition                               | arithmetic       |
| `-`               | Subtraction                            | arithmetic       |
| `*`               | Multiplication                         | arithmetic       |
| `/`               | Division                               | arithmetic       |
| `/%`              | Division with remainder                | arithmetic       |
| `%`               | Remainder of division                  | arithmetic       |
| `%%`              | Is divisible by                        | arithmetic       |
| `^`               | Exponentiation                         | arithmetic       |
| `^^`              | Exponential                            | arithmetic       |
| `**`              | Tuple construction                     | arithmetic       |
| `++`              | reserved                               |                  |
| `\|`              | Or                                     | logical          |
| `\|\|`            | Conditional or                         | logical          |
| `&`               | And                                    | logical          |
| `&&`              | Conditional and                        | logical          |
| `!`               | Not                                    | logical          |
| `then` ... `else` | Implication with alternative           | logical          |
| `then`            | Implication                            | logical          |
| `=`               | Equals                                 | binding          |
| `==`              | Equivalent to                          | relational       |
| `<`               | Less than                              | relational       |
| `<=`              | Less than or equal to                  | relational       |
| `>`               | Greater than                           | relational       |
| `>=`              | Greater than or equal to               | relational       |
| `:`               | Is member of                           | relational       |
| `::`              | Is collection of                       | relational       |
| `:::`             | Is sequence of                         | relational       |
| `+:`              | Append                                 | list             |
| `:+`              | Prepend (cons)                         | list             |
| `-:`              | Remove first                           | list             |
| `:-`              | Remove last                            | list             |
| `?`               | Constrain value by function            | filter           |
| `??`              | Filter set/list/function by function   | filter           |
| `\`               | Constrain value by expression          | filter           |
| `\\`              | Filter set/list/function by expression | filter           |
| `=>`              | Lambda function                        | functional       |
| `~~>`             | Lambda function, non-declaring         | functional       |
| `<--`             | Reverse lambda function                | functional       |
| `<~~`             | Reverse lambda function, non-declaring | functional       |
| `->`              | Ordered pair                           | functional       |
| `<-`              | Reverse ordered pair                   | functional       |
| `~>`              | Ordered pair, non-declaring            | functional       |
| `<~`              | Reverse ordered pair, non-declaring    | functional       |
| `>>`              | Function composition                   | functional       |
| `<<`              | Function composition, reverse          | functional       |
| `\|>`             | Function invocation                    | functional       |
| `<\|`             | Function application                   | functional       |
| `!!`              | Inverse                                | functional       |
| `class`           | Create class                           | class            |
| `extend`          | Extend class                           | class            |
| `abstract`        | Abstract property                      | class            |
| `virtual`         | Virtual property                       | class            |
| `override`        | Override virtual property              | class            |
| `managed`         | Managed set/function                   | managed          |
| `:=`              | Oppropose                              | managed          |
| `catch`           | Catch exception                        | exception        |
| `throw`           | Throw exception                        | exception        |
| `throws`          | reserved                               | exception        |
| `with`            |                                        |                  |
| `where`           |                                        |                  |
| `;`               | Choice                                 | choice           |
| `...`             | Range                                  | choice           |
| `<...`            | Range exclusing lower bound            | choice           |
| `...<`            | Range excluding upper bound            | choice           |
| `<...<`           | Range excluding lower and upper bound  | choice           |
| `partial`         | Partial definition                     | choice           |
| `>>>`             | Fold expressions right                 | expression lists |
| `<<<`             | Fold expressions left                  | expression lists |
| `public`          |                                        | declarative      |
| `private`         |                                        | declarative      |
| `internal`        |                                        | declarative      |
| `protected`       |                                        | declarative      |
| `let`             |                                        | declarative      |
| `var`             |                                        | declarative      |
| `is`              |                                        | declarative      |
| `in`              |                                        | declarative      |
| `.` *identifier*  |                                        |                  |
| `..` *identifier* |                                        |                  |
| `./`              |                                        |                  |
| `../`             |                                        |                  |

---
#### Arithmetic operators

##### Addition: `+`

The operator has the following signature:

    || >>>
    
    Int8 * Int8 ^^ Int8
    Int16 * Int16 ^^ Int16
    Int32 * Int32 ^^ Int32
    Int64 * Int64 ^^ Int64
    UInt8 * UInt8 ^^ UInt8
    UInt16 * UInt16 ^^ UInt16
    UInt32 * UInt32 ^^ UInt32
    UInt64 * UInt64 ^^ UInt64

    List * List ^^ List
    Set * Set ^^ Set
    Func * Func ^^ Func



##### Subtraction: `-`

##### Multiplication: `*` 

##### Division: `/`

##### Division with remainder: `/%`

##### Remainder: `%`

##### Divisible: `%%`

---
#### Logical operators

##### Or: `|`


##### Guarded or: `||`

`bool*bool`: 
- Returns `true` if one of these holds:
  - left operand evaluates to `true`. 
  - left operand evaluates to `false` and right operand evaluates to `true`
- Returns `false` if both operands evaluates to `false`.

Checked exceptions resulting from evaluating the right operand are caught and ignored if any of the following is true
- the left operand evaluates to `false` 
- an exception is thrown during evaluation of the left operand

This semantic makes the operator work almost like the conditional-or operator in C#. 

`Func*Func`:

Combines two functions, *f* (left operand) and *g* (right operand) such that the resulting function returns *f x* for any *x* that *f* is defined for and *g x* for any *x* that *f* is not defined for but *g* is defined for.

    FuncGuardedOr = (Func f, Func g) => x => x:f.Domain then f x else g x




##### Double-guarded or: `|||`

`bool*bool`: returns `true` any one of these holds:
- left operand evaluates to `true`. 
- right operand evaluates to `true`. 

Returns `false` if both operands evaluates to `false`.

Checked exceptions resulting from evaluating any of the operands are caught and ignored if the other operand evaluates to `true`.


##### And: `&`
##### Guarded and: `&&`
##### Double-guarded and: `&&&`
##### Or: `|`

---
#### Relational operators
---
#### Filter operators
---
#### Functional operators

---
#### Class operators
##### Class operator `class` 

The prefix `class` operator creates a *class* from a *model set*. 

The class is itself a set. Each element of the class set corresponds to an element of the model set, but the class element (*instance*) and the model set element are considered *separate* elements.

##### Override `override`
`override` overrides a property function with a value. Often this property is a function-valued property (method).

Rather than just swapping in a new value, the right operand of `override` must be a function which accepts the *current* value and returns the *new* value. This way the override can compose the new value based on the existing value.


#### Managed properties operators
#### Exception operators

#### Choice operators

##### Ranges `...`, `<...`, `...<`, `<...<` 

#### Expression list operators







##### Conditional-or  `||`

This is the binary *conditional-or* operator. It is defined for `bool*bool`, `Set*Set` and `Func*Func`.

`bool a, bool b`: Returns true when `a` is `true` or when `a` is `false` and `b` is `true`.

`Func a, Func b`: Returns a function 

`Set a, Set b`: 

In addition to being defined for booleans, it is also defined for sets and functions. For functions, it will combine two functions to form a new function. The new function is defined by all of the ordered pairs of the left function and those ordered pairs from the right function where the left



### Operator functions

#### `_+_` function

This is the function to which a `+` operator expression is parsed. Identifiers are allowed to contain special characters - even spaces - as long as they are delimited by backticks. By overriding this function we are effectively overloading the `+` operator.



#### `>>>`, `<<<` fold operators

`>>>` is the *fold right* operator. It accepts a function on the left and an expression list on the right. When its is  followed by a line break instead of a token, it expects a line-break delimited list.

`<<<` is the *fold left* operator. It accepts a binary function on the left and an expression list on the right. When its is followed by a line break before any token, it expects a line-break delimited list.

#### `=>` Lambda function


