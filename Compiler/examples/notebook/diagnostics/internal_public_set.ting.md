```ting
internal public { 1, 2, 3, a }
```
```ting-output
Compilation succeeded.

internal[#0] public[#1] {1, 2, 3, id(decl:#2)}

Core:
internal[#0] public[#1] {1, 2, 3, id(decl:#2)}

Scopes:
[0] NamesScope           #0
    names=[a<name>]
[1] NamesScope           #1
    names=[a<name>]
```
