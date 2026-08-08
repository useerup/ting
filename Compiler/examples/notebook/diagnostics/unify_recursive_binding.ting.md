```ting
let f ?= fn{ 0 -> 1, int n -> n * f (n - 1) } in f 3
```
```ting-output
Compilation succeeded.

Scopes:
[0] type=LocalScope usage=ambient id=00000000-0000-0000-0000-000000000000
    names=[bool<name>, string<name>, char<name>, float<name>, double<name>, int<name>, uint<name>, byte<name>, sbyte<name>, short<name>, ushort<name>, long<name>, ulong<name>, half<name>, decimal<name>, DateTime<name>, DateTimeOffset<name>, set<name>, any<name>, Func<name>, _all_<name>]
[1] type=LocalScope usage=local id=#0
    names=[f<name>]
[2] type=LocalScope usage=local id=#1
    names=[]
[3] type=LocalScope usage=local id=#2
    names=[n<name>]

Identifiers:
[0] (1,1) name='_||_' role=referential(resolved) resolved-declaration=#3
[1] (1,5) name='f' role=declarative bound-scopes=[#0]
[2] (1,7) name='_?=_' role=referential(resolved) resolved-declaration=#4
[3] (1,16) name='_=>_' role=referential(resolved) resolved-declaration=#5
[4] (1,22) name='int' role=referential(resolved) resolved-declaration=#6
[5] (1,26) name='n' role=declarative bound-scopes=[#2]
[6] (1,28) name='_=>_' role=referential(resolved) resolved-declaration=#5
[7] (1,31) name='n' role=referential(resolved) resolved-declaration=#7
[8] (1,33) name='_*_' role=referential(resolved) resolved-declaration=#8
[9] (1,35) name='f' role=referential(resolved) resolved-declaration=#9
[10] (1,38) name='n' role=referential(resolved) resolved-declaration=#7
[11] (1,40) name='_-_' role=referential(resolved) resolved-declaration=#10
[12] (1,47) name='_in_' role=referential(resolved) resolved-declaration=#11
[13] (1,50) name='f' role=referential(resolved) resolved-declaration=#9
```