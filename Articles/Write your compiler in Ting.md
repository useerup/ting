



# Parse single a character

We use the following `Char` function to parse individual characters. 

    Char = Chars c -> [c,,rest] -> rest

What this function does is that it accepts a character and returns a function. The returned function which will accept any string which starts with that character and returns the string *without* that character. We call functions which accepts a string that starts with some specific character or characters and returns the remainder of that string for *parser function*.

    Parsers = string => string

So `Char` is really a function which accepts a character and returns a parser for that specific character.

How is this useful for parsing? Here we exploit the fact that in Ting - being a logical programming language - functions are *relations* between members of its domain and members of its range. So while Char is a function which accept a character

In our first grammar, characters are the tokens, so we will not need *tokenization* beyond reading single characters.

`Strings` is the set of all strings. A string is a list of characters, so `Strings` is the set of *all lists of characters*. Obviously, this is an infinite set, but that doesn't concern us, as we are not going to enumerate or count the members.

[c,,rest] deconstructs a list, isolating the head `c` from the tail `rest`. 

Char is a curried function which accepts a character `c` and returns a function which accepts a string (a member of `Strings` which is the set of *all lists of of characters*) which *starts with* with `c`. This latter function returns the remaining string after `c`; the `rest` above.

This is really the core of parsing: Identify a matching character, remove it and continue with the remains of the string.

# Parsing multiple characters

# Combinators

    Source = String
    Parser = Source => Source
    Rule = Any => Parser

    Not = Parser p -> Source s ? ~: p.Domain -> s

We need a special rule which can apply another rule zero or more times:

    ZeroOrMore = Rule rule -> {
        var rule h >> Cut >> these t <-- [h,,t] 
        Not(rule _)  <--  [] 
    }


We also need a rule for one or more occurrences of a rule:

    OneOrMore = Rule rule -> rule h >> ZeroOrMore rule t <- [h,,t]



## Word tokens
    
    WordToken = (WordBeginChars>>Char) h >> ZeroOrMore (WordFollowChars>>Char) t <- [h,,t] 
    where
        WordBeginChars = Unicode./(LowercaseLetter|UppercaseLetter|TitlecaseLetter|ModifierLetter)|{'_'}
        WordFollowChars = WordBeginChars | Unicode.DecimalNumber

# Word symbols and identifiers

    WordSymbols = {"let","in","where", ... }
    



## Character tokens

Character symbols may span multiple characters. One symbol may actually start with characters of another symbol, like e.g. `<=` and `<`. To avoid ambiguities, we 

    let CharSymbol = CharSymbols.OrderByDescending(.Length).Map(sym->sym+rest->rest).Fold(||)
    Symbol = 








Parse symbols
    
    let 
        CharSymbol = { string(CharSymbols.OrderDescending(.Length) sym + rest)  -->  sym }
        WordSymbol = WordSymbols >> Word
    in
        Symbol = CharSymbol | WordSymbol

