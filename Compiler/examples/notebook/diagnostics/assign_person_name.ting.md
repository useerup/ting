```ting
person.Name = "Zaphod"
```
```ting-output
Compilation succeeded.

Scopes:
[0] NamesScope           #0
    names=[]

Identifiers:
[0] (1,1) name='person' role=referential candidate-scopes=[00000000-0000-0000-0000-000000000000]
[1] (1,8) name='Name' role=referential candidate-scopes=[00000000-0000-0000-0000-000000000000]
[2] (1,13) name='_=_' role=referential(resolved) resolved-declaration=#1

Diagnostics:
- (1,1-1,7) unresolved name 'person'
```
