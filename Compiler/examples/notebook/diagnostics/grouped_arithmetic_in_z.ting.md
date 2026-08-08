```ting
(x + y) * (a - b) in z
```
```ting-output
Compilation succeeded.

Scopes:
No scopes collected.

Identifiers:
[0] (1,2) name='x' role=referential candidate-scopes=[00000000-0000-0000-0000-000000000000]
[1] (1,4) name='_+_' role=referential(resolved) resolved-declaration=#0
[2] (1,6) name='y' role=referential candidate-scopes=[00000000-0000-0000-0000-000000000000]
[3] (1,9) name='_*_' role=referential(resolved) resolved-declaration=#1
[4] (1,12) name='a' role=referential candidate-scopes=[00000000-0000-0000-0000-000000000000]
[5] (1,14) name='_-_' role=referential(resolved) resolved-declaration=#2
[6] (1,16) name='b' role=referential candidate-scopes=[00000000-0000-0000-0000-000000000000]
[7] (1,19) name='_in_' role=referential(resolved) resolved-declaration=#3
[8] (1,22) name='z' role=referential candidate-scopes=[00000000-0000-0000-0000-000000000000]

Diagnostics:
- (1,2-1,3) unresolved name 'x'
- (1,6-1,7) unresolved name 'y'
- (1,12-1,13) unresolved name 'a'
- (1,16-1,17) unresolved name 'b'
- (1,22-1,23) unresolved name 'z'
```
