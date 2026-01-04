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

