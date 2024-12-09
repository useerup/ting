## Sets

Thus, sets are *types*; not data structures.


### Sets as functions

When used as the function in a function application, a set acts as its own *identity function*. The identity function of a set is the function which accepts any member of the set and returns that same member.

The most common usage of this is in declarative expressions where a set as its identity function can be used to constrain argument declaration.

    int x

This expression is only defined for values of `x` that belongs to `int`.

Beyond function application the following operators will also automatically convert a set to its identity function:

- `>>` and `<<`: If either operand is a set, it is converted into its identity function.
- `|>`: If the right operand is a set it is converted into its identity function.
- `<|`: If the left operand is a set it is converted into its identity function.

You can also explicitly convert the set to the identity function by using the set method `IdentityFunc`. This is what the function operators do internally:

    int.IdentityFunc x      // the same as int x


### Sets of ordered pairs

If a set is a set of *ordered pairs* it can be converted into the corresponding function by invoking `AsFunc` method.

    { 0 -> 1, 1 -> 1, var int n ? >=2 -> this(n-2)+this(n-1) }.AsFunc

This could also be written using the function literal:

    {^ 0 -> 1, 1 -> 1, var int n?>=2 -> this(n-2)+this(n-1) ^}

Any function can be converted into a set of ordered pairs:

    float.AsSet

The `AsSet` method returns the internal set of ordered pairs that defines the function.

    x : int
    y : int.A
