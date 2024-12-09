Hello fellow language designers!

For the language I am designing, I plan on *dogfooding* my compiler

With the language syntax and semantics, I am also considering a minimal "core" language and then bootstrap the compiler from there. There is no compiler yet; this is still just a mental exercise. But mental exercises are fun, right?

So, as an exercise, I have "implemented" an interpreter/evaluator for a another "minimal language": A limited math expression, which should be familiar to everyone.

This limited language features the four basic arithmetic operators as well as the power operator. It also allows parenthesis to group expression to override precedence. This should cover topics such as precedence, associativity end recursive syntax.

However, having worked on this project for a looong time, a have probably developed some blind spots in terms of language ergonomics. So my question to you is:

**Is this understandable?**

Obviously, you don't know my language, but given that you know the task, does the "program" below intuitively make sense?

    // The main expression.
    Exp = 
        LeftAssoc ( `+` | `-` )
            LeftAssoc ( `*` | `/` | `%` )
                RightAssoc ( `^` )
                    Prefix 0 ( `-` | `+` )
                        Literal | ParensExp

    // Rules for each operator
    `+` = Symbol "+"  <-  (Decimals a, Decimals b) -> a+b
    `-` = Symbol "-"  <-  (Decimals a, Decimals b) -> a-b
    `*` = Symbol "*"  <-  (Decimals a, Decimals b) -> a*b
    `/` = Symbol "/"  <-  (Decimals a, Decimals b) -> a/b
    `^` = Symbol "^"  <-  (Decimals a, Decimals b) -> a^b

    // A numeric literal (decimal number)
    Literal = Space >> {
        OneOrMore Digit i >> Char '.' >> ZeroOrMore Digit f  <--  Decimals.Parse (i+"."+f)
        OneOrMore Digit i  <--  decimal.Parse i
    }

    // A parenthesis delimited recursive expression
    ParensExp = Symbol "(" >> Exp exp >> Symbol ")"  <-  exp

    // Rule which skips space and matches a character symbol
    Symbol = Space >> CharSymbol (CharSymbols sym)  <-  sym

    // The set of strings that make up character symbols
    CharSymbols = { "+", "-", "*", "/", "^", "(", ")" }


---
**Compiler combinators**

For completeness, I show here the implementation of the combinators used above

    Space = Char.IsWhiteSpace >> Char >> Space || Strings

    Char = string [c,,rest] -> rest  <-  c

    Digit = {'0'...'9'} >> Char

    // Matches a rule one or more time and assembling a list
    OneOrMore = Rules rule -> {
        rule exp >> these exps  <--  [exp,,exps]
        rule exp  <--  [exp]
    }

    LeftAssoc = BinaryOperators operator -> Rules next -> {
        these left >> operator f >> next right  <-  f(left,right)
        next exp  <-  exp
    }

    LeftAssoc = BinaryOperators operator -> Rules next -> {
        next left >> operator f >> these right  <-  f(left,right)
        next exp  <-  exp
    }

    Prefix = id -> BinaryOperators operator -> Rule next -> {
        operator op >> these exp  <--  op(id, exp)
        next exp  <--  exp
    }

    BinaryOperators = Parsers ^ (Decimals^2 => Decimals)

    Parsers = Strings=>Strings 



