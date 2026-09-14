```ting
"zark" => (name->value)
```
```ting-output
Compilation failed.

Scopes:
[1] Local : bool, string, char, float, double, int, uint, byte, sbyte, short, ushort, long, ulong, half, decimal, DateTime, DateTimeOffset, set, any, Func, _all_
[2] Local : -_, _where_, _in_, _=_, _?=_, _then_, _else_, _contract_, _->_, _>>_, _<<_, _|>_, _<|_, _=>_, _catch_, _\_, _|_, _||_, _|||_, _&_, _&&_, _&&&_, _?_, _??_, !_, _:_, _::_, _:::_, _<_, _<=_, _>=_, _>_, _==_, _!=_, _!:_, _..._, _...<_, _<..._, _<...<_, _;_, _;;_, _^^_, _**_, _+_, _-_, _*_, _/_, _%_, _/%_, _^_, fn_, class_, total_, partial_
[3] Local :

Identifiers:
_=>_[1,8] ref - scope 2
name[1,12] ref - unresolved
_->_[1,16] ref - scope 2
value[1,18] ref - unresolved

Diagnostics:
- (1,12-1,16) unresolved name 'name'
- (1,18-1,23) unresolved name 'value'
```
