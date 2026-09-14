```ting
(x + y) * (a - b) in z
```
```ting-output
Compilation failed.

Scopes:
[1] Local : bool, string, char, float, double, int, uint, byte, sbyte, short, ushort, long, ulong, half, decimal, DateTime, DateTimeOffset, set, any, Func, _all_
[2] Local : -_, _where_, _in_, _=_, _?=_, _then_, _else_, _contract_, _->_, _>>_, _<<_, _|>_, _<|_, _=>_, _catch_, _\_, _|_, _||_, _|||_, _&_, _&&_, _&&&_, _?_, _??_, !_, _:_, _::_, _:::_, _<_, _<=_, _>=_, _>_, _==_, _!=_, _!:_, _..._, _...<_, _<..._, _<...<_, _;_, _;;_, _^^_, _**_, _+_, _-_, _*_, _/_, _%_, _/%_, _^_, fn_, class_, total_, partial_

Identifiers:
x[1,2] ref - unresolved
_+_[1,4] ref - scope 2
y[1,6] ref - unresolved
_*_[1,9] ref - scope 2
a[1,12] ref - unresolved
_-_[1,14] ref - scope 2
b[1,16] ref - unresolved
_in_[1,19] ref - scope 2
z[1,22] ref - unresolved

Diagnostics:
- (1,2-1,3) unresolved name 'x'
- (1,6-1,7) unresolved name 'y'
- (1,12-1,13) unresolved name 'a'
- (1,16-1,17) unresolved name 'b'
- (1,22-1,23) unresolved name 'z'
```
