```ting
x => x * 2m
```
```ting-output
Compilation succeeded.

Scopes:
[0] NamesScope           #0
    names=[x<name>]

Identifiers:
[0] (1,1) name='x' role=declarative bound-scopes=[#0]
[1] (1,3) name='_=>_' role=referential(resolved) resolved-declaration=#1
[2] (1,6) name='x' role=referential(resolved) resolved-declaration=#2
[3] (1,8) name='_*_' role=referential(resolved) resolved-declaration=#3
```
