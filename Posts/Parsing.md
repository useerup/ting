In Ting a function is defined only for the argument values that will result in a successful calculation, unless the function is marked `partial`.

The `==>` is the *implication* operator. For `bool` the expression `a ==> b` is equivalent to `!a || b. For *functions*` `f ==> g` means that `g` must be defined for all arguments of `f` *and* `g x` must be unifiable to `f x`. The operator is often used where `f` is an ambigous function, e.g. (int**int) f.

    a ==> b alt c

A *parser function* is a function which accepts a `Sources` and returns

    ParserFuncs = { Sources(s,pos) -> Sources(s,_?>=pos) }

A parser function is a function which accepts a source text and returns a source text. The parser function must be defined only for the set of source texts it can parse.

A function which accepts a character and returns a function which parses said character:

    Char = Chars c => Source([c,,tail],pos) -> tail

    (int*int)**int ==> 

If we invoke this function with an ambigous value, such as an unrestricted value, the returned parser will parse any character, but will bind the parsed character to the argument.

We can use thus

By convention it should remove the parsed text and return the remaining tail part of the input source text.

    Parser = { Source s -> s.Right _ }

`RightAssoc` is a curried function which accepts 1) a binary operator parser and 2) a sub-expression parser.

A binary operator parser is a parser which parses an operator and returns the corresponding binary function. A binary function is a function which accepts a 2-tuple argument. For instance, the parser for `+` will return a function which will add the two items of the 2-tuple.

In Ting this would look like

    `+` = Symbol "+" <- 

|                       |    |
| ----------------------- | ---- |
| Identical             | =  |
| Less than or equal to | <= |
| Equals                | =  |
| Equals                | =  |
| Equals                | =  |
| Equals                | =  |
