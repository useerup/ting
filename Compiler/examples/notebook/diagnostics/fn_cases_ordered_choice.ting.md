```ting
fn{ 0 -> 1, int n -> n * 2 }
```

```ting-output
Compilation succeeded.

Scopes:
[0] NamesScope           #0
    names=[]
[1] NamesScope           #1
    names=[n<name>]

Identifiers:
[0] (1,1) name='_||_' role=referential(resolved) resolved-declaration=#2
[1] (1,7) name='_=>_' role=referential(resolved) resolved-declaration=#3
[2] (1,13) name='int' role=referential(resolved) resolved-declaration=#4
[3] (1,17) name='n' role=declarative bound-scopes=[#1]
[4] (1,19) name='_=>_' role=referential(resolved) resolved-declaration=#3
[5] (1,22) name='n' role=referential(resolved) resolved-declaration=#5
[6] (1,24) name='_*_' role=referential(resolved) resolved-declaration=#6
```