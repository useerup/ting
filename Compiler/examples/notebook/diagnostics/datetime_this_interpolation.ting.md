```ting
let DateTime = records(Year:int,Month:int,Day:int) in
DateTime this => $"{Year}-{Month}-{Day}"
```
```ting-output
Compilation succeeded.

Scopes:
[0] NamesScope           #0
    names=[DateTime<name>]
[1] NamesScope           #1
    names=[Year<name>, Month<name>, Day<name>]
[2] NamesScope           #2
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
[13] (2,15) name='_=>_' role=referential(resolved) resolved-declaration=#8
[14] (2,21) name='Year' role=referential(resolved) resolved-declaration=#9
[15] (2,28) name='Month' role=referential(resolved) resolved-declaration=#10
[16] (2,36) name='Day' role=referential(resolved) resolved-declaration=#11
```
