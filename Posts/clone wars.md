

## Operators

### Operator `./`: Scoped Member Access

The `./` operator allows developers to reference the members of an object or expression directly. Acting as a binary infix operator, it evaluates the right-hand expression (rhs) within the scope of the left-hand operand (lhs).

The lhs operand provides the scope for evaluating rhs. All accessible members of lhs can be referenced directly in rhs without qualification. Identifiers of lhs shadows any identifier already in scope with the same name.

Considerations
- `./` enhances readability by reducing redundant object references.
- Care should be taken to avoid ambiguity when dealing with nested scopes.


### Operator `+`: Addition / concatenation

Defined for
- byte*byte=>byte
- sbyte*sbyte=>sbyte
- int*int=>int
- uint*uint=>uint
- long*long=>long
- ulong*ulong=>ulong
- float*float=>float
- double*double=>double
- decimal*decimal=>decimal
- list*list=>list
- set*set=>set

### Operator `-`: Addition / concatenation

### Operator `*`: Addition / concatenation

### Operator `+`: Addition / concatenation

### Operator `/`: Addition / concatenation

### Operator `%`: Addition / concatenation

