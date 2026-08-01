```ting
(@ Name = "Bob", Age = 42)
```

```ting-output
Compilation succeeded.

(@[#0] (id(resolved:#1) (id(decl:#2), "Bob")), (id(resolved:#1) (id(decl:#3), 42)))

Scopes:
[0] NamesScope           #0
    names=[Name<name>, Age<name>]

Value:
(@ Name = "Bob", Age = 42)
```