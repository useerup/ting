```ting
let person.Name = "Zaphod"
```
```ting-output
Compilation succeeded.

local[#0] (id(resolved:#1) ((id(ref:[#0, 00000000-0000-0000-0000-000000000000])).[#2](id(decl:#3)), "Zaphod"))

Core:
local[#0] equals(project(id(ref:[#0, 00000000-0000-0000-0000-000000000000]), id(decl:#3)), "Zaphod")

Scopes:
[0] NamesScope           #0
    names=[Name<extension property> host=person.]
[1] NamesScope           #2
    names=[]

Diagnostics:
- (1,5-1,11) unresolved name 'person'
```
