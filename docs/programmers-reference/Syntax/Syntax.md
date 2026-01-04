## Syntax

A Ting program or module is an expression. 

An expression can be

- a function application, such as `Math.Sin 0.7`. 
- an infix binary operator
- a prefix operator expression, such as `!IsVisible`
- a postfix operator expression
- a parenthesis grouping 
- a literal such as `"Zaphod Beeblebrox"`, `42` or `667_776.65f[m/s^2]`. Literals are covered in [*Literals*](Literals.md)
- Folds


### Tokens

Where more than one token could match, only the longest matching token matches. This means that `a<=b` is broken into tokens `a`, `<=` and `b`, not `a`, `<`, `=` and `b`.

### Function application

### Infix binary operators

### Prefix and postfix unary operators

### Literals

### Parenthesis grouping

### Folds

Some special *syntax list operators* 

`---` creates a list from the following list of expressions

`,,,` creates a tuple from the following list of expressions

`>>>` folds the following list of expressions from left to right.

`<<<` folds the following list of expressions from right to left.

