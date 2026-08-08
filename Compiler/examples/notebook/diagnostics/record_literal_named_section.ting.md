```ting
(@ Name = "Bob", Age = 42)
```

```ting-output
Compilation succeeded.

Scopes:
[0] NamesScope           #0
    names=[Name<name>, Age<name>]

Identifiers:
[0] (1,4) name='Name' role=declarative bound-scopes=[#0]
[1] (1,9) name='_=_' role=referential(resolved) resolved-declaration=#1
[2] (1,18) name='Age' role=declarative bound-scopes=[#0]
[3] (1,22) name='_=_' role=referential(resolved) resolved-declaration=#1
```