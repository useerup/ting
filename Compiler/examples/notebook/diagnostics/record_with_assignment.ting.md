```ting
record(Answer:int) \ (Answer = 42)
```
```ting-output
Compilation succeeded.

Scopes:
[0] NamesScope           #0
    names=[Answer<name>]
[1] NamesScope           #1
    names=[Answer<name>]

Identifiers:
[0] (1,8) name='Answer' role=declarative bound-scopes=[#0, #1]
[1] (1,14) name='_:_' role=referential(resolved) resolved-declaration=#2
[2] (1,15) name='int' role=referential(resolved) resolved-declaration=#3
[3] (1,20) name='_\_' role=referential(resolved) resolved-declaration=#4
[4] (1,23) name='Answer' role=referential(resolved) resolved-declaration=#5
[5] (1,30) name='_=_' role=referential(resolved) resolved-declaration=#6
```
