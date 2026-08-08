```ting
let f = a => b => a - b in f -1 -2
```

```ting-output
Compilation succeeded.

Scopes:
[0] NamesScope           #0
    names=[f<name>]
[1] NamesScope           #1
    names=[a<name>]
[2] NamesScope           #2
    names=[b<name>]

Identifiers:
[0] (1,5) name='f' role=declarative bound-scopes=[#0]
[1] (1,7) name='_=_' role=referential(resolved) resolved-declaration=#3
[2] (1,9) name='a' role=declarative bound-scopes=[#1]
[3] (1,11) name='_=>_' role=referential(resolved) resolved-declaration=#4
[4] (1,14) name='b' role=declarative bound-scopes=[#2]
[5] (1,16) name='_=>_' role=referential(resolved) resolved-declaration=#4
[6] (1,19) name='a' role=referential(resolved) resolved-declaration=#5
[7] (1,21) name='_-_' role=referential(resolved) resolved-declaration=#6
[8] (1,23) name='b' role=referential(resolved) resolved-declaration=#7
[9] (1,25) name='_in_' role=referential(resolved) resolved-declaration=#8
[10] (1,28) name='f' role=referential(resolved) resolved-declaration=#9
[11] (1,30) name='-_' role=referential(resolved) resolved-declaration=#10
[12] (1,33) name='-_' role=referential(resolved) resolved-declaration=#10
```