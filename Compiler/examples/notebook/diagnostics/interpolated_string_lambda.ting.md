```ting
$"{Word name} = {int value}" => (name->value)
```
```ting-output
Compilation failed.

Scopes:
[1] Local : bool, string, char, float, double, int, uint, byte, sbyte, short, ushort, long, ulong, half, decimal, DateTime, DateTimeOffset, set, any, Func, _all_
[2] Local : -_, _where_, _in_, _=_, _?=_, _then_, _else_, _contract_, _->_, _>>_, _<<_, _|>_, _<|_, _=>_, _catch_, _\_, _|_, _||_, _|||_, _&_, _&&_, _&&&_, _?_, _??_, !_, _:_, _::_, _:::_, _<_, _<=_, _>=_, _>_, _==_, _!=_, _!:_, _..._, _...<_, _<..._, _<...<_, _;_, _;;_, _^^_, _**_, _+_, _-_, _*_, _/_, _%_, _/%_, _^_, fn_, class_, total_, partial_
[3] Local : name, value

Identifiers:
Word[1,4] ref - unresolved
name[1,9] decl - scope 3
int[1,18] ref - scope 1
value[1,22] decl - scope 3
_=>_[1,30] ref - scope 2
name[1,34] ref - scope 3
_->_[1,38] ref - scope 2
value[1,40] ref - scope 3

Diagnostics:
- (1,4-1,8) unresolved name 'Word'
```
