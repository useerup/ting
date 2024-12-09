## Identifiers





### Adopting identifiers

The `adopt` keyword can be used to hoist identifiers into the current scope.

Example:

    Player = adopt {| White, Black |}

Normally, a discriminated union defines properties on the union which represents each case; it
does not define the identifiers in the enclosing scope. This means that to reference the 
white player we would need to write `Player.White`. By *adopting* the union the properties of `Player`
becomes available as aliases in the current scope.
