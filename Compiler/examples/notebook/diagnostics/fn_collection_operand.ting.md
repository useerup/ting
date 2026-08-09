```ting
let mappings = { 1 -> 10, 2 -> 20 } in fn mappings
```

```ting-output
Compilation succeeded.

Scopes:
[1] Local : bool, string, char, float, double, int, uint, byte, sbyte, short, ushort, long, ulong, half, decimal, DateTime, DateTimeOffset, set, any, Func, _all_
[2] Local : -_, _where_, _in_, _=_, _?=_, _then_, _else_, _contract_, _->_, _>>_, _<<_, _|>_, _<|_, _=>_, _catch_, _\_, _|_, _||_, _|||_, _&_, _&&_, _&&&_, _?_, _??_, !_, _:_, _::_, _:::_, _<_, _<=_, _>=_, _>_, _==_, _!=_, _!:_, _..._, _...<_, _<..._, _<...<_, _;_, _;;_, _^^_, _**_, _+_, _-_, _*_, _/_, _%_, _/%_, _^_, fn_, class_, total_, partial_
[3] Local : mappings

Identifiers:
mappings[1,5] decl - scope 3
_=_[1,14] ref - scope 2
_->_[1,20] ref - scope 2
_->_[1,29] ref - scope 2
_in_[1,37] ref - scope 2
fn_[1,40] ref - scope 2
mappings[1,43] ref - scope 3
```