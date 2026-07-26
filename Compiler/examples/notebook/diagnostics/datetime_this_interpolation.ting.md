```ting
DateTime this => $"{Year}-{Month}-{Day}"
```
```ting-output
Compilation succeeded.

local[#0] (id(resolved:#1) ((id(resolved:#2) this[decl:#3]), $"{id(ref:[#0, 00000000-0000-0000-0000-000000000000])}-{id(ref:[#0, 00000000-0000-0000-0000-000000000000])}-{id(ref:[#0, 00000000-0000-0000-0000-000000000000])}"))

Scopes:
[0] NamesScope           #0
    names=[<name>]

Diagnostics:
- unresolved name 'Year'
- unresolved name 'Month'
- unresolved name 'Day'
```
