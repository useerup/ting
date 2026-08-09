```ting
let DateTime = records(Year:int,Month:int,Day:int) in
DateTime this => $"{Year}-{Month}-{Day}"
```
```ting-output
Compilation succeeded.

Scopes:
[1] Local : bool, string, char, float, double, int, uint, byte, sbyte, short, ushort, long, ulong, half, decimal, DateTime, DateTimeOffset, set, any, Func, _all_
[2] Local : -_, _where_, _in_, _=_, _?=_, _then_, _else_, _contract_, _->_, _>>_, _<<_, _|>_, _<|_, _=>_, _catch_, _\_, _|_, _||_, _|||_, _&_, _&&_, _&&&_, _?_, _??_, !_, _:_, _::_, _:::_, _<_, _<=_, _>=_, _>_, _==_, _!=_, _!:_, _..._, _...<_, _<..._, _<...<_, _;_, _;;_, _^^_, _**_, _+_, _-_, _*_, _/_, _%_, _/%_, _^_, fn_, class_, total_, partial_
[3] Local : DateTime
[4] Record : Year, Month, Day
[5] Local :

Identifiers:
DateTime[1,5] decl - scope 3
_=_[1,14] ref - scope 2
Year[1,24] decl - scope 4
_:_[1,28] ref - scope 2
int[1,29] ref - scope 1
Month[1,33] decl - scope 4
_:_[1,38] ref - scope 2
int[1,39] ref - scope 1
Day[1,43] decl - scope 4
_:_[1,46] ref - scope 2
int[1,47] ref - scope 1
_in_[1,52] ref - scope 2
DateTime[2,1] ref - scope 3
this[2,10] decl - scope 5
_=>_[2,15] ref - scope 2
Year[2,21] ref - scope 4
Month[2,28] ref - scope 4
Day[2,36] ref - scope 4
```
