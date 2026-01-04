# Building a parser

Let's build a parser for the Ting language in Ting.

Some languages use an exception mechanism to signal that a function or method has been called with invalid parameters. Java even has an entire exception hierarchy - checked exceptions - for dealing with those exceptions that represent some limitation of the program logic. Other languages use an error mechanism where the *return value* of a function/method may signal success or failure.

In the Ting language *definedness* is an important concept for functions. To that end, it is possible to restrict the allowable argument values through *refinements*. As an example the operator `/` (division) is defined for number types, but *excluding* the zero value. This means that it is an error to invoke it with a zero value.

Consider the function 

    f = float x => 1/x^2

Even though the argument is declared as `float x`, the compiler deduces that invoking the function is *not defined* for the argument value zero, because that would entail invoking `/` with zero, for which it is undefined.

When we are parsing text, we use this principle: A parser function is only defined for the sources that match the pattern being parsed by that function. A very simple parser function simply parses a single `.` character from a string could look like this:

    string (['.']+rest) => rest

The argument of this function is defined as `string(['.']+rest)`. This means that the argument must be string (a member of the `string` set) and that it must be the result of something prepended with the character `.`. In other words, the function is only defined for strings starting with `.`.

However, that is not a very useful parser. We usually have an idea *which* character(s) we expect. That is the essense of parsing.

    Sources = string

    Parsers = Sources-->Sources

    Parsers.Id = Parsers(s=>s)

    Rules = Any-->Parsers


So we introduce a `Char` *rule* which - given a character - returns a parser for that character:

    Char = char c => Sources([c]+rest) => rest


The following are some *parser combinators*:

    Optional = Rules rule => (||) >>>
        [h] => rule h
        [] => Func Sources

    OneOrMore = Rules rule => this ~~ (||) >>>
        [h]+t => rule h >> this t
        [h] => rule h 

    ZeroOrMore = Rules rule => this ~~ (||) >>>
        [h]+t => rule h >> rule t
        [] => Func Sources


We can write a parser which will match a *word*, defined as a sequence which begins with a character from the `WordBeginCharacters` set and is followed by any number of characters from the `WordFollowCharacters` set:


    // A parser for a word token
    WordBeginChars = Unicode.Letters | {'_'}
    WordFollowChars = WordBeginChars | Unicode.Digits
    Word = [head]+tail => (WordBeginChars>>Char) head >> ZeroOrMore (WordFollowChars>>Char) tail



