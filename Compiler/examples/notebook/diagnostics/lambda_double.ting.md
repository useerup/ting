```ting
x => x * 2m
```
```ting-output
Compilation succeeded.

Scopes:
[0] type=LocalScope usage=ambient id=00000000-0000-0000-0000-000000000000
    names=[bool<name>, string<name>, char<name>, float<name>, double<name>, int<name>, uint<name>, byte<name>, sbyte<name>, short<name>, ushort<name>, long<name>, ulong<name>, half<name>, decimal<name>, DateTime<name>, DateTimeOffset<name>, set<name>, any<name>, Func<name>, _all_<name>]
[1] type=LocalScope usage=local id=#0
    names=[x<name>]

Identifiers:
[0] (1,1) name='x' role=declarative bound-scopes=[#0]
[1] (1,3) name='_=>_' role=referential(resolved) resolved-declaration=#1
[2] (1,6) name='x' role=referential(resolved) resolved-declaration=#2
[3] (1,8) name='_*_' role=referential(resolved) resolved-declaration=#3
```
