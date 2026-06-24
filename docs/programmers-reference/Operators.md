## Operators

Binary operators can be categorized as follows:

| pattern           | in declarative context                                                                 | in referential context                                                                                             |
| ----------------- | -------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------ |
| left-declarative  | Creates a local scope spanning the operator expression. Both operands are declarative. | Creates a local scope spanning the operator expression. Left operand is declarative; right operand is referential. |
| right-declarative | Creates a local scope spanning the operator expression. Both operands are declarative. | Creates a local scope spanning the operator expression. Right operand is declarative; left operand is referential. |
| left-referential  | Left operand is referential; right operand is declarative.                             | Both operands are referential.                                                                                     |
| right-referential | Left operand is declarative; right operand is referential.                             | Both operands are referential.                                                                                     |
| non-declarative   | Both operands are declarative.                                                         | Both operands are referential.                                                                                     |

The publishing operators (`public`, `private`, `internal`, `protected`) each creates a scope for which the operand expression is declarative. The operators can be nested. In that case the rule about overlapping declarative contexts comes into play, which means that identifiers that are declared in multiple contexts are unified. This effectively means that `protected internal int Answer` declares `Answer` as both a *protected* identifier and an *internal* identifier, and that both declarations refer to the same value.

## Arithmetic operators

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


#### `^^` reverse eksponentiation

The `^^` operator raises the right operand to the power of the left operand.

It is commonly used to define sets of functions

Examples

    int ^^ int         // The set of functions from `int` to `int`
    int*int ^^ int     // The set of functions from `int*int` to `int`


#### `%` remainder

The `%` operator returns the remainder from dividing the left operand by the right operand.

#### `/%` division with remainder

The `/%` operator performs division and returns both quotient and remainder as a tuple.



## Choice operators


#### The `;` *choice* operator

The `;` *choice* operator creates a choice between two values

The value of `a ; b` is nondeterministically `a` or `b`. At any one time the expression can be only one of the values, but backtracking can cause the expression to choose the other value. The operator introduces *ordered nondeterminism* in that it will choose the value of `a` until backtracking forces it to choose the value of `b`

If any of the operands are undefined the they are disregarded. Only if both operands are undefined is the value of the expression undefined.

Choice operator is handy when defining *generator functions* together with set- or list definitions to produce an procedurally defined set or list.


##### Example: List of Fibonacci numbers

The following is an example of a list of Fibonacci numbers:

    {* let fib = (a,b)=>a;fib(b,a+b) in fib(0,1) *}

The list evaluates to `[0, 1, 1, 2, 3, 5, …]`, i.e. the Fibonacci sequence, because successive backtracking steps force each new choice produced by the generator. The list is - in principle - infinite, as no upper bound has been specified. This means that the numbers will be generated on-demand. However, the `int` set is a 32-bit signed integer and the generator *will* cause an overflow when the list is enumerated past the overflow point.

Step-by-step:

