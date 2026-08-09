```ting
a==2 then a==3 then 0 else 1 else 2
```
```ting-output
Compilation succeeded.

Scopes:
[1] Local : bool, string, char, float, double, int, uint, byte, sbyte, short, ushort, long, ulong, half, decimal, DateTime, DateTimeOffset, set, any, Func, _all_
[2] Local : -_, _where_, _in_, _=_, _?=_, _then_, _else_, _contract_, _->_, _>>_, _<<_, _|>_, _<|_, _=>_, _catch_, _\_, _|_, _||_, _|||_, _&_, _&&_, _&&&_, _?_, _??_, !_, _:_, _::_, _:::_, _<_, _<=_, _>=_, _>_, _==_, _!=_, _!:_, _..._, _...<_, _<..._, _<...<_, _;_, _;;_, _^^_, _**_, _+_, _-_, _*_, _/_, _%_, _/%_, _^_, fn_, class_, total_, partial_

Identifiers:
a[1,1] ref - unresolved
_==_[1,2] ref - scope 2
_then_[1,6] ref - scope 2
a[1,11] ref - unresolved
_==_[1,12] ref - scope 2
_then_[1,16] ref - scope 2
_else_[1,23] ref - scope 2
_else_[1,30] ref - scope 2

Diagnostics:
- (1,1-1,2) unresolved name 'a'
- (1,11-1,12) unresolved name 'a'
```
