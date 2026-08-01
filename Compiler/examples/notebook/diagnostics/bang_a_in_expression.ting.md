```ting
!a in b + c * d
```
```ting-output
Compilation succeeded.

(id(resolved:#0) ((id(resolved:#1) id(ref:[00000000-0000-0000-0000-000000000000])), (id(resolved:#2) (id(ref:[00000000-0000-0000-0000-000000000000]), (id(resolved:#3) (id(ref:[00000000-0000-0000-0000-000000000000]), id(ref:[00000000-0000-0000-0000-000000000000])))))))

Scopes:
No scopes collected.

Diagnostics:
- (1,2-1,3) unresolved name 'a'
- (1,7-1,8) unresolved name 'b'
- (1,11-1,12) unresolved name 'c'
- (1,15-1,16) unresolved name 'd'
```
