```ting
$"{Word name} = {int value}" => (name->value)
```
```ting-output
Compilation succeeded.

local[#0] (id(resolved:#1) ($"{(id(ref:[#0, 00000000-0000-0000-0000-000000000000]) id(decl:#2))} = {(id(resolved:#3) id(decl:#4))}", ((id(resolved:#5) (id(resolved:#2), id(resolved:#4))))))

Scopes:
[0] NamesScope           #0
    names=[name<name>, value<name>]

Diagnostics:
- (1,4-1,8) unresolved name 'Word'
```
