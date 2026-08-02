```ting
let x = 42 in primary x = candidate x
```

```ting-output
Compilation succeeded.

local[#0] (id(resolved:#1) ((id(resolved:#2) (id(decl:#3), 42)), (id(resolved:#2) (primary(id(resolved:#3)), candidate(id(resolved:#3))))))

Core:
local[#0] (id(resolved:#1) (equals(id(decl:#3), 42), equals(primary(id(resolved:#3)), candidate(id(resolved:#3)))))

Scopes:
[0] NamesScope           #0
    names=[x<name>]

Value:
true
```
