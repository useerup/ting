```ting
let x = 1 in x ?? throw x
```

```ting-output
Compilation succeeded.

local[#0] (id(resolved:#1) ((id(resolved:#2) (id(decl:#3), 1)), (id(resolved:#4) (id(resolved:#3), throw id(resolved:#3)))))

Core:
local[#0] (id(resolved:#1) (equals(id(decl:#3), 1), (id(resolved:#4) (id(resolved:#3), throw(id(resolved:#3))))))

Scopes:
[0] NamesScope           #0
    names=[x<name>]
```