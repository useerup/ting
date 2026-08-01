```ting
fn{ 0 -> 1, int n -> n * 2 }
```

```ting-output
Compilation succeeded.

(id(resolved:#0) (local[#1] (id(resolved:#2) (0, 1)), local[#3] (id(resolved:#2) ((id(resolved:#4) id(decl:#5)), (id(resolved:#6) (id(resolved:#5), 2))))))

Scopes:
[0] NamesScope           #1
    names=[]
[1] NamesScope           #3
    names=[n<name>]
```