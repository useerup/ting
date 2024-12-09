# Parsers and productions


## Grammar notation

For *sets* the `+` operator denotes the most general function which maps from the left operand (the *domain* of the function) to the right operand (the *codomain* of the function). This most general function maps any value of the domain to any value of the codomain. A function may be *nondeterministic* which means that a single member of the domain may map to multiple members of the codomain.

The `{}` set constructor constructs a set by enumerating the expressions member expressions. If an expression is nondeterministic, i.e. the value is one of a set of possible values, this nondeterminism is unrolled and the set will contain all of the values.

In declarative context the left operand of the operator `=` is declarative and the right operand is referential.



## Source structure


A string is a sequence of Unicode characters. The set of all strings is the set of all possible lists of characters.

    Strings = Chars.Lists

`Chars` denotes the built-in set of all Unicode characters. Every set has a property `Lists` which is the set of all lists of members of the set.

The set of all possible source texts is (for now) the set of all possible strings

    Sources = Strings


## Parsers and productions

A *parser* is a function which accepts a source and returns the remaining (unparsed) source. The set of parsers are:

    Parsers = { Sources + Sources }


A *production* is a function which accepts a *product* and returns a *parser* for that product. The set of all production functions are the set of all functions that map a product (which can be anything) to a parser:

    Productions = { Any + Parsers }


## Basic productions

The `Char` production is used to parse a single character. The rule accepts a single parameter which is the character to match. The rule matches any source that begins with the character and returns the remaining source. If no constraint is put on the character to match, the resulting parser will match any character:

    Char = Chars c \ Sources (c,,s) \ s
    String = Strings s \ Sources (s+rest) \ rest

    Underscore = Char '_'
    Quote = Char '"'
    SingleQuote = Char '\''





## Production combinators

The `OneOrMore` production combinator accepts an *item production* and returns a *combined production* which accepts a list of products and is defined for sources with one or more occurrences of the production and w

    OneOrMore = Productions prod \ {
        prod h >> self t  <-  (h,,t)
        prod h  <-  (h,,)
    }

The `ZeroOrMore` production combinator accepts a production as its first parameter and the product as its second parameter. `OneOrMore (Char _) s`

    ZeroOrMore = Productions prod \ {
        prod h >> self t  <-  (h,,t) 
        Sources  <-  ()
    }

The `Optional` production combinator accepts an *item production* and returns a *combined production* which will match zero or one occurence of the *item production*.

    Optional = Production prod \ {
        prod h  <-  h
        Sources  <-  ()
    }


## Words 

    WordBeginChars = Unicode.UpperCaseLetters | Unicode.LowerCaseLetters | {'_'}
    WordFollowChars = WordBeginChars | Digits

    WordBeginChar = WordBeginChars c \ Char c
    WordFollowChar = WordFollowChars c \ Char c

    Word = { WordBeginChar h >> ZeroOrMore WordFollowChar t  <-  (h,,t) }



## Text literals




