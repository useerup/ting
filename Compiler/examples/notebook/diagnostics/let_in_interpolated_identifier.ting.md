```ting
let answer=42 in $`Flollop{answer}`
```
```ting-output
Compilation succeeded.

local[#0] (id(resolved:#1) ((id(resolved:#2) (id(decl:#3), 42)), id(ref:[#0, 00000000-0000-0000-0000-000000000000])))

Core:
local[#0] (id(resolved:#1) (equals(id(decl:#3), 42), id(ref:[#0, 00000000-0000-0000-0000-000000000000])))

Scopes:
[0] NamesScope           #0
    names=[answer<name>]
```
