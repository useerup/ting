# Syntax Grammar

## Top-level
```
program  ::= ws expr EOF
```

## Terms (atoms)
```
term     ::= number
           | identifier
           | tuple
           | set-literal
           | list-literal
           | string-literal

number          ::= [0-9]+
identifier      ::= (letter | '_') (letter | digit | '_')*
tuple           ::= '(' (expr (',' expr)*)? ')'
set-literal     ::= '{' (expr (',' expr)*)? '}'
list-literal    ::= '[' (expr (',' expr)*)? ']'
string-literal  ::= '"' [^"]* '"'
```

## Expressions
```
expr ::= prefix-expr
       | expr infix-op expr
```

---

## Operator Precedence Table

Higher precedence = binds tighter. All operators are infix unless marked **prefix**.

| Prec | Operator(s) | Assoc | Notes |
|------|-------------|-------|-------|
| 1 | `let` `public` `private` `internal` `protected` | — | **Prefix** |
| 2 | `in` | Right | `x in y` — right side is a reference |
| 10 | `=` | Left | Assignment; right side is a reference |
| 20 | `->` | Left | Arrow / function type |
| 30 | `>>` `<<` `\|>` `<\|` | Left | Composition / pipe |
| 40 | `=>` | Right | Lambda / pattern bind; left side is declared |
| 70 | `./` `../` | Left | Relative path / member access |
| 80 | `catch` `\` | Left | Exception catch; `\` declares left side |
| 90 | `\|` `\|\|` | Left | Logical / union OR |
| 100 | `&` `&&` | Left | Logical / union AND |
| 103 | `?` `??` | Left | Optional / null-coalesce; right side is a reference |
| 110 | `!` | — | **Prefix** — logical NOT |
| 115 | `:` `::` `:::` | Left | Type annotation / cons; right side is a reference |
| 115 | `<` `<=` `>=` `>` | Left / None | Comparison; right side is a reference |
| 115 | `==` `!=` `!:` | Left / None | Equality / non-membership; right side is a reference |
| 120 | `...` `...<` `<...` `<...<` | Left | Range variants |
| 120 | `;` `;;` | Left | Sequencing |
| 130 | `+` `-` | Left | Additive |
| 140 | `*` `/` `%` `/%` | Left | Multiplicative |
| 150 | `^` | Left | Exponentiation |
| 160 | `.` `..` | Left | Member access; left side is a reference |

---

## Notes on Semantics

- **Reference** — several operators tag one side as a *reference* (e.g., `=`, `in`, `.`, `?`, `:`). This is a semantic marker in the AST, not a syntactic difference.
- **Declare** — `=>` and `\` tag the *left* side as a declaration (`Let`-wrapped).
- **Prefix keywords** (`let`, `public`, `private`, `internal`, `protected`) are not identifier-boundary-safe in the operator precedence parser the same way the `keyword` helper enforces; they rely on FParsec's `PrefixOperator` matching.
- **Whitespace** is insignificant and skipped between all tokens.
- **Strings** are delimited by `"` and do not support escape sequences.
