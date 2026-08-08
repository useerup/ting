```ting
a==2 then a==3 then 0 else 1 else 2
```
```ting-output
Compilation succeeded.

Scopes:
No scopes collected.

Identifiers:
[0] (1,1) name='a' role=referential candidate-scopes=[00000000-0000-0000-0000-000000000000]
[1] (1,2) name='_==_' role=referential(resolved) resolved-declaration=#0
[2] (1,6) name='_then_' role=referential(resolved) resolved-declaration=#1
[3] (1,11) name='a' role=referential candidate-scopes=[00000000-0000-0000-0000-000000000000]
[4] (1,12) name='_==_' role=referential(resolved) resolved-declaration=#0
[5] (1,16) name='_then_' role=referential(resolved) resolved-declaration=#1
[6] (1,23) name='_else_' role=referential(resolved) resolved-declaration=#2
[7] (1,30) name='_else_' role=referential(resolved) resolved-declaration=#2

Diagnostics:
- (1,1-1,2) unresolved name 'a'
- (1,11-1,12) unresolved name 'a'
```
