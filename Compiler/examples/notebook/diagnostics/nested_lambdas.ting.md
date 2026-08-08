```ting
x => x + y => x + y + z => x
```
```ting-output
Compilation succeeded.

Scopes:
[0] type=LocalScope usage=ambient id=00000000-0000-0000-0000-000000000000
    names=[bool<name>, string<name>, char<name>, float<name>, double<name>, int<name>, uint<name>, byte<name>, sbyte<name>, short<name>, ushort<name>, long<name>, ulong<name>, half<name>, decimal<name>, DateTime<name>, DateTimeOffset<name>, set<name>, any<name>, Func<name>, _all_<name>]
[1] type=LocalScope usage=local id=#0
    names=[x<name>]
[2] type=LocalScope usage=local id=#1
    names=[x<name>, y<name>]
[3] type=LocalScope usage=local id=#2
    names=[x<name>, y<name>, z<name>]

Identifiers:
[0] (1,1) name='x' role=declarative bound-scopes=[#0]
[1] (1,3) name='_=>_' role=referential(resolved) resolved-declaration=#3
[2] (1,6) name='x' role=declarative bound-scopes=[#1]
[3] (1,8) name='_+_' role=referential(resolved) resolved-declaration=#4
[4] (1,10) name='y' role=declarative bound-scopes=[#1]
[5] (1,12) name='_=>_' role=referential(resolved) resolved-declaration=#3
[6] (1,15) name='x' role=declarative bound-scopes=[#2]
[7] (1,17) name='_+_' role=referential(resolved) resolved-declaration=#4
[8] (1,19) name='y' role=declarative bound-scopes=[#2]
[9] (1,21) name='_+_' role=referential(resolved) resolved-declaration=#4
[10] (1,23) name='z' role=declarative bound-scopes=[#2]
[11] (1,25) name='_=>_' role=referential(resolved) resolved-declaration=#3
[12] (1,28) name='x' role=referential(resolved) resolved-declaration=#5

Diagnostics:
- (1,28-1,29) illegal shadowing of 'x' in nested local scope
```
