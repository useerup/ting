```ting
record(Answer:int) \ (Answer = 42)
```
```ting-output
Compilation succeeded.

Scopes:
[0] type=LocalScope usage=ambient id=00000000-0000-0000-0000-000000000000
    names=[bool<name>, string<name>, char<name>, float<name>, double<name>, int<name>, uint<name>, byte<name>, sbyte<name>, short<name>, ushort<name>, long<name>, ulong<name>, half<name>, decimal<name>, DateTime<name>, DateTimeOffset<name>, set<name>, any<name>, Func<name>, _all_<name>]
[1] type=LocalScope usage=local id=#0
    names=[Answer<name>]
[2] type=LocalScope usage=record id=#1
    names=[Answer<name>]

Identifiers:
[0] (1,8) name='Answer' role=declarative bound-scopes=[#0, #1]
[1] (1,14) name='_:_' role=referential(resolved) resolved-declaration=#2
[2] (1,15) name='int' role=referential(resolved) resolved-declaration=#3
[3] (1,20) name='_\_' role=referential(resolved) resolved-declaration=#4
[4] (1,23) name='Answer' role=referential(resolved) resolved-declaration=#5
[5] (1,30) name='_=_' role=referential(resolved) resolved-declaration=#6
```
