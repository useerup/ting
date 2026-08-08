```ting
let x = 42 in primary x = candidate x
```

```ting-output
Compilation succeeded.

Scopes:
[0] NamesScope           #0
    names=[x<name>]

Identifiers:
[0] (1,5) name='x' role=declarative bound-scopes=[#0]
[1] (1,7) name='_=_' role=referential(resolved) resolved-declaration=#1
[2] (1,12) name='_in_' role=referential(resolved) resolved-declaration=#2
[3] (1,23) name='x' role=referential(resolved) resolved-declaration=#3
[4] (1,25) name='_=_' role=referential(resolved) resolved-declaration=#1
[5] (1,37) name='x' role=referential(resolved) resolved-declaration=#3
```
