```ting
let pair = (1, 2) in (pair @@ Result = 3)
```

```ting-output
Compilation succeeded.

Scopes:
[0] type=LocalScope usage=ambient id=00000000-0000-0000-0000-000000000000
    names=[bool<name>, string<name>, char<name>, float<name>, double<name>, int<name>, uint<name>, byte<name>, sbyte<name>, short<name>, ushort<name>, long<name>, ulong<name>, half<name>, decimal<name>, DateTime<name>, DateTimeOffset<name>, set<name>, any<name>, Func<name>, _all_<name>]
[1] type=LocalScope usage=local id=#0
    names=[pair<name>]
[2] type=LocalScope usage=product id=#1
    names=[Result<name>]

Identifiers:
[0] (1,5) name='pair' role=declarative bound-scopes=[#0]
[1] (1,10) name='_=_' role=referential(resolved) resolved-declaration=#2
[2] (1,19) name='_in_' role=referential(resolved) resolved-declaration=#3
[3] (1,23) name='pair' role=referential(resolved) resolved-declaration=#4
[4] (1,31) name='Result' role=declarative bound-scopes=[#1]
[5] (1,38) name='_=_' role=referential(resolved) resolved-declaration=#2
```