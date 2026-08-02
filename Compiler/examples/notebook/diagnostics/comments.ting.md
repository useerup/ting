```ting
// A line comment before the expression.
let x = 1 /* a block comment /* nested */ inside an expression */ + 2 in
    x // and one at the end of a line
```

```ting-output
Compilation succeeded.

local[#0] (id(resolved:#1) ((id(resolved:#2) (id(decl:#3), (id(resolved:#4) (1, 2)))), id(resolved:#3)))

Core:
local[#0] (id(resolved:#1) (equals(id(decl:#3), (id(resolved:#4) (1, 2))), id(resolved:#3)))

Scopes:
[0] NamesScope           #0
    names=[x<name>]

Value:
3
```