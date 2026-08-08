```ting
let a = { 1, 2 } in let b = { 2, 3 } in (a &&& b) ||| a
```

```ting-output
Compilation succeeded.

Scopes:
[0] type=LocalScope usage=ambient id=00000000-0000-0000-0000-000000000000
    names=[bool<name>, string<name>, char<name>, float<name>, double<name>, int<name>, uint<name>, byte<name>, sbyte<name>, short<name>, ushort<name>, long<name>, ulong<name>, half<name>, decimal<name>, DateTime<name>, DateTimeOffset<name>, set<name>, any<name>, Func<name>, _all_<name>]
[1] type=LocalScope usage=local id=#0
    names=[a<name>]
[2] type=LocalScope usage=local id=#1
    names=[b<name>]

Identifiers:
[0] (1,5) name='a' role=declarative bound-scopes=[#0]
[1] (1,7) name='_=_' role=referential(resolved) resolved-declaration=#2
[2] (1,18) name='_in_' role=referential(resolved) resolved-declaration=#3
[3] (1,25) name='b' role=declarative bound-scopes=[#1]
[4] (1,27) name='_=_' role=referential(resolved) resolved-declaration=#2
[5] (1,38) name='_in_' role=referential(resolved) resolved-declaration=#3
[6] (1,42) name='a' role=referential(resolved) resolved-declaration=#4
[7] (1,44) name='_&&&_' role=referential(resolved) resolved-declaration=#5
[8] (1,48) name='b' role=referential(resolved) resolved-declaration=#6
[9] (1,51) name='_|||_' role=referential(resolved) resolved-declaration=#7
[10] (1,55) name='a' role=referential(resolved) resolved-declaration=#4
```