```ting
record(Answer:int) \ (Answer = 42)
```
```ting-output
Compilation succeeded.

Scopes:
[1] Local : bool, string, char, float, double, int, uint, byte, sbyte, short, ushort, long, ulong, half, decimal, DateTime, DateTimeOffset, set, any, Func, _all_
[2] Local : -_, _where_, _in_, _=_, _?=_, _then_, _else_, _contract_, _->_, _>>_, _<<_, _|>_, _<|_, _=>_, _catch_, _\_, _|_, _||_, _|||_, _&_, _&&_, _&&&_, _?_, _??_, !_, _:_, _::_, _:::_, _<_, _<=_, _>=_, _>_, _==_, _!=_, _!:_, _..._, _...<_, _<..._, _<...<_, _;_, _;;_, _^^_, _**_, _+_, _-_, _*_, _/_, _%_, _/%_, _^_, fn_, class_, total_, partial_
[3] Local : Answer
[4] Record : Answer

Identifiers:
Answer[1,8] decl - scope 3, 4
_:_[1,14] ref - scope 2
int[1,15] ref - scope 1
_\_[1,20] ref - scope 2
Answer[1,23] ref - scope 3, 4
_=_[1,30] ref - scope 2
```
