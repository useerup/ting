```ting
let x = 1 in x ?? throw x
```

```ting-output
Compilation succeeded.

Scopes:
[1] Local : bool, string, char, float, double, int, uint, byte, sbyte, short, ushort, long, ulong, half, decimal, DateTime, DateTimeOffset, set, any, Func, _all_
[2] Local : -_, _where_, _in_, _=_, _?=_, _then_, _else_, _contract_, _->_, _>>_, _<<_, _|>_, _<|_, _=>_, _catch_, _\_, _|_, _||_, _|||_, _&_, _&&_, _&&&_, _?_, _??_, !_, _:_, _::_, _:::_, _<_, _<=_, _>=_, _>_, _==_, _!=_, _!:_, _..._, _...<_, _<..._, _<...<_, _;_, _;;_, _^^_, _**_, _+_, _-_, _*_, _/_, _%_, _/%_, _^_, fn_, class_, total_, partial_
[3] Local : x

Identifiers:
x[1,5] decl - scope 3
_=_[1,7] ref - scope 2
_in_[1,11] ref - scope 2
x[1,14] ref - scope 3
_??_[1,16] ref - scope 2
x[1,25] ref - scope 3
```