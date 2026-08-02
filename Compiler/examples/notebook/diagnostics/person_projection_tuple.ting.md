```ting
person.(Name,Age)
```
```ting-output
Compilation succeeded.

(id(ref:[00000000-0000-0000-0000-000000000000])).[#0]((id(ref:[00000000-0000-0000-0000-000000000000]), id(ref:[00000000-0000-0000-0000-000000000000])))

Core:
project(id(ref:[00000000-0000-0000-0000-000000000000]), (id(ref:[00000000-0000-0000-0000-000000000000]), id(ref:[00000000-0000-0000-0000-000000000000])))

Scopes:
[0] NamesScope           #0
    names=[]

Diagnostics:
- (1,1-1,7) unresolved name 'person'
- (1,9-1,13) unresolved name 'Name'
- (1,14-1,17) unresolved name 'Age'
```
