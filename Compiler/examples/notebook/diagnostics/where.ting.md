```ting
T * T ^^ T where T:(Integrals|FloatingPoints|Lists)
```
```ting-output
Compilation succeeded.

local[#0] (id(ref:[00000000-0000-0000-0000-000000000000]) ((id(resolved:#1) ((id(resolved:#2) (id(resolved:#3), id(resolved:#3))), id(resolved:#3))), (id(resolved:#4) (id(decl:#3), ((id(resolved:#5) ((id(resolved:#5) (id(ref:[#0, 00000000-0000-0000-0000-000000000000]), id(ref:[#0, 00000000-0000-0000-0000-000000000000]))), id(ref:[#0, 00000000-0000-0000-0000-000000000000]))))))))

Scopes:
[0] NamesScope           #0
    names=[T<name>]

Diagnostics:
- unresolved name '_where_'
- unresolved name 'Integrals'
- unresolved name 'FloatingPoints'
- unresolved name 'Lists'
```
