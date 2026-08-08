```ting
let answer=42 in $`Flollop{answer}`
```
```ting-output
Compilation succeeded.

Scopes:
[0] type=LocalScope usage=ambient id=00000000-0000-0000-0000-000000000000
    names=[bool<name>, string<name>, char<name>, float<name>, double<name>, int<name>, uint<name>, byte<name>, sbyte<name>, short<name>, ushort<name>, long<name>, ulong<name>, half<name>, decimal<name>, DateTime<name>, DateTimeOffset<name>, set<name>, any<name>, Func<name>, _all_<name>]
[1] type=LocalScope usage=local id=#0
    names=[answer<name>]

Identifiers:
[0] (1,5) name='answer' role=declarative bound-scopes=[#0]
[1] (1,11) name='_=_' role=referential(resolved) resolved-declaration=#1
[2] (1,15) name='_in_' role=referential(resolved) resolved-declaration=#2
[3] (1,18) name='Flollop{...}' role=referential candidate-scopes=[#0, 00000000-0000-0000-0000-000000000000]
```
