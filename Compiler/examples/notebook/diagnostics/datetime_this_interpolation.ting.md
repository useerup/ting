```ting
DateTime this => $"{Year}-{Month}-{Day}"
```
```ting-output
Compilation succeeded.

local[#0] (id(resolved:#1) ((id(resolved:#2) this[decl:#3]), $"{id(ref:[#0, 00000000-0000-0000-0000-000000000000])}-{id(ref:[#0, 00000000-0000-0000-0000-000000000000])}-{id(ref:[#0, 00000000-0000-0000-0000-000000000000])}"))

Core:
local[#0] (id(resolved:#1) ((id(resolved:#2) this[decl:#3]), $"{id(ref:[#0, 00000000-0000-0000-0000-000000000000])}-{id(ref:[#0, 00000000-0000-0000-0000-000000000000])}-{id(ref:[#0, 00000000-0000-0000-0000-000000000000])}"))

Scopes:
[0] NamesScope           #0
    names=[<name>]

Diagnostics:
- (1,21-1,25) unresolved name 'Year'
- (1,28-1,33) unresolved name 'Month'
- (1,36-1,39) unresolved name 'Day'
```
