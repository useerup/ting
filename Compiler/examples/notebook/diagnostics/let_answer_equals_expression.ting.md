```ting
let x=21 in Answer = x*2
```
```ting-output
Compilation succeeded.

local[#0] (id(resolved:#1) ((id(resolved:#2) (id(decl:#3), 21)), (id(resolved:#2) (id(ref:[#0, 00000000-0000-0000-0000-000000000000]), (id(resolved:#4) (id(resolved:#3), 2))))))

Scopes:
[0] NamesScope           #0
    names=[x<name>]

Diagnostics:
- unresolved name 'Answer'
```
