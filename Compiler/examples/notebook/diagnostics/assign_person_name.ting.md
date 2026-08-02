```ting
person.Name = "Zaphod"
```
```ting-output
Compilation succeeded.

(id(resolved:#0) ((id(ref:[00000000-0000-0000-0000-000000000000])).[#1](id(ref:[00000000-0000-0000-0000-000000000000])), "Zaphod"))

Core:
equals(project(id(ref:[00000000-0000-0000-0000-000000000000]), id(ref:[00000000-0000-0000-0000-000000000000])), "Zaphod")

Scopes:
[0] NamesScope           #1
    names=[]

Diagnostics:
- (1,1-1,7) unresolved name 'person'
```
