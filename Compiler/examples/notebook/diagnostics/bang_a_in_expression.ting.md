```ting
!a in b + c * d
```
```ting-output
Compilation succeeded.

Scopes:
[0] type=LocalScope usage=ambient id=00000000-0000-0000-0000-000000000000
    names=[bool<name>, string<name>, char<name>, float<name>, double<name>, int<name>, uint<name>, byte<name>, sbyte<name>, short<name>, ushort<name>, long<name>, ulong<name>, half<name>, decimal<name>, DateTime<name>, DateTimeOffset<name>, set<name>, any<name>, Func<name>, _all_<name>]

Identifiers:
[0] (1,1) name='!_' role=referential(resolved) resolved-declaration=#0
[1] (1,2) name='a' role=referential candidate-scopes=[00000000-0000-0000-0000-000000000000]
[2] (1,4) name='_in_' role=referential(resolved) resolved-declaration=#1
[3] (1,7) name='b' role=referential candidate-scopes=[00000000-0000-0000-0000-000000000000]
[4] (1,9) name='_+_' role=referential(resolved) resolved-declaration=#2
[5] (1,11) name='c' role=referential candidate-scopes=[00000000-0000-0000-0000-000000000000]
[6] (1,13) name='_*_' role=referential(resolved) resolved-declaration=#3
[7] (1,15) name='d' role=referential candidate-scopes=[00000000-0000-0000-0000-000000000000]

Diagnostics:
- (1,2-1,3) unresolved name 'a'
- (1,7-1,8) unresolved name 'b'
- (1,11-1,12) unresolved name 'c'
- (1,15-1,16) unresolved name 'd'
```
