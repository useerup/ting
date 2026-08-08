```ting
let x = 1 in x ?? throw x
```

```ting-output
Compilation succeeded.

Scopes:
[0] NamesScope           #0
    names=[x<name>]

Identifiers:
[0] (1,5) name='x' role=declarative bound-scopes=[#0]
[1] (1,7) name='_=_' role=referential(resolved) resolved-declaration=#1
[2] (1,11) name='_in_' role=referential(resolved) resolved-declaration=#2
[3] (1,14) name='x' role=referential(resolved) resolved-declaration=#3
[4] (1,16) name='_??_' role=referential(resolved) resolved-declaration=#4
[5] (1,25) name='x' role=referential(resolved) resolved-declaration=#3
```