### Functional operators

#### `=>` "lambda" function

#### `->` "ordered pair" operator

An ordered pair is constructed with the infix operator `->`. It produces an ordered pair whose left component is the LHS and whose right component is the RHS.

#### `**` "fusion" operator

#### `-->` "set of functions" operator

`-->` creates a set of functions from LHS set to RHS set. Both LHS and RHS must be sets.

Example:

    int-->string    // the set of functions from `int` to `string`

#### `>>` and `<<` "pipeline" operators

The pipeline operators `>>` and `<<` are used to compose functions, passing the output of one function as the input to the next.

- `f >> g` creates a new function that applies `f` to its argument, then applies `g` to the result.
- `g << f` is equivalent to `f >> g`, composing `f` and `g` in the same order.

Example:

```ting
let add1 x = x + 1
let double x = x * 2

let addThenDouble = add1 >> double
addThenDouble(3) // evaluates to 8
```

These operators help build complex function pipelines in a readable, declarative style.

A common idiom is to compose a function with a set. When used as a function, a set acts as its own identity function. This can be used to restrict a function to be defined only for a subset of its arguments. 

Consider for example a function `ExpectChar` which "parses" the first character of a string: `ExpectChar = char c => [c,,rest] => rest`. We can define the set of digits as `Digits = {'0'...'9'}`. We can now define a function which will only parse diggits: `ExpectDigit = Digits >> ExpectChar`.


#### `|>` and `<|` "function application" operators




#### Automatic set-to-function conversion

When a set is used in a position where a function is expected, it is converted to its own *identity function*. An identity function of a set is a function which accepts members of the set and returns the same member.

    (+)