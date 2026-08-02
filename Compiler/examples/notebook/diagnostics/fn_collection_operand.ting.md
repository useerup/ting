```ting
let mappings = { 1 -> 10, 2 -> 20 } in fn mappings
```

```ting-output
Compilation succeeded.

local[#0] (id(resolved:#1) ((id(resolved:#2) (id(decl:#3), {(id(resolved:#4) (1, 10)), (id(resolved:#4) (2, 20))})), (id(resolved:#5) id(resolved:#3))))

Core:
local[#0] (id(resolved:#1) (equals(id(decl:#3), {(id(resolved:#4) (1, 10)), (id(resolved:#4) (2, 20))}), (id(resolved:#5) id(resolved:#3))))

Scopes:
[0] NamesScope           #0
    names=[mappings<name>]
```