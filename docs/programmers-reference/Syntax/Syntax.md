## Syntax

A Ting program or module is an expression. 

An expression can be

- a function application, such as `Math.Sin 0.7`. 
- an infix binary operator expression, such as `a + b`
- a prefix operator expression, such as `!IsVisible`
- a literal such as `"Zaphod Beeblebrox"`, `42` or `667_776.65f[m/s^2]`. 
- a parenthesis grouping 
- a set definition
- a list literal

Literals are covered in [*Literals*](Literals.md)

### Tokens

Where more than one token could match, only the longest matching token matches. This means that `a<=b` is broken into tokens `a`, `<=` and `b`, not `a`, `<`, `=` and `b`.

### Function application

### Infix binary operators

### Prefix and postfix unary operators

### Literals

### Parenthesis grouping
