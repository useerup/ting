```ting
let n = 10 in n-1 + -2
```

```ting-output
Compilation succeeded.

local[#0] (id(resolved:#1) ((id(resolved:#2) (id(decl:#3), 10)), (id(resolved:#4) ((id(resolved:#5) (id(resolved:#3), 1)), (id(resolved:#6) 2)))))

Core:
local[#0] (id(resolved:#1) (equals(id(decl:#3), 10), (id(resolved:#4) ((id(resolved:#5) (id(resolved:#3), 1)), (id(resolved:#6) 2)))))

Scopes:
[0] NamesScope           #0
    names=[n<name>]

Value:
7
```