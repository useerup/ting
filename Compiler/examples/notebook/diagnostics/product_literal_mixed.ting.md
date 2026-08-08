```ting
(1, 2 @ Result = 3)
```

```ting-output
Compilation succeeded.

Scopes:
[0] type=LocalScope usage=ambient id=00000000-0000-0000-0000-000000000000
    names=[bool<name>, string<name>, char<name>, float<name>, double<name>, int<name>, uint<name>, byte<name>, sbyte<name>, short<name>, ushort<name>, long<name>, ulong<name>, half<name>, decimal<name>, DateTime<name>, DateTimeOffset<name>, set<name>, any<name>, Func<name>, _all_<name>]
[1] type=LocalScope usage=product id=#0
    names=[Result<name>]

Identifiers:
[0] (1,9) name='Result' role=declarative bound-scopes=[#0]
[1] (1,16) name='_=_' role=referential(resolved) resolved-declaration=#1
```