```ting
let DateTime = records(Year:int,Month:int,Day:int) in
DateTime this => $"{Year}-{Month}-{Day}"
```
```ting-output
Compilation succeeded.

Scopes:
[0] type=LocalScope usage=ambient id=00000000-0000-0000-0000-000000000000
    names=[bool<name>, string<name>, char<name>, float<name>, double<name>, int<name>, uint<name>, byte<name>, sbyte<name>, short<name>, ushort<name>, long<name>, ulong<name>, half<name>, decimal<name>, DateTime<name>, DateTimeOffset<name>, set<name>, any<name>, Func<name>, _all_<name>]
[1] type=LocalScope usage=local id=#0
    names=[DateTime<name>]
[2] type=LocalScope usage=record id=#1
    names=[Year<name>, Month<name>, Day<name>]
[3] type=LocalScope usage=local id=#2
    names=[<name>]

Identifiers:
[0] (1,5) name='DateTime' role=declarative bound-scopes=[#0]
[1] (1,14) name='_=_' role=referential(resolved) resolved-declaration=#3
[2] (1,24) name='Year' role=declarative bound-scopes=[#1]
[3] (1,28) name='_:_' role=referential(resolved) resolved-declaration=#4
[4] (1,29) name='int' role=referential(resolved) resolved-declaration=#5
[5] (1,33) name='Month' role=declarative bound-scopes=[#1]
[6] (1,38) name='_:_' role=referential(resolved) resolved-declaration=#4
[7] (1,39) name='int' role=referential(resolved) resolved-declaration=#5
[8] (1,43) name='Day' role=declarative bound-scopes=[#1]
[9] (1,46) name='_:_' role=referential(resolved) resolved-declaration=#4
[10] (1,47) name='int' role=referential(resolved) resolved-declaration=#5
[11] (1,52) name='_in_' role=referential(resolved) resolved-declaration=#6
[12] (2,1) name='DateTime' role=referential(resolved) resolved-declaration=#7
[13] (2,10) name='this' role=declarative bound-scopes=[#2]
[14] (2,15) name='_=>_' role=referential(resolved) resolved-declaration=#8
[15] (2,21) name='Year' role=referential(resolved) resolved-declaration=#9
[16] (2,28) name='Month' role=referential(resolved) resolved-declaration=#10
[17] (2,36) name='Day' role=referential(resolved) resolved-declaration=#11
```
