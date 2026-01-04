## Lists



### List definitions

A *list definition* is similar to a set definition, but instead of producing a *set* it produces a *list* of items.

A list definition *unwinds* [ordered nondeterminism](<Determinism and nondeterminism.md#ordered-nondeterminism>) of any of the expressions in-place. The list definition `[ 1...10 ]` is thus the list of integers from 1 to 10 in that order.

### Examples


This is the list of Fibonacci numbers:

    [ (0,1) |> let f ~ (a,b) => a ; f(b;a+b) ]

How this works:
- `[` ... `]` :
- `(0,1) |>` *...* : 
- `let f ~ ` ... :
- `(a,b) => a ; f(b;a+b)` :

