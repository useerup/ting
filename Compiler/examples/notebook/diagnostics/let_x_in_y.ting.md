```ting
let x in y
```
```ting-output
Compilation succeeded.

Scopes:
[0] NamesScope           #0
    names=[x<name>]

Identifiers:
[0] (1,5) name='x' role=declarative bound-scopes=[#0]
[1] (1,7) name='_in_' role=referential(resolved) resolved-declaration=#1
[2] (1,10) name='y' role=referential candidate-scopes=[#0, 00000000-0000-0000-0000-000000000000]

Diagnostics:
- (1,10-1,11) unresolved name 'y'
```
