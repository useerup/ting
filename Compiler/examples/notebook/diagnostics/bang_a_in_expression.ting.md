```ting
!a in b + c * d
```
```ting-output
Compilation succeeded.

Scopes:
[1] Local : bool, string, char, float, double, int, uint, byte, sbyte, short, ushort, long, ulong, half, decimal, DateTime, DateTimeOffset, set, any, Func, _all_
[2] Local : -_, _where_, _in_, _=_, _?=_, _then_, _else_, _contract_, _->_, _>>_, _<<_, _|>_, _<|_, _=>_, _catch_, _\_, _|_, _||_, _|||_, _&_, _&&_, _&&&_, _?_, _??_, !_, _:_, _::_, _:::_, _<_, _<=_, _>=_, _>_, _==_, _!=_, _!:_, _..._, _...<_, _<..._, _<...<_, _;_, _;;_, _^^_, _**_, _+_, _-_, _*_, _/_, _%_, _/%_, _^_, fn_, class_, total_, partial_

Identifiers:
!_[1,1] ref - scope 2
a[1,2] ref - unresolved
_in_[1,4] ref - scope 2
b[1,7] ref - unresolved
_+_[1,9] ref - scope 2
c[1,11] ref - unresolved
_*_[1,13] ref - scope 2
d[1,15] ref - unresolved

Diagnostics:
- (1,2-1,3) unresolved name 'a'
- (1,7-1,8) unresolved name 'b'
- (1,11-1,12) unresolved name 'c'
- (1,15-1,16) unresolved name 'd'
```
