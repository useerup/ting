```ting
// A line comment before the expression.
let x = 1 /* a block comment /* nested */ inside an expression */ + 2 in
    x // and one at the end of a line
```

```ting-output
Compilation succeeded.

Scopes:
[1] Local : bool, string, char, float, double, int, uint, byte, sbyte, short, ushort, long, ulong, half, decimal, DateTime, DateTimeOffset, set, any, Func, _all_
[2] Local : -_, _where_, _in_, _=_, _?=_, _then_, _else_, _contract_, _->_, _>>_, _<<_, _|>_, _<|_, _=>_, _catch_, _\_, _|_, _||_, _|||_, _&_, _&&_, _&&&_, _?_, _??_, !_, _:_, _::_, _:::_, _<_, _<=_, _>=_, _>_, _==_, _!=_, _!:_, _..._, _...<_, _<..._, _<...<_, _;_, _;;_, _^^_, _**_, _+_, _-_, _*_, _/_, _%_, _/%_, _^_, fn_, class_, total_, partial_
[3] Local : x

Identifiers:
x[2,5] decl - scope 3
_=_[2,7] ref - scope 2
_+_[2,67] ref - scope 2
_in_[2,71] ref - scope 2
x[3,5] ref - scope 3
```