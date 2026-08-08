```ting
let a = { 1, 2 } in let b = { 2, 3 } in (a &&& b) ||| a
```

```ting-output
Compilation succeeded.

Scopes:
[0] NamesScope           #0
    names=[a<name>]
[1] NamesScope           #1
    names=[b<name>]

Identifiers:
[0] (1,5) name='a' role=declarative bound-scopes=[#0]
[1] (1,7) name='_=_' role=referential(resolved) resolved-declaration=#2
[2] (1,18) name='_in_' role=referential(resolved) resolved-declaration=#3
[3] (1,25) name='b' role=declarative bound-scopes=[#1]
[4] (1,27) name='_=_' role=referential(resolved) resolved-declaration=#2
[5] (1,38) name='_in_' role=referential(resolved) resolved-declaration=#3
[6] (1,42) name='a' role=referential(resolved) resolved-declaration=#4
[7] (1,44) name='_&&&_' role=referential(resolved) resolved-declaration=#5
[8] (1,48) name='b' role=referential(resolved) resolved-declaration=#6
[9] (1,51) name='_|||_' role=referential(resolved) resolved-declaration=#7
[10] (1,55) name='a' role=referential(resolved) resolved-declaration=#4
```