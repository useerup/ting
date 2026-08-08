```ting
`then` = 42
```
```ting-output
Compilation succeeded.

Scopes:
[0] type=LocalScope usage=ambient id=00000000-0000-0000-0000-000000000000
    names=[bool<name>, string<name>, char<name>, float<name>, double<name>, int<name>, uint<name>, byte<name>, sbyte<name>, short<name>, ushort<name>, long<name>, ulong<name>, half<name>, decimal<name>, DateTime<name>, DateTimeOffset<name>, set<name>, any<name>, Func<name>, _all_<name>]

Identifiers:
[0] (1,1) name='then' role=referential candidate-scopes=[00000000-0000-0000-0000-000000000000]
[1] (1,8) name='_=_' role=referential(resolved) resolved-declaration=#0

Diagnostics:
- (1,1-1,7) unresolved name 'then'
```
