```ting
fn{ 0 -> 1, int n -> n * 2 }
```

```ting-output
Compilation succeeded.

Scopes:
[1] Local : bool, string, char, float, double, int, uint, byte, sbyte, short, ushort, long, ulong, half, decimal, DateTime, DateTimeOffset, set, any, Func, _all_
[2] Local : -_, _where_, _in_, _=_, _?=_, _then_, _else_, _contract_, _->_, _>>_, _<<_, _|>_, _<|_, _=>_, _catch_, _\_, _|_, _||_, _|||_, _&_, _&&_, _&&&_, _?_, _??_, !_, _:_, _::_, _:::_, _<_, _<=_, _>=_, _>_, _==_, _!=_, _!:_, _..._, _...<_, _<..._, _<...<_, _;_, _;;_, _^^_, _**_, _+_, _-_, _*_, _/_, _%_, _/%_, _^_, fn_, class_, total_, partial_
[3] Local :
[4] Local : n

Identifiers:
_||_[1,1] ref - scope 2
_=>_[1,7] ref - scope 2
int[1,13] ref - scope 1
n[1,17] decl - scope 4
_=>_[1,19] ref - scope 2
n[1,22] ref - scope 4
_*_[1,24] ref - scope 2
```