```ting
let f = a => b => a - b in f -1 -2
```

```ting-output
Compilation succeeded.

Scopes:
[0] type=LocalScope usage=ambient id=00000000-0000-0000-0000-000000000000
    names=[bool<name>, string<name>, char<name>, float<name>, double<name>, int<name>, uint<name>, byte<name>, sbyte<name>, short<name>, ushort<name>, long<name>, ulong<name>, half<name>, decimal<name>, DateTime<name>, DateTimeOffset<name>, set<name>, any<name>, Func<name>, _all_<name>]
[1] type=LocalScope usage=local id=#0
    names=[f<name>]
[2] type=LocalScope usage=local id=#1
    names=[a<name>]
[3] type=LocalScope usage=local id=#2
    names=[b<name>]

Identifiers:
[0] (1,5) name='f' role=declarative bound-scopes=[#0]
[1] (1,7) name='_=_' role=referential(resolved) resolved-declaration=#3
[2] (1,9) name='a' role=declarative bound-scopes=[#1]
[3] (1,11) name='_=>_' role=referential(resolved) resolved-declaration=#4
[4] (1,14) name='b' role=declarative bound-scopes=[#2]
[5] (1,16) name='_=>_' role=referential(resolved) resolved-declaration=#4
[6] (1,19) name='a' role=referential(resolved) resolved-declaration=#5
[7] (1,21) name='_-_' role=referential(resolved) resolved-declaration=#6
[8] (1,23) name='b' role=referential(resolved) resolved-declaration=#7
[9] (1,25) name='_in_' role=referential(resolved) resolved-declaration=#8
[10] (1,28) name='f' role=referential(resolved) resolved-declaration=#9
[11] (1,30) name='-_' role=referential(resolved) resolved-declaration=#10
[12] (1,33) name='-_' role=referential(resolved) resolved-declaration=#10
```