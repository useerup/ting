```ting
let pair = (1, 2) in (pair @@ Result = 3)
```

```ting-output
Compilation succeeded.

Scopes:
[0] NamesScope           #0
    names=[pair<name>]
[1] NamesScope           #1
    names=[Result<name>]

Identifiers:
[0] (1,5) name='pair' role=declarative bound-scopes=[#0]
[1] (1,10) name='_=_' role=referential(resolved) resolved-declaration=#2
[2] (1,19) name='_in_' role=referential(resolved) resolved-declaration=#3
[3] (1,23) name='pair' role=referential(resolved) resolved-declaration=#4
[4] (1,31) name='Result' role=declarative bound-scopes=[#1]
[5] (1,38) name='_=_' role=referential(resolved) resolved-declaration=#2
```