```ting
x => x + y => x + y + z => x
```
```ting-output
Compilation failed.

Scopes:
[1] Local : bool, string, char, float, double, int, uint, byte, sbyte, short, ushort, long, ulong, half, decimal, DateTime, DateTimeOffset, set, any, Func, _all_
[2] Local : -_, _where_, _in_, _=_, _?=_, _then_, _else_, _contract_, _->_, _>>_, _<<_, _|>_, _<|_, _=>_, _catch_, _\_, _|_, _||_, _|||_, _&_, _&&_, _&&&_, _?_, _??_, !_, _:_, _::_, _:::_, _<_, _<=_, _>=_, _>_, _==_, _!=_, _!:_, _..._, _...<_, _<..._, _<...<_, _;_, _;;_, _^^_, _**_, _+_, _-_, _*_, _/_, _%_, _/%_, _^_, fn_, class_, total_, partial_
[3] Local : x
[4] Local : x, y
[5] Local : x, y, z

Identifiers:
x[1,1] decl - scope 3
_=>_[1,3] ref - scope 2
x[1,6] decl - scope 4
_+_[1,8] ref - scope 2
y[1,10] decl - scope 4
_=>_[1,12] ref - scope 2
x[1,15] decl - scope 5
_+_[1,17] ref - scope 2
y[1,19] decl - scope 5
_+_[1,21] ref - scope 2
z[1,23] decl - scope 5
_=>_[1,25] ref - scope 2
x[1,28] ref - scope 5

Diagnostics:
- (1,28-1,29) illegal shadowing of 'x' in nested local scope
```
