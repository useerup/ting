```ting
let f ?= fn{ 0 -> 1, int n -> n * f (n - 1) } in f 3
```
```ting-output
Compilation succeeded.

local[#0] (id(resolved:#1) ((id(resolved:#2) (id(decl:#3), (id(resolved:#4) (local[#5] (id(resolved:#6) (0, 1)), local[#7] (id(resolved:#6) ((id(resolved:#8) id(decl:#9)), (id(resolved:#10) (id(resolved:#9), (id(resolved:#3) ((id(resolved:#11) (id(resolved:#9), 1)))))))))))), (id(resolved:#3) 3)))

Scopes:
[0] NamesScope           #0
    names=[f<name>]
[1] NamesScope           #5
    names=[]
[2] NamesScope           #7
    names=[n<name>]
```