1. `[` `]` is a [list definition](../Lists.md#list-definitions). The literal contains a single expression, so the list is formed by unwinding every value that expression can produce.
2. `let … in …` introduces a local binding. The declarative part `fib = (a,b) => a ; fib(b, a+b)` must succeed first; once it does, the right-hand expression `fib(0,1)` is evaluated using that binding.
3. The local `fib` function is the generator. The choice operator `;` yields `a` first; only when backtracking demands another value does it evaluate `fib(b, a+b)` which in turn yields the next Fibonacci number. Because `;` is ordered, the sequence the list observes is precisely the Fibonacci sequence.
4. The call `fib(0,1)` seeds the generator with the canonical starting pair. The enclosing list literal consumes each emitted value, giving a deterministic list even though the generator itself is nondeterministic.




#### The `;;` *alternative choice* operator 

The value of `a ;; b` is `a` *unless* `a` is undefined in which case it is `b`.

An `;;` expression will not by itself introduce nondeterminism, though `a` or `b` may themselves be nondeterministic expressions.

`a ;; b` is essentially equivalent to `(a catch UndefinedException _ => b)`, i.e. evaluate `a` and if an `UndefinedException` is raised from that evaluation then return the value of `b` instead.

## Class operators


#### The `class` operator


Examples

    Person = class records(Name:string, DateOfBirth:DateOnly)

    42.0#m/s^2


#### The `extend` operator


    Student = Person extend {
        
    }

## Comparison operators

#### `<` less than

#### `<=` less than or equal to

#### `==` equal to

#### `>=` greater than or equal to

#### `>` greater than

#### `!=` not equals by value

## Exception operators

### The `catch` operator

The `catch` operator catches exceptions throw from evaluating the value of an expression.


### `throw` operator

`throw` is a prefix operator

## Functional operators

#### `=>` "lambda" function

#### `->` "ordered pair" operator

An ordered pair is constructed with the infix operator `->`. It produces an ordered pair whose left component is the LHS and whose right component is the RHS.

#### `**` "fusion" operator

#### `^^` "set of functions" operator

`^^` creates a set of functions from LHS set to RHS set. Both LHS and RHS must be sets.

Example:

    int^^string     // the set of functions from `int` to `string`

#### `>>` and `<<` "pipeline" operators

The pipeline operators `>>` and `<<` are used to compose functions, passing the output of one function as the input to the next.

- `f >> g` creates a new function that applies `f` to its argument, then applies `g` to the result.
- `g << f` is equivalent to `f >> g`, composing `f` and `g` in the same order.

Example:

    let add1 x = x + 1
    let double x = x * 2

    let addThenDouble = add1 >> double
    addThenDouble(3) // evaluates to 8

These operators help build complex function pipelines in a readable, declarative style.

A common idiom is to compose a function with a set. When used as a function, a set acts as its own identity function. This can be used to restrict a function to be defined only for a subset of its arguments. 

Consider for example a function `ExpectChar` which "parses" the first character of a string: `ExpectChar = char c => [c,,rest] => rest`. We can define the set of digits as `Digits = {'0'...'9'}`. We can now define a function which will only parse diggits: `ExpectDigit = Digits >> ExpectChar`.


#### `|>` and `<|` "function application" operators

Examples

    f <| x          // same as `f x`
    x |> f          // same as `f x`

Unlike the juxtaposition function application, the function application operators are are *binding neutral*: In declarative binding position **both** operands are declarative. In referential binding position **both** operands are referential.

#### Automatic set-to-function conversion

When a set is used in a position where a function is expected, it is promoted to its own *identity function*. An identity function of a set is a function which accepts members of the set and returns the same member.

This is frequently used to restrict variables during declaration, such as `let float a = 42`. In this case `float a` is a function application. `float` is a set, so it is promoted to the function `float x=>x`. Thus it is equivalent to `let ref(float x=>x) a = 42` and thus `a` is inferred to be a member of `float`.

## Grammar-shift operators

If the grammar-shift operator is the last token on a line, the expression list will start indented on the next non-blank line, and be delimited by line breaks.

If the grammar-shift operator is **not** the last token on a line, then the remainder of the line is parsed and white-space separated expressions

#### The `>>>` fold left-to-right operator

    Identifier = [h]+t => 
        >> >>> (WordBeginChar h) (ZeroOrMore WordFollowChar t)

#### The `<<<` fold right-to-left operator


#### The `(,,,)` operator

#### The `[,,,]` operator

#### The `{,,,}` operator

## Identity operators

#### The `=` "equals" operator

#### The `?=` "unify" operator

Unifies the left operand with the right operand. If unification is not possible, then the expression is undefined.

The `?=` operator is right-referential. This is frequently used to refer to an expression in a 
recursive subexpression, like in this anonymous definition of the factorial function:

    let f ?= fn{ 0 -> 1, (int??>0) n => n*f(n-1) }

`f` is *declared* by the LHS of `?=` and *referenced* in the RHS.

## In operator

### The `in` operator

Accepts a proposition as its left operand and any expression as its right operand. The value is that of the right operand, provided that the lefty operand evaluatess to `true`. If the left operand evaluates to `false` or is undefined, then the value of the `in` expression is undefined.

The `in` operator is *right-referential*, which means that when it appears in a declarative scope, then the left operand is *declarative* and the right operand is *referential* in that scope.

## Logical operators

#### `&` strict conjunction

Strict (non-short-circuit) conjunction evaluates both operands; if either side is undefined the result is undefined.

| left        | right       | result      |
| ----------- | ----------- | ----------- |
| `true`      | `true`      | `true`      |
| `true`      | `false`     | `false`     |
| `true`      | *undefined* | *undefined* |
| `false`     | `true`      | `false`     |
| `false`     | `false`     | `false`     |
| `false`     | *undefined* | *undefined* |
| *undefined* | `true`      | *undefined* |
| *undefined* | `false`     | *undefined* |
| *undefined* | *undefined* | *undefined* |


#### `&&` guarded conjunction

If the left operand is false, any undefinedness from the right operand is caught and the result is `false`. If the left operand is `true`, the result equals the right operand. If the left operand is *undefined*, the result is *undefined*.

| left        | right       | result      |
| ----------- | ----------- | ----------- |
| `true`      | `true`      | `true`      |
| `true`      | `false`     | `false`     |
| `true`      | *undefined* | *undefined* |
| `false`     | `true`      | `false`     |
| `false`     | `false`     | `false`     |
| `false`     | *undefined* | `false`     |
| *undefined* | `true`      | *undefined* |
| *undefined* | `false`     | *undefined* |
| *undefined* | *undefined* | *undefined* |

#### `&&&` mutually guarded conjunction

Mutually guarded conjunction: if either operand is `false`, the result is `false` and any undefinedness on the other side is caught. If both are `true`, the result is `true`. Otherwise the result is *undefined*.

| left        | right       | result      |
| ----------- | ----------- | ----------- |
| `true`      | `true`      | `true`      |
| `true`      | `false`     | `false`     |
| `true`      | *undefined* | *undefined* |
| `false`     | `true`      | `false`     |
| `false`     | `false`     | `false`     |
| `false`     | *undefined* | `false`     |
| *undefined* | `true`      | *undefined* |
| *undefined* | `false`     | `false`     |
| *undefined* | *undefined* | *undefined* |

#### `|` strict disjunction

Strict (non-short-circuit) disjunction evaluates both operands; if either side is undefined the result is *undefined*.

| left        | right       | result      |
| ----------- | ----------- | ----------- |
| `true`      | `true`      | `true`      |
| `true`      | `false`     | `true`      |
| `true`      | *undefined* | *undefined* |
| `false`     | `true`      | `true`      |
| `false`     | `false`     | `false`     |
| `false`     | *undefined* | *undefined* |
| *undefined* | `true`      | *undefined* |
| *undefined* | `false`     | *undefined* |
| *undefined* | *undefined* | *undefined* |

#### `||` guarded disjunction

If the left operand is `true`, the value of the right operand does not matter and any undefinedness from evaluating the right operand is caught. If the left operand is `false`, the result equals the right operand. If the left operand is *undefined*, the result is *undefined*.

| left        | right       | result      |
| ----------- | ----------- | ----------- |
| `true`      | `true`      | `true`      |
| `true`      | `false`     | `true`      |
| `true`      | *undefined* | `true`      |
| `false`     | `true`      | `true`      |
| `false`     | `false`     | `false`     |
| `false`     | *undefined* | *undefined* |
| *undefined* | `true`      | *undefined* |
| *undefined* | `false`     | *undefined* |
| *undefined* | *undefined* | *undefined* |

#### `|||` mutually guarded disjunction

Mutually guarded disjunction: if either operand is `true`, the result is `true` and any undefinedness on the other side is caught. If both are `false`, the result is `false`. Otherwise the result is *undefined*.

| left        | right       | result      |
| ----------- | ----------- | ----------- |
| `true`      | `true`      | `true`      |
| `true`      | `false`     | `true`      |
| `true`      | *undefined* | `true`      |
| `false`     | `true`      | `true`      |
| `false`     | `false`     | `false`     |
| `false`     | *undefined* | *undefined* |
| *undefined* | `true`      | `true`      |
| *undefined* | `false`     | *undefined* |
| *undefined* | *undefined* | *undefined* |

#### `!` not

## Membership operators

#### `:` "is member of"

#### `!:` "is not member of"

#### `::` "is collection of"

#### `:::` "is sequence of"

## Operator precedence

## Projection operators

#### The `.` property projection operator

The operator is a binary operator. On the left it accepts an expression which has *properties*. On the right it accepts an expression that declares identifiers.

In referential context the RHS defines which properties are selected from the LHS.

    customer.Orders
    
    customer.(Orders)

    customer.(Orders<=AllOrders)

##### `.` in declarative position

In declarative position the RHS *declares* properties on the LHS value.

    public customer.CompletedOrdersTotal = customer.Orders.Where(.Completed).Total

    public customer.Orders.CompletedTotal = customer.Orders.Where(.Completed).Total

    


##### `.` as prefix operator

As with all binary operators you can use the `.` operator in *prefix position*.

The following expression is a function which accepts an object which has an `Amount` property and returns the value of that property.

    .Amount


The following  expression is a function which accepts an object which has a property `Amount` which again must be a member of the `decimal` set. It returns the value of the `Amount` property, which is guaranteed to be a member of the `decimal` set.

    .(decimal Amount)

The ability to easily create a function to a property is handy when for instance you want to calculate the sum over a property of a list or set:

    ordersTotal = customer.Orders.Sum(.Total)

Note how `.Total` is used instead of `x=>x.Total` or similar.


The following expression is a function which accepts an object which has a property `Orders` which again is a list of `AllOrders` elements. The function returns the sum of the `Amount` property over all of the `Orders` elements.

    x?.(Orders:AllOrders.Lists) => x.Orders.Sum(.Amount)

Here the `.` operator is used in prefix position twice:
1. `.(Orders:AllOrders.Lists)`: This is the RHS of `?`. It is a function that restricts the LHS of `?` to objects that has an `Orders` property which must be a list of `AllOrders` elements.
2. `.Amount`: This is a function which selects the `Amount` property from the argument.

### The `..` element property projection operator

The `..` operator selects a property from the *elements/items* of a set or sequence. It returns a *function* from an element/item of the set/sequence to the property of that element/item.

Given a set `Customers` in which each element has a property `FirstName` these two expressions are equivalent:
1. `Customers c => c.FirstName`
2. `Customers..FirstName`

##### `..` in declarative context

In declarative context the `..` operator *declares* a property on each element/item of the LHS argument.

The following example declares a `FullName` on each element of the `Customers` set where the FullName is calculated as `FirstName`, `MiddleName` and `LastName` concatenated with `" "` as separator, i.e. *string joined*.

    public Customers..FullName = c => string.Join(" ",[c.FirstName,c.MiddleName,c.LastName])

With the above declaration, any object known to be a member of `Customers` now has a `FullName` property:

    let c:Customers in c.FullName


##### `..` as prefix operator

As with all binary operators you can use the `..` operator in *prefix position*.

The following expression is a function which accepts a set or sequence whose elements have a `FullName` property and returns a projection of that property from each element.

The following expression is a function which accepts a set or sequence whose elements have a property `FullName` which is a member of the `string` set. It returns a projection of the `FullName` property from each element.

    ..(string FullName)

## Property operators

### The `.` property selector

A property selector expression takes the form `whole.part` where 
- `whole` is some expression which has properties
- `part` is a *declarative* expression (often just an identifier)

Note how - unlike many other object-oriented languages - the `part` need not be an identifier but may be an *expression* with identifiers.

The operator can be used in both referential and declarative context. 

The right `part` operand is always declarative. The identifiers that it declares are properties of the `whole` left operand.

In referential context the operator is used to *reference* the properties of the left operand.

In declarative context the operator is used to *declare* the properties on the left operand.

When used in prefix position - like any other binary operator - it evaluates to a function which accepts the left operand, essentially `whole => whole.part`.

A expression such as `.Name` is thus a function which assumes that the argument is some structured value with a `Name` property. It returns the value of that property.

`.(string Name)` is a similar function, but which is only defined for structured values which has a `Name` property the value of which is a member of the `string` set.

`.(string Name, int Age)` is a function which is only defined for structured values which has a `Name` and an `Age` property. The value of `Name` mustbe a member of `string`, and the value of `Age` must be a member of `int`. The result is a *tuple* of those two values.

`.record(Name, Age)`

`.$"{string Name} (age {int Age})"`. When invoked with a record `record(Name="Tom Balivar", Age=32)` this function will return the string `Tom Balivar (age 32)`



### The `..` member property selector


The `some..prop` expression returns a function that maps from a member of the set `some` to the property of `prop` of that member.

The left operand must be a set. The right operand is always declarative. An identifier declared by the expression is a reference to a propertu of an element of the set. 

In referential context all of the members of the left operand set **must** define properties compatible with the identifier(s) declared by the right operand.

In declarative context all of the members of the left operand set are extended with properties corresponding to the identifiers declared by the right operand.

#### Examples

Given a set `persons` the following proposition will declare a new `Age` property on every member of `persons` 

    public persons..Age = this => DateTime.Today.FullYearSubtract BirthDate

## Range operators

### `...`, `<...`, `...<` and `<...<` "range" operators

A *range* is an expression which can assume any value within specified lower and upper bounds.

A range inherently ordered and nondeterministic. However, if the range provably only contains a single value, it is considered deterministic.

These infix binary operators accept a lower bound and an upper bound, respectively. The 4 operators differ in whether the bounds are included in or excluded from the range.

| Expression  | Equivalent to         |
| ----------- | --------------------- |
| `a ... b`   | `x \ x >= a & x <= b` |
| `a <... b`  | `x \ x > a & x <= b`  |
| `a ...< b`  | `x \ x >= a & x < b`  |
| `a <...< b` | `x \ x > a & x < b`   |


#### Ranges and set/list definitions

Ranges are frequently used with set- or list definitions, which will *unwind* the nondeterminism by backtracking.

    Digits = { '0'...'9' }          // set of every character from '0' through '9'
    InnerBand = { 0<...<10 }        // set of every integer from 1 through 9
    Specials = { -1, 0...3, 42 }    // mix constants with a range in one definition

In each case the set contains one entry per value the range can assume; combining deterministic members with a range literal produces a single unified set.

    OneDigitNumbers = [ 0...9 ]     // list containing numbers from 0 through 9
    InteriorList = [ 0<...<10 ]     // list containing only interior values 1 through 9
        
List literals evaluate the range left-to-right, emitting every value the range can produce while preserving order.

## Restriction operators

#### `?`

#### `\` "such that"

#### `??`

## Scope operators

### The `./` *property scope* operator




### The `../` *member property scope* operator

## Such-that operator

### The `\` "such that" operator

The `\` operator is *declarative* and *right-referential* meaning that it introduces a new local scope which is declarative in its left operand and referential in its right operand.

A *such-that* expression evaluates to the value of the left operator, provided that the right operator is satisfied (evaluates to `true`) for the value.

    int x \ x != 0      // A value that is not equal to 0

The operator is frequently used in combination with set- and list definitions.

    { int x \ x != 0 } 

This example reads as "the set of `int` members such that the `int` member is greater than zero".