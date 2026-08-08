```ting
let f ?= fn{ 0 -> 1, int n -> n * f (n - 1) } in f 3
```
```ting-output
Compilation succeeded.

Scopes:
[0] NamesScope           #0
    names=[f<name>]
[1] NamesScope           #1
    names=[]
[2] NamesScope           #2
    names=[n<name>]

Identifiers:
[0] (1,1) name='_||_' role=referential(resolved) resolved-declaration=#3
[1] (1,5) name='f' role=declarative bound-scopes=[#0]
[2] (1,7) name='_?=_' role=referential(resolved) resolved-declaration=#4
[3] (1,16) name='_=>_' role=referential(resolved) resolved-declaration=#5
[4] (1,22) name='int' role=referential(resolved) resolved-declaration=#6
[5] (1,26) name='n' role=declarative bound-scopes=[#2]
[6] (1,28) name='_=>_' role=referential(resolved) resolved-declaration=#5
[7] (1,31) name='n' role=referential(resolved) resolved-declaration=#7
[8] (1,33) name='_*_' role=referential(resolved) resolved-declaration=#8
[9] (1,35) name='f' role=referential(resolved) resolved-declaration=#9
[10] (1,38) name='n' role=referential(resolved) resolved-declaration=#7
[11] (1,40) name='_-_' role=referential(resolved) resolved-declaration=#10
[12] (1,47) name='_in_' role=referential(resolved) resolved-declaration=#11
[13] (1,50) name='f' role=referential(resolved) resolved-declaration=#9
```