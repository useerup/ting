\> You might like \[Unified Condition Expressions) then! :-)

Actually, no.  Unifoid Condition Expressions are too complex for my taste. Something as simple as an *implication* should be *really* simple.

Here is my train of thoughts:

C-style `if` seems backwards. Because they chose to put `if` at the start, they needed to throw in `(` and `)` to delimit the condition from the conclusion.

The ternary operator `?` `:` has it *almost* right, IMHO.

So I am designing a language where I am going to use `then` to express an implication. So I can write

    a then b

In a logical context this means that if `a` is `true` then `b` must also be true (I am designing a logical PL). If `a` is `false` then no statement is made about `b`.

However, `then` is a *binary operator*. On the right hand side it accepts any type, and the type of the entire expression depends on that type.

`then` *also* accepts an *alternative expression* as the right operand, i.e. one which can be constructed using the `else` operator. This allows me to write

    a then b else c

Which with parenthesis is equivalent to

    a then (b else c)

Because `then` is overloaded to accept an alternative expression as the right operand, this is sematically *similar* to the `if-then-else` statement or ternary operator `?` and `:`.

So I *somewhat* keeps similarity with Pascal/Basic/... although I strip `if` because 1) it is superfluous and 2) it would make `then` and `else` parts of a larger, complex syntactic construct rather than simple binary operators.

Interestingly, this frees up the `if` to be used as a keyword. While I abhore Ruby's `y if x` syntax, maybe Matz was onto something afterall.

In my language I allow binary operators to be used in prefix or postfix positions as well. When used like that, they acts as curried operators, returning the underlying function partially applied. While `+` is an (infix) binary operator, it can be used prefix as well. So the these expressions are actually equivalent:

    a + b

    + b a

    (+ b) a

Now, if I define `if` like in Ruby, these are equivalent

    a if b

    if b a