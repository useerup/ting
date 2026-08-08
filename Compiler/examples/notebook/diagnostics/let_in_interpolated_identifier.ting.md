```ting
let answer=42 in $`Flollop{answer}`
```
```ting-output
Compilation succeeded.

Scopes:
[0] NamesScope           #0
    names=[answer<name>]

Identifiers:
[0] (1,5) name='answer' role=declarative bound-scopes=[#0]
[1] (1,11) name='_=_' role=referential(resolved) resolved-declaration=#1
[2] (1,15) name='_in_' role=referential(resolved) resolved-declaration=#2
[3] (1,18) name='Flollop{...}' role=referential candidate-scopes=[#0, 00000000-0000-0000-0000-000000000000]
```
