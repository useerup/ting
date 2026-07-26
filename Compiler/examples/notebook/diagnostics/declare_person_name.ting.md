```ting
let person.Name = "Zaphod"
```
```ting-output
Compilation succeeded.

local[#0] (id(resolved:#1) ((id(ref:[#0, 00000000-0000-0000-0000-000000000000])).[#2](id(decl:#3)), "Zaphod"))

Scopes:
[0] NamesScope           #0
    names=[Name<extension property> host=person.]
[1] NamesScope           #2
    names=[]

Diagnostics:
- unresolved name 'person'
```
