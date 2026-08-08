```ting
T * T ^^ T where T:(Integrals|FloatingPoints|Lists)
```
```ting-output
Compilation succeeded.

Scopes:
[0] NamesScope           #0
    names=[T<name>]

Identifiers:
[0] (1,1) name='T' role=referential(resolved) resolved-declaration=#1
[1] (1,3) name='_*_' role=referential(resolved) resolved-declaration=#2
[2] (1,5) name='T' role=referential(resolved) resolved-declaration=#1
[3] (1,7) name='_^^_' role=referential(resolved) resolved-declaration=#3
[4] (1,10) name='T' role=referential(resolved) resolved-declaration=#1
[5] (1,12) name='_where_' role=referential(resolved) resolved-declaration=#4
[6] (1,18) name='T' role=declarative bound-scopes=[#0]
[7] (1,19) name='_:_' role=referential(resolved) resolved-declaration=#5
[8] (1,21) name='Integrals' role=referential candidate-scopes=[#0, 00000000-0000-0000-0000-000000000000]
[9] (1,30) name='_|_' role=referential(resolved) resolved-declaration=#6
[10] (1,31) name='FloatingPoints' role=referential candidate-scopes=[#0, 00000000-0000-0000-0000-000000000000]
[11] (1,45) name='_|_' role=referential(resolved) resolved-declaration=#6
[12] (1,46) name='Lists' role=referential candidate-scopes=[#0, 00000000-0000-0000-0000-000000000000]

Diagnostics:
- (1,21-1,30) unresolved name 'Integrals'
- (1,31-1,45) unresolved name 'FloatingPoints'
- (1,46-1,51) unresolved name 'Lists'
```
