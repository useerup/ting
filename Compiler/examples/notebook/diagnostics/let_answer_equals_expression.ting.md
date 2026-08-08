```ting
let x=21 in Answer = x*2
```
```ting-output
Compilation succeeded.

Scopes:
[0] NamesScope           #0
    names=[x<name>]

Identifiers:
[0] (1,5) name='x' role=declarative bound-scopes=[#0]
[1] (1,6) name='_=_' role=referential(resolved) resolved-declaration=#1
[2] (1,10) name='_in_' role=referential(resolved) resolved-declaration=#2
[3] (1,13) name='Answer' role=referential candidate-scopes=[#0, 00000000-0000-0000-0000-000000000000]
[4] (1,20) name='_=_' role=referential(resolved) resolved-declaration=#1
[5] (1,22) name='x' role=referential(resolved) resolved-declaration=#3
[6] (1,23) name='_*_' role=referential(resolved) resolved-declaration=#4

Diagnostics:
- (1,13-1,19) unresolved name 'Answer'
```
