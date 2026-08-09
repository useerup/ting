```ting
let f = a => b => a - b in f -1 -2
```

```ting-output
Compilation succeeded.

Scopes:
[1] Local : bool, string, char, float, double, int, uint, byte, sbyte, short, ushort, long, ulong, half, decimal, DateTime, DateTimeOffset, set, any, Func, _all_
[2] Local : -_, _where_, _in_, _=_, _?=_, _then_, _else_, _contract_, _->_, _>>_, _<<_, _|>_, _<|_, _=>_, _catch_, _\_, _|_, _||_, _|||_, _&_, _&&_, _&&&_, _?_, _??_, !_, _:_, _::_, _:::_, _<_, _<=_, _>=_, _>_, _==_, _!=_, _!:_, _..._, _...<_, _<..._, _<...<_, _;_, _;;_, _^^_, _**_, _+_, _-_, _*_, _/_, _%_, _/%_, _^_, fn_, class_, total_, partial_
[3] Local : f
[4] Local : a
[5] Local : b

Identifiers:
f[1,5] decl - scope 3
_=_[1,7] ref - scope 2
a[1,9] decl - scope 4
_=>_[1,11] ref - scope 2
b[1,14] decl - scope 5
_=>_[1,16] ref - scope 2
a[1,19] ref - scope 4
_-_[1,21] ref - scope 2
b[1,23] ref - scope 5
_in_[1,25] ref - scope 2
f[1,28] ref - scope 3
-_[1,30] ref - scope 2
-_[1,33] ref - scope 2
```