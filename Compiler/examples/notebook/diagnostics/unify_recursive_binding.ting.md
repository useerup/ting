```ting
let f ?= fn{ 0 -> 1, int n -> n * f (n - 1) } in f 3
```
```ting-output
Compilation succeeded.

Scopes:
[1] Local : bool, string, char, float, double, int, uint, byte, sbyte, short, ushort, long, ulong, half, decimal, DateTime, DateTimeOffset, set, any, Func, _all_
[2] Local : -_, _where_, _in_, _=_, _?=_, _then_, _else_, _contract_, _->_, _>>_, _<<_, _|>_, _<|_, _=>_, _catch_, _\_, _|_, _||_, _|||_, _&_, _&&_, _&&&_, _?_, _??_, !_, _:_, _::_, _:::_, _<_, _<=_, _>=_, _>_, _==_, _!=_, _!:_, _..._, _...<_, _<..._, _<...<_, _;_, _;;_, _^^_, _**_, _+_, _-_, _*_, _/_, _%_, _/%_, _^_, fn_, class_, total_, partial_
[3] Local : f
[4] Local :
[5] Local : n

Identifiers:
_||_[1,1] ref - scope 2
f[1,5] decl - scope 3
_?=_[1,7] ref - scope 2
_=>_[1,16] ref - scope 2
int[1,22] ref - scope 1
n[1,26] decl - scope 5
_=>_[1,28] ref - scope 2
n[1,31] ref - scope 5
_*_[1,33] ref - scope 2
f[1,35] ref - scope 3
n[1,38] ref - scope 5
_-_[1,40] ref - scope 2
_in_[1,47] ref - scope 2
f[1,50] ref - scope 3
```