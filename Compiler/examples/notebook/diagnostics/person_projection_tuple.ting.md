```ting
person.(Name,Age)
```
```ting-output
Compilation succeeded.

Scopes:
[0] NamesScope           #0
    names=[]

Identifiers:
[0] (1,1) name='person' role=referential candidate-scopes=[00000000-0000-0000-0000-000000000000]
[1] (1,9) name='Name' role=referential candidate-scopes=[00000000-0000-0000-0000-000000000000]
[2] (1,14) name='Age' role=referential candidate-scopes=[00000000-0000-0000-0000-000000000000]

Diagnostics:
- (1,9-1,13) unresolved name 'Name'
- (1,14-1,17) unresolved name 'Age'
- (1,1-1,7) unresolved name 'person'
```
