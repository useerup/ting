# Grammar 


## Lists

A list is an ordered collection of objects. The same or identical objects can occur multiple times in a list.

`Lists` is the set of all lists.





    Lists..Items = { [x]+xs -> x; xs.Items }



## Characters and strings

`Chars` is the set of all unicode characters.

`Strings` is the set of all lists of unicode characters:

    Strings = Lists.Of Chars



## Parsers

`Parsers` is the set of parser functions. A parser function accepts a `Strings` member and returns a `Strings` member.

    Parsers = Strings ^^ Strings

## Rules

`Rules` is the set of rules. A *rule* is a function that accepts a product and returns a parser for that product. The product may be anything.

    Rules = Any ^^ Parsers


## Combinator rules

To build the grammar from basic rules, a number of *combinator rules* are used.

`OneOrMore` accepts a rule and returns a parser which will greedily match one or more applications of the rule.

    OneOrMore = Rules rule \ {
        rule head >> these tail  ;;  [head,,tail]
        rule item  ;;  [item]
    }


`ZeroOrMore` accepts a rule and returns a parser which will greedily match zero or more applications of the rule.

    ZeroOrMore = Rules rule \ {
        rule head >> these tail  ;;  [head,,tail]
        s \ s  ;;  []
    }


`Not` accepts a rule or a parser and returns an identity parser which is only defined for strings that *does not* match the rule or parser.

    Not = Parsers p \ Strings.Where(!:p.Domain)  ||  Rules r \ Not r.Codomain


## Token rules

`Char` is a rule which produces a parser for a single character.

    Char =  c \ Sources [c,,rest] \ rest 

`Word` is a function which accepts a word and returns a parser for that word. A *word* is a sequence of characters which begins with a character of the `WordBeginChars` set and is followed by zero or more characters of the `WordFollowChars` set.

    WordBeginChars = Unicode.Letters
    WordFollowChars = Unicode.Letters | Unicode.Digits | {'_'}
    Word = Whitespace >> (WordBeginChars>>Char) h >> ZeroOrMore (WordFollowChars>>Char) t  \\  [h,,t]

`WordSymbol` is a function which accepts a member of the `WordSymbols` set and returns a parser for that word symbol.
A word symbol is a word that is reserved as a symbol. A word which is *not* a member of the `WordSymbols` set is considered an *identifier*. `Identifier` is a function which accepts a word which *is not* a member of the `WordSymbols` set and returns a parser for that word. 

    WordSymbols = { "extend", "class", ... }
    WordSymbol = WordSymbols >> Word
    Identifier = !WordSymbols >> Word

`CharSymbol` is a function which matches the next sequence of characters against the set of character symbols `CharSymbols` by giving preference to the longest match.

    CharSymbol = Whitespace >> {  }

    CharSymbols = { ".", "..", "=", "+", "-", "*", "/", ---omitted--- }



`Symbol` is the union function of `CharSymbol` and `WordSymbol`. 

    Symbol = CharSymbol | WordSymbol


## Associativity combinators

`LeftAssoc`, `RightAssoc` and `NonAssoc` combinators are used to parse expressions in left-, right- and non-associative manner, respectively.

    LeftAssoc = BinaryOperators operator \ Rules next \ {
        these left >> operator op >> Cut >> next right  ;;  op(left,right)
        next
    }

    RightAssoc = BinaryOperators operator \ Rules next \ {
        next left >> operator op >> Cut >> these right  ;;  op(left,right)
        next
    }

    NonAssoc = BinaryOperators operator \ Rules next \ {
        next left >> operator op >> Cut >> next right  ;;  op(left,right)
        next >>
    }


## Prefix and suffix combinators



    Prefix = UnaryOperators operator \ Rules next \ {
        operator f >> This exp  ;;  f exp
        next
    }

    Postfix = UnaryOperators operator \ Rules next \ {
        This exp >> operator f  ;;  f exp
        next
    }

    Cut = Source=>Source


## Grammar rules

    Expression = 
        LeftAssoc `;;`
            RightAssoc `;`
                LeftAssoc `\\`
                    RightAssoc `\`
                        Prefix `ref` | `let` | `var`
                            LeftAssoc (`|` | `||` | `|||` )
                                LeftAssoc (`&` | `&&` | `&&&`)
                                    NonAssoc (`<=` | `<` | `==` | `>` | `>=` | `!=` | `:` | `::` | `:::` | `!:`)
                                        Prefix `!`
                                            LeftAssoc (`<<` | `>>` | `<|` | `|>`)
                                                LeftAssoc (`+` | `-`)
                                                    LeftAssoc (`*` | `/` | `%` | `/%` | `%%`   )
                                                        RightAssoc (`^` | `^^`)
                                                            Prefix `-`
                                                                Postfix ProjectionOperators
                                                                    LeftAssoc ApplicationOperators
                                                                        Term
    

    EqualOperator = Symbol "="  \\  Equals
    

    RelationalOperators = {
        Symbol "=="  ;;  EqualTo
        Symbol "!="  ;;  NotEqualTo
        Symbol "<"  ;;  LessThan
        Symbol "<="  ;;  LessThanOrEqualTo
        Symbol ">="  ;;  GreaterThanOrEqualTo
        Symbol ">"  ;;  GreaterThan
        Symbol ":"  ;;  MemberOf
        Symbol "::"  ;;  CollectionOf
        Symbol ":::"  ;;  SequenceOf
        Symbol "!:"  ;;  NotMemberOf
    }

    ImplicativeOperators = {
        Symbol "->"  ;;  Then
        Symbol "--"  ;;  Else
    }

    FunctionalOperators = {
        Symbol "|>"  ;;  Invoke
        Symbol "<|"  ;;  Apply
        Symbol ">>"  ;;  Compose
        Symbol "<<"  ;;  ReverseCompose
    }
    
    AdditiveOperators = {
        Symbol "+"  ;;  `+`
        Symbol "-"  ;;  `-`
    }

    MultiplicativeOperators = {
        Symbol "*"  ;;  `*`
        Symbol "/"  ;;  `/`
        Symbol "%"  ;;  Remainder
        Symbol "%%"  ;;  IntegerDivide
        Symbol "/%"  ;;  DivideWithRemainder
    }

    ExponentialOperators = {
        Symbol "^"  ;;  RaiseToPower
        Symbol "^^"  ;;  SwappedRaiseToPower
    }

    ProjectionOperators = {
        Symbol "." >> Identifier name  ;;  Property name
        Symbol ".." >> Identifier name  ;;  MemberProperty name
        Symbol ".(" >> ExpressionList exps >> Symbol ")"  ;;  ScopeTuple exps
        Symbol ".(." >> ExpressionList exps >> Symbol ".)"  ;;  ScopeRecord exps
        Symbol "..(" >> ExpressionList exps >> Symbol ")"  ;;  MemberScopeTuple exps
        Symbol "..(." >> ExpressionList exps >> Symbol ".)"  ;;  MemberScopeRecord exps
    }


    ApplicationOperators = Rule rule \ {
        Not (Space _)  ;;  Apply
        NonLineBreakingSpace _  ;;  Apply
        IndentedLineBreakSpace _  ;;  Apply
    }

    Property = string name \ Records whole \ Core.AccessProperty(whole,name)

    MemberProperty = string name \ Sets.Of Records whole \ Core.AccessMemberProperty(whole,name)

    Term = 
        Literal
        || Identifier
        || SetLiteral
        || ParenthesisExpression
        || Range


    SetLiteral = Symbol "{" >> MemberExpressions exps >> Symbol "}"  \\  { exps.Items } 

    ParenthesisExpression = {
        Symbol "(" >> ExpressionList list >> Symbol ")"  ;;  list
    }


    ExpressionList = {
        Expression head >> Symbol "," >> cut >> ExpressionList tail ;;  [head,,tail]
        Expression exp >> Not (Symbol ",")  ;;  list exp
    }

      
    ExpressionList = {
        Not (Expression _)
        Expression head >> Symbol "," >> ExpressionList tail  ;;  [head,,tail]
        Expression head >> Symbol ",," >> Expression tail  ;;  [head,,tail]
    }


    `+` = 


    
## Reference

A *type* is either a *set* or a *class*. A *set* is a *structural type*. A set includes all values that satisfy the set criteria. A *class* is a *nominal type*. A class includes only those values that have been constructed as a value of the class or one of its subclasses.

`class` denotes a function which accepts a set and returns a *class* of that set.

 `Lists.Of` denotes a function which accepts a type *T* and returns the set of all lists of *T*, i.e. lists where each item is constrained to be a member of *T*

 A *list literal* is a number of expressions inside `[` and `]`. Expressions delimited by `,` denotes. The last expression can optionally be delimited by `,,` and denotes a *tail list*. 

### The binary operator `*` 

When both operands are types, the operator returns a tuple of those types.

### The binary operator `\` and `\\`

`\` constructs a function. Left operand is the argument and right operand is the result. The argument is *declarative*, meaning that it declares its identifiers.

`\\` constructs an *inverse function*. `y \\ x` is the same as `(y \ x)^-1`

###

### The binary operator `^^` 

When both operands are types (sets or classes), the operator returns the set of all functions from the left operand to the right operand.

### The prefix operator `~`

Declares the identifiers of the operand expression.



