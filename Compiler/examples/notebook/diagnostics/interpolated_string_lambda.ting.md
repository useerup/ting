```ting
$"{Word name} = {int value}" => (name->value)
```
```ting-output
Compilation succeeded.

Scopes:
[0] type=LocalScope usage=ambient id=00000000-0000-0000-0000-000000000000
    names=[bool<name>, string<name>, char<name>, float<name>, double<name>, int<name>, uint<name>, byte<name>, sbyte<name>, short<name>, ushort<name>, long<name>, ulong<name>, half<name>, decimal<name>, DateTime<name>, DateTimeOffset<name>, set<name>, any<name>, Func<name>, _all_<name>]
[1] type=LocalScope usage=local id=#0
    names=[name<name>, value<name>]

Identifiers:
[0] (1,4) name='Word' role=referential candidate-scopes=[#0, 00000000-0000-0000-0000-000000000000]
[1] (1,9) name='name' role=declarative bound-scopes=[#0]
[2] (1,18) name='int' role=referential(resolved) resolved-declaration=#1
[3] (1,22) name='value' role=declarative bound-scopes=[#0]
[4] (1,30) name='_=>_' role=referential(resolved) resolved-declaration=#2
[5] (1,34) name='name' role=referential(resolved) resolved-declaration=#3
[6] (1,38) name='_->_' role=referential(resolved) resolved-declaration=#4
[7] (1,40) name='value' role=referential(resolved) resolved-declaration=#5

Diagnostics:
- (1,4-1,8) unresolved name 'Word'
```
