```ting
let f = a => b => a - b in f -1 -2
```

```ting-output
Compilation succeeded.

local[#0] (id(resolved:#1) ((id(resolved:#2) (id(decl:#3), local[#4] (id(resolved:#5) (id(decl:#6), local[#7] (id(resolved:#5) (id(decl:#8), (id(resolved:#9) (id(resolved:#6), id(resolved:#8))))))))), ((id(resolved:#3) (id(resolved:#10) 1)) (id(resolved:#10) 2))))

Core:
local[#0] (id(resolved:#1) (equals(id(decl:#3), local[#4] (id(resolved:#5) (id(decl:#6), local[#7] (id(resolved:#5) (id(decl:#8), (id(resolved:#9) (id(resolved:#6), id(resolved:#8)))))))), ((id(resolved:#3) (id(resolved:#10) 1)) (id(resolved:#10) 2))))

Scopes:
[0] NamesScope           #0
    names=[f<name>]
[1] NamesScope           #4
    names=[a<name>]
[2] NamesScope           #7
    names=[b<name>]

Value:
1
```