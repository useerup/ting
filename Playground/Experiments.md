# Ting Language

    CharSymbols = ...

    WordSymbols = ...

    WordBeginChars = UniCode.Letters

    WordFollowChars = WordBeginChars | { '0'...'9' }

    Symbol = 
        CharSymbols.OrderByDescending(.Length).Map(sym => ParseString(sym,rest) => rest).FoldLeft (||) 
        || WordSymbols.OrderBy(._).FoldLeft (||) (sym => ({sym}>>ParseWord) => rest)

    ParseWord = string[h,,t] => WordBeginChar h >> ZeroOrMore WordFollowChar t 

    ParseString = string s => s + rest => rest

    WordBeginChar = WordBeginChars >> ParseChar
    
    WordFollowChar = WordBeginChars >> ParseChar

    ZeroOrMore = rule => f ~ ([h,,t] => rule h >> f t) || Not rule 
    
    Not = Rule (_=>dom=>_) => Func (!{dom})

`Not` is a function which accepts an *argument function* and returns a *result function*. The argument function is unified with the function ´_=>dom=>_´, effectively binding `dom` *non-deterministically* to the domain of the result function. So the argument function must be a function which returns a function. This fits a `Rule` function (a function which is a member of the `Rule` set of functions). A Rule function accepts a product and returns a parser for that product. A parser is itself a function which accepts a source and returns a source, which is usually the tail end of the argument source.


## Functions

Because Ting is a logic programming language, what we call *functions* are actually more akin to *relations* in mathematics. When applied, a Ting function establishes a relationship between the input arguments and the result of the application.

## Operators

### Declarative operators

A number of Ting operators are *declarative*. Declarative operators declare one or more of their operands.

### Operator precedence

1. `^`
2. `-` (unary minus)
3. `*` `/` `%`
4. `+` `-`
5. `;` `;;`
6. `<` `<=` `>=` `>` `==` `!=`
7. `!`
8. `&` `&&`
9. `|` `||`
10. `=>`
11. `>>` `<<` 
12. `~`
13. `|>` `<|`
14. `=`

### `=>` "lambda" operator

The `=>` lambda operator defines a function. 

The operator creates a local scope where the left operand is declarative in that scope and the right operand is referential. In the expression `x => x * 2` the left operand `x` is declared and is referenced in the right operand expression `x * 2`.

If the operator expression itself appers in a declarative scope, that declarative scope is propagated to both operands. If the expression `x => x * y` appears in a declarate scope, then *both* `x` and `y` is declared in that scope.

### `\` "such that" operator

The `\` operator restricts its left operand to those values where the right operand evaluates to true. The expression `x\x:int` is a value that is restricted to be members of the `int` set.

The operator creates a local scope where the left operand is declarative in that scope and the right operand is referential. 

If the operator expression itself appers in a declarative scope, that declarative scope is propagated to both operands. If the expression `x \ x == y` appears in a declarate scope, then *both* `x` and `y` is declared in that scope.

### `~` "alias" operator

The value of a `~` operator expression is that of the left and right operands unified.

The operator creates a local scope spanning the operator expression. The scope is declarative in the left operand and referential in the right operand.

The operator is commonly used for introducing a local alias for a function so that it can call itself recursively.

The following example is an anonympus factorial function:

    f ~ int n?>=0 => n==0 then 1 else n*f(n-1) 

The following example is the set of Fibonacci numbers:

    { (0,1) |> f ~ (a,b) => (c=a+b in c;f(b,c) )  }

The function `(a,b) => (c=a+b in c;f(b,c) )` is aliased to `f` so that it can be referenced recursively.

    


## `?` "where" operator

If an `?` expression  appears in a declarative scope, that scope is converted to a referential scope through its right operand.

## `;` "choice" operator

The `;` operator returns a non-deterministic *choice* between the left and the right operand.

