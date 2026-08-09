```ting
T * T ^^ T where T:(Integrals|FloatingPoints|Lists)
```
```ting-output
Compilation succeeded.

Scopes:
[1] Local : bool, string, char, float, double, int, uint, byte, sbyte, short, ushort, long, ulong, half, decimal, DateTime, DateTimeOffset, set, any, Func, _all_
[2] Local : -_, _where_, _in_, _=_, _?=_, _then_, _else_, _contract_, _->_, _>>_, _<<_, _|>_, _<|_, _=>_, _catch_, _\_, _|_, _||_, _|||_, _&_, _&&_, _&&&_, _?_, _??_, !_, _:_, _::_, _:::_, _<_, _<=_, _>=_, _>_, _==_, _!=_, _!:_, _..._, _...<_, _<..._, _<...<_, _;_, _;;_, _^^_, _**_, _+_, _-_, _*_, _/_, _%_, _/%_, _^_, fn_, class_, total_, partial_
[3] Local : T

Identifiers:
T[1,1] ref - scope 3
_*_[1,3] ref - scope 2
T[1,5] ref - scope 3
_^^_[1,7] ref - scope 2
T[1,10] ref - scope 3
_where_[1,12] ref - scope 2
T[1,18] decl - scope 3
_:_[1,19] ref - scope 2
Integrals[1,21] ref - unresolved
_|_[1,30] ref - scope 2
FloatingPoints[1,31] ref - unresolved
_|_[1,45] ref - scope 2
Lists[1,46] ref - unresolved

Diagnostics:
- (1,21-1,30) unresolved name 'Integrals'
- (1,31-1,45) unresolved name 'FloatingPoints'
- (1,46-1,51) unresolved name 'Lists'
```
