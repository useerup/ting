```ting
let x=21 in Answer = x*2
```
```ting-output
Compilation succeeded.

local[#0] (id(resolved:#1) ((id(resolved:#2) (id(decl:#3), 21)), (id(resolved:#2) (id(ref:[#0, 00000000-0000-0000-0000-000000000000]), (id(resolved:#4) (id(resolved:#3), 2))))))

Core:
local[#0] (id(resolved:#1) (equals(id(decl:#3), 21), equals(id(ref:[#0, 00000000-0000-0000-0000-000000000000]), (id(resolved:#4) (id(resolved:#3), 2)))))

Scopes:
[0] NamesScope           #0
    names=[x<name>]

Diagnostics:
- (1,13-1,19) unresolved name 'Answer'
```