## Literals

    
    NumericLiteral = {
        NumericToken digits >> NumericTypeSuffix type  <-  type.Parse digits
    }

    NumericTypeSuffix = {
        NoSpace void >> Word suf  <-  suf;{"f"->Singles, "d"->Doubles, "m"->Decimals, "ul"->UnsignedLongs, "l"->Longs, "u"->UnsignedInts, "s"->Shorts, "us"->UnsignedShorts}
        Not (Nospace void >> Word _)  <-  Ints||Longs||Singles||Doubles
    }

    NumericToken = { 
        DigitSequenceGrouped i >> Period void >> DigitSequence f  <-  i+"."+f 
        DigitSequenceGrouped i >> Not Period void  <-  i
    }

    DigitSequenceGrouped = {
        Digit head >> ZeroOrMore Underscore void >> self tail  <-  (head,,tail)
        Digit head >> Not Underscore void | Not Digit _  <-  (head,,)
    }



    CharLiteral = { 
        SingleQuote void >> EscapeCharacter c >> SingleQuote void  <-  c 
        At void >> SingleQuote void >> VerbatimCharacter c >> SingleQuote void  <-  c
    }

    StringLiteral = { 
        DoubleQuote void >> ZeroOrMore EscapeCharacter s >> DoubleQuote void  <-  s 
        At void >> DoubleQuote void >> ZeroOrMore VerbatimChar s >> DoubleQuote void (.) <-  s
    }


    CharacterLiteral = EscapeCharacterLiteral | VerbatimCharacterLiteral

    EscapeCharacterLiteral = {
        SingleQuote void >> Not Escape void >> 

    StringLiteral = EscapeStringLiteral | VerbatimStringLiteral



When a production is parameterized

    TwoDigits = { Digit d1 >> Digit d2  <-  (d1,d2) }



A *parser* function accepts a source text and returns a source text. The *set of parsers* is thus:

        Parsers = { Source + Source }

A given parser function is defined only for the source text that begins with the construct of the parser.

A *production* function accepts a product and returns a parser for that product:

        Productions = { Any + Parsers }

A word token begins with a 

        WordToken = { 
            WhiteSpace _
            >> Character WordBeginChars h
            >> ZeroOrMore (Character WordFollowChars) t
            <-
            (h,,t)
        }


A word symbol is a word token which *is* in the list of reserved word symbols

        WordSymbol = ReservedWords w \ WordToken w 

        ReservedWords = { "not", "foldl", "foldr", "map" }
    
A character symbol is the longest matching sequence of characters from the set of reserved character symbols. 

We order the character symbols by descending length, generate a production (maps the symbol to a parser for the symbol) for each symbol. All of these productions are then combined into one production through guarded disjunction. This means that a matching parser will shadow every following parser and prevent backtracking, even if more parsers could match the source.

        CharSymbol = WhiteSpace _ >> 
            CharTokens
            .OrderBy \(-.Length)
            .Map(sym\sym\(sym+tail)\tail)
            .FoldLeft(||)

        CharTokens = { ".", "..", "->", "<-", "<->" }

An identifier is a word token which is *not* in the list of reserved word symbols, i.e. it *must* be a member of the complement of word symbols

        Identifier = (!WordSymbols) w \ WordToken w
        
        
A symbol is a character symbol or a word symbol.

        Symbol = CharSymbol | WordSymbol




    StringLiteral = 
    { 
        Char '"' >> ZeroOrMore StringChar cs >> Char '"' <-  cs 
        Char '@' >> Char '"' >> ZeroOrMore VerbatimChar cs >> Char '"'  <-  cs
    }

    StringChar = {
        "\\t"+s \ s <- "\t"
        "\\n"+s \ s <- "\n"
        "\\r"+s \ s <- "\r"
        "\\\""+s \ s <- "\""
    }




An expression

        BinaryFunctions = Any*Any+Any

        BinaryOperators = { BinaryFunctions+Parsers }

        LeftAssoc = 
            BinaryOperators operator \ 
            Productions next \ 
            { 
                this left >> operator op >> next right  <-  op(left,right)
                this left >> operator op >> Not (next _)  <-  right\op(left,right)
                next exp >> Not (operator _)  <-  exp
            }

        RightAssoc = 
            BinaryOperators operator \ 
            Productions next \ 
            { 
                next left >> operator op >> this right  <-  op(left,right)
                next exp  <-  exp
            }

        NonAssoc = 
            BinaryOperators operator \ 
            Productions next \ 
            { 
                next left >> operator op >> next right  <-  op(left,right)
                next exp  <-  exp
            }

        Exp = 
            NonAssoc `_=_`
                LeftAssoc (`_|_` | `_||_` | `_|||_` | `_|!|_`)
                    LeftAssoc (`_&_` | `_&&_` | `_&&&_` )
                        NonAssoc RelationalOperators
                            LeftAssoc AdditiveOperators
                                LeftAssoc MultiplicativeOperators
                                    RightAssoc ExponentialOperator

        `_=_` = Symbol "=" \ (l,r) \ Intrinsic.Identical(l, reference r)

        `_|_` = Symbol "|" \ Intrinsic.BooleanOr


        