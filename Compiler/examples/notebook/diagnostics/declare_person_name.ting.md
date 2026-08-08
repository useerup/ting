```ting
let person.Name = "Zaphod"
```
```ting-output
Compilation succeeded.

Scopes:
[0] NamesScope           #0
    names=[Name<extension property> host=person.]
[1] NamesScope           #1
    names=[]

Identifiers:
[0] (1,5) name='person' role=referential candidate-scopes=[#0, 00000000-0000-0000-0000-000000000000]
[1] (1,12) name='Name' role=declarative bound-scopes=[#0]
[2] (1,17) name='_=_' role=referential(resolved) resolved-declaration=#2

Diagnostics:
- (1,5-1,11) unresolved name 'person'
```
