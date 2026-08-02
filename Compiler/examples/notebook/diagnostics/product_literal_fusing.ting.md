```ting
let pair = (1, 2) in (pair @ Result = 3)
```

```ting-output
Compilation succeeded.

local[#0] (id(resolved:#1) ((id(resolved:#2) (id(decl:#3), (1, 2))), (id(resolved:#3) @[#4] (id(resolved:#2) (id(decl:#5), 3)))))

Core:
local[#0] (id(resolved:#1) (equals(id(decl:#3), (1, 2)), (id(resolved:#3) @[#4] equals(id(decl:#5), 3))))

Scopes:
[0] NamesScope           #0
    names=[pair<name>]
[1] NamesScope           #4
    names=[Result<name>]

Value:
(1, 2 @ Result = 3)
```