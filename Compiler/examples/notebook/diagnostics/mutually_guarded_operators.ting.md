```ting
let a = { 1, 2 } in let b = { 2, 3 } in (a &&& b) ||| a
```

```ting-output
Compilation succeeded.

local[#0] (id(resolved:#1) ((id(resolved:#2) (id(decl:#3), {1, 2})), local[#4] (id(resolved:#1) ((id(resolved:#2) (id(decl:#5), {2, 3})), (id(resolved:#6) (((id(resolved:#7) (id(resolved:#3), id(resolved:#5)))), id(resolved:#3)))))))

Core:
local[#0] (id(resolved:#1) (equals(id(decl:#3), {1, 2}), local[#4] (id(resolved:#1) (equals(id(decl:#5), {2, 3}), (id(resolved:#6) (((id(resolved:#7) (id(resolved:#3), id(resolved:#5)))), id(resolved:#3)))))))

Scopes:
[0] NamesScope           #0
    names=[a<name>]
[1] NamesScope           #4
    names=[b<name>]
```