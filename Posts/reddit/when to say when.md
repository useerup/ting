I did away with the switch/when statements when I realized that the way I create functions easily lend itself to a very similar construct.

    flip_coin |> func {
        true;false -> "true or false"
        _ -> "wat"
    }

Explanations:
- `|>` invokes a function
- `{`...`}` constructs a *set*. If the first item is on a new line, then all items are assumes to be on separate lines.
- `->` constructs an `ordered pair`. An ordered pair is like a tuple, but whose identity of the pair is the argument (left) component.
- `func` converts a set of ordered pairs so a function.
- `;` creates a *choice* (term stolen from the language Verse). A choice is non-deterministic in that the value of a choice can be any of the values. The set constructor `{`...`}` unwinds non-determinism, so the set actually contains both values.
- `_` is any value. In this case (I hope I can make) the compiler generate a compilation error, as it should be able to prove that it cannot be reached.

Sets (and by extension functions being bases on sets of ordered pairs) are not data structures in my language, rather they are *types*. Like in math, a set can only contain unique values. In my language, this uniqueness is by identity. If a set is constructed with a list of items with the same identity, only the first such identical item is a member of the set. This is important for functions, as the identity of ordered pairs is that of the argument component of the ordered pair.

So in essense, the switch/when is just a function application.


