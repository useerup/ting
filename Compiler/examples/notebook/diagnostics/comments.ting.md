```ting
// A line comment before the expression.
let x = 1 /* a block comment /* nested */ inside an expression */ + 2 in
    x // and one at the end of a line
```

```ting-output
Compilation succeeded.

Scopes:
[0] NamesScope           #0
    names=[x<name>]

Identifiers:
[0] (2,5) name='x' role=declarative bound-scopes=[#0]
[1] (2,7) name='_=_' role=referential(resolved) resolved-declaration=#1
[2] (2,67) name='_+_' role=referential(resolved) resolved-declaration=#2
[3] (2,71) name='_in_' role=referential(resolved) resolved-declaration=#3
[4] (3,5) name='x' role=referential(resolved) resolved-declaration=#4
```