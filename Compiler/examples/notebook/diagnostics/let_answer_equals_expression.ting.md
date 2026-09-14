```ting
let x=21 in Answer = x*2
```
```ting-output
Compilation failed.

Scopes:
[1] Local : bool, string, char, float, double, int, uint, byte, sbyte, short, ushort, long, ulong, half, decimal, DateTime, DateTimeOffset, set, any, Func, _all_
[2] Local : -_, _where_, _in_, _=_, _?=_, _then_, _else_, _contract_, _->_, _>>_, _<<_, _|>_, _<|_, _=>_, _catch_, _\_, _|_, _||_, _|||_, _&_, _&&_, _&&&_, _?_, _??_, !_, _:_, _::_, _:::_, _<_, _<=_, _>=_, _>_, _==_, _!=_, _!:_, _..._, _...<_, _<..._, _<...<_, _;_, _;;_, _^^_, _**_, _+_, _-_, _*_, _/_, _%_, _/%_, _^_, fn_, class_, total_, partial_
[3] Local : x

Identifiers:
x[1,5] decl - scope 3
_=_[1,6] ref - scope 2
_in_[1,10] ref - scope 2
Answer[1,13] ref - unresolved
_=_[1,20] ref - scope 2
x[1,22] ref - scope 3
_*_[1,23] ref - scope 2

Diagnostics:
- (1,13-1,19) unresolved name 'Answer'
```
