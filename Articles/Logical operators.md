

The defintion for `|`:

    // `|` is a function which parses the symbol "||" and returns the `Or` function
    `|` = Symbol "|" \ Or 

    // `Or` is (at this level) just an alias for `bool.Or`
    Or virtual = bool.Or 

    // `bool.Or` is a completely evaluated form of the intrinsic `Or`
    bool.Or = Expression bool a \ Expression bool b \ Valuation v \ 
        Core.Or( v.Value a & v.IsDef b, v.Value b & v.IsDef a )


Here `Symbol` is a parser combinator which parses the token that is passed to it. 

The arguments to `|` (the operands) may or may not be defined under the actual valuation. I don't want the language to behave in a nondeterministic manner by depending on whether it evaluates the left or the right operand first. Thus `bool.Or` (and by extension the operator `|`) is *only* defined if *both* operands are defined. If any of


### Appendix: Definition of `Symbol`

    Symbol = 
        CharSymbol | WordSymbol

    CharSymbol = 
        CharSymbols
        .OrderByDescending(.Length)
        .Map(sym \ sym \ Source sym++rest \ rest)
        .FoldLeft(||)

    CharSymbols = {
        "->"
        "<-"
        "<->"
        ...
    }

    WordSymbol = 
        WordSymbols >> Word

    WordSymbols = class {
        "class"
        "in"
        ...
    }

    Word = WordBeginChar head >> ZeroOrMore WordFollowChar tail  \  head :+ tail


While `Symbol` accepts a string constant like this, it is only defined for the subset of `string` that is the tokens of the language.
    

    ZeroOrMore = Rule rule \ {
        Not (rule _)  <-  (,,)
        rule head >> ZeroOrMore rule tail  <-  (head,,tail)
    }

    OneOrMore = Rule rule \ {
        rule head >> ZeroOrMore rule tail  <-  (head,,tail)
    } 

