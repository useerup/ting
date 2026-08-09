```ting
let a = { 1, 2 } in let b = { 2, 3 } in (a &&& b) ||| a
```

```ting-output
Compilation succeeded.

Scopes:
[1] Local : bool, string, char, float, double, int, uint, byte, sbyte, short, ushort, long, ulong, half, decimal, DateTime, DateTimeOffset, set, any, Func, _all_
[2] Local : -_, _where_, _in_, _=_, _?=_, _then_, _else_, _contract_, _->_, _>>_, _<<_, _|>_, _<|_, _=>_, _catch_, _\_, _|_, _||_, _|||_, _&_, _&&_, _&&&_, _?_, _??_, !_, _:_, _::_, _:::_, _<_, _<=_, _>=_, _>_, _==_, _!=_, _!:_, _..._, _...<_, _<..._, _<...<_, _;_, _;;_, _^^_, _**_, _+_, _-_, _*_, _/_, _%_, _/%_, _^_, fn_, class_, total_, partial_
[3] Local : a
[4] Local : b

Identifiers:
a[1,5] decl - scope 3
_=_[1,7] ref - scope 2
_in_[1,18] ref - scope 2
b[1,25] decl - scope 4
_=_[1,27] ref - scope 2
_in_[1,38] ref - scope 2
a[1,42] ref - scope 3
_&&&_[1,44] ref - scope 2
b[1,48] ref - scope 4
_|||_[1,51] ref - scope 2
a[1,55] ref - scope 3
```