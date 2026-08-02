```ting
a==2 then a==3 then 0 else 1 else 2
```
```ting-output
Compilation succeeded.

(id(resolved:#0) ((id(resolved:#1) (id(ref:[00000000-0000-0000-0000-000000000000]), 2)), (id(resolved:#0) ((id(resolved:#1) (id(ref:[00000000-0000-0000-0000-000000000000]), 3)), (id(resolved:#2) (0, (id(resolved:#2) (1, 2))))))))

Core:
(id(resolved:#0) (equals(primary(id(ref:[00000000-0000-0000-0000-000000000000])), primary(2)), (id(resolved:#0) (equals(primary(id(ref:[00000000-0000-0000-0000-000000000000])), primary(3)), (id(resolved:#2) (0, (id(resolved:#2) (1, 2))))))))

Scopes:
No scopes collected.

Diagnostics:
- (1,1-1,2) unresolved name 'a'
- (1,11-1,12) unresolved name 'a'
```
