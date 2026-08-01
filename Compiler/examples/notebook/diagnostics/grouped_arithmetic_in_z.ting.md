```ting
(x + y) * (a - b) in z
```
```ting-output
Compilation succeeded.

(id(resolved:#0) ((id(resolved:#1) (((id(resolved:#2) (id(ref:[00000000-0000-0000-0000-000000000000]), id(ref:[00000000-0000-0000-0000-000000000000])))), ((id(resolved:#3) (id(ref:[00000000-0000-0000-0000-000000000000]), id(ref:[00000000-0000-0000-0000-000000000000])))))), id(ref:[00000000-0000-0000-0000-000000000000])))

Scopes:
No scopes collected.

Diagnostics:
- (1,2-1,3) unresolved name 'x'
- (1,6-1,7) unresolved name 'y'
- (1,12-1,13) unresolved name 'a'
- (1,16-1,17) unresolved name 'b'
- (1,22-1,23) unresolved name 'z'
```
