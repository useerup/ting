# Suprising features of Ting


## You can declare `this`

In many OO languages, `this` refers to the current instance of a class. In Ting `this` likewise refers to the ambient instance scope.

So what happens when `ting` appears in a *declarative position*? You bind the ambient scope:

    DateTimeOffset..FormatRoundtrip = this => $"{Year:D4}-{Month:D2}-{Day:D2}T{Hour:D2}-{Minute:D2}-{Second:D2}"

In this example `this` is bound to the member of `DateTimeOffset` being formatted.


The operators `./` and `../` explicitly binds `this` to the left operand in the right operand.