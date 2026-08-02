```ting
x => x + y => x + y + z => x
```
```ting-output
Compilation succeeded.

local[#0] (id(resolved:#1) (id(decl:#2), local[#3] (id(resolved:#1) ((id(resolved:#4) (id(decl:#5), id(decl:#6))), local[#7] (id(resolved:#1) ((id(resolved:#4) ((id(resolved:#4) (id(decl:#8), id(decl:#9))), id(decl:#10))), id(resolved:#8)))))))

Core:
local[#0] (id(resolved:#1) (id(decl:#2), local[#3] (id(resolved:#1) ((id(resolved:#4) (id(decl:#5), id(decl:#6))), local[#7] (id(resolved:#1) ((id(resolved:#4) ((id(resolved:#4) (id(decl:#8), id(decl:#9))), id(decl:#10))), id(resolved:#8)))))))

Scopes:
[0] NamesScope           #0
    names=[x<name>]
[1] NamesScope           #3
    names=[x<name>, y<name>]
[2] NamesScope           #7
    names=[x<name>, y<name>, z<name>]

Diagnostics:
- (1,28-1,29) illegal shadowing of 'x' in nested local scope
```
