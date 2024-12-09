# Operators
                

## Logical operators


### `|` Or

### `||` Half guarded or

### `|||` Fully guarded  or

### `&` And

### `&&` Half guarded and

### `&&&` Fully guarded and

### `!` Not



## Structural

### `->` Ordered pair

### `<-` Reverse ordered pair

### `=>` Lambda

### `#` Unify



## Relational operators

### `=` Equals

### `==` Equivalent

### `!=` Not equivalent to

### `<` Less than

### `<=` Less than or equal to

### `>` Greater than

### `>=` Greater than or equal to

### `:` Is member of

### `::` Is collection of

### `:::` Is list of

### `!:` Is not member of



## Compositional operators

### `|>` Invoke

### `<|` Apply

### `>>` Compose

### `<<` Reverse compose



## Arithmetic operators

### `+` Add

### `-` Subtract

### Unary `-`: Negation

### `*` Multiplication

String examples:

    "abc" * 3                   // "abcabcabc"

    "abc" * n = "abcabcabc"     // n=3


### `/` Division

String examples:

    "abcabcabc" / 3             // "abc"
    "abcabcabc" / "abc"         // 3
    "abcabcabc" / 1             // "abcabcabc"

    "abcabcabc" / "abe"         // undefined
    "abcabcabc" / 2             // undefined
    "abcabcabc" / 4             // undefined


### `%` Remainder

### `/%` Division with remainder

### `/!` Integer divide

### `^` Exponentation



## Projection operators

### `.` Property (suffix operator)

### `..` Member property (suffix operator)

### `.(` `)` 

### `..(` `)`



## Scope operators

### `./` Property scope

Example:

    price = orderItem ./ (UnitPrice * Quantity * (1 - Discount))

### `../` Member property scope

