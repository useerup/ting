```ting
"zark" => (name->value)
```
```ting-output
Compilation succeeded.

Scopes:
[0] type=LocalScope usage=ambient id=00000000-0000-0000-0000-000000000000
    names=[bool<name>, string<name>, char<name>, float<name>, double<name>, int<name>, uint<name>, byte<name>, sbyte<name>, short<name>, ushort<name>, long<name>, ulong<name>, half<name>, decimal<name>, DateTime<name>, DateTimeOffset<name>, set<name>, any<name>, Func<name>, _all_<name>]
[1] type=LocalScope usage=local id=#0
    names=[]

Identifiers:
[0] (1,8) name='_=>_' role=referential(resolved) resolved-declaration=#1
[1] (1,12) name='name' role=referential candidate-scopes=[#0, 00000000-0000-0000-0000-000000000000]
[2] (1,16) name='_->_' role=referential(resolved) resolved-declaration=#2
[3] (1,18) name='value' role=referential candidate-scopes=[#0, 00000000-0000-0000-0000-000000000000]

Diagnostics:
- (1,12-1,16) unresolved name 'name'
- (1,18-1,23) unresolved name 'value'
```
