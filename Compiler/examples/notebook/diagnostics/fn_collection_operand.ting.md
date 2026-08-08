```ting
let mappings = { 1 -> 10, 2 -> 20 } in fn mappings
```

```ting-output
Compilation succeeded.

Scopes:
[0] NamesScope           #0
    names=[mappings<name>]

Identifiers:
[0] (1,5) name='mappings' role=declarative bound-scopes=[#0]
[1] (1,14) name='_=_' role=referential(resolved) resolved-declaration=#1
[2] (1,20) name='_->_' role=referential(resolved) resolved-declaration=#2
[3] (1,29) name='_->_' role=referential(resolved) resolved-declaration=#2
[4] (1,37) name='_in_' role=referential(resolved) resolved-declaration=#3
[5] (1,40) name='fn_' role=referential(resolved) resolved-declaration=#4
[6] (1,43) name='mappings' role=referential(resolved) resolved-declaration=#5
```