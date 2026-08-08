```ting
// A line comment before the expression.
let x = 1 /* a block comment /* nested */ inside an expression */ + 2 in
    x // and one at the end of a line
```

```ting-output
Compilation succeeded.

Scopes:
[0] type=LocalScope usage=ambient id=00000000-0000-0000-0000-000000000000
    names=[bool<name>, string<name>, char<name>, float<name>, double<name>, int<name>, uint<name>, byte<name>, sbyte<name>, short<name>, ushort<name>, long<name>, ulong<name>, half<name>, decimal<name>, DateTime<name>, DateTimeOffset<name>, set<name>, any<name>, Func<name>, _all_<name>]
[1] type=LocalScope usage=local id=#0
    names=[x<name>]

Identifiers:
[0] (2,5) name='x' role=declarative bound-scopes=[#0]
[1] (2,7) name='_=_' role=referential(resolved) resolved-declaration=#1
[2] (2,67) name='_+_' role=referential(resolved) resolved-declaration=#2
[3] (2,71) name='_in_' role=referential(resolved) resolved-declaration=#3
[4] (3,5) name='x' role=referential(resolved) resolved-declaration=#4
```