```ting
internal public { 1, 2, 3, a }
```
```ting-output
Compilation succeeded.

Publish
  (CoreExp+LocalScope, Internal,
   Publish
     (CoreExp+LocalScope, Public,
      Set
        [Literal (Int32 1); Literal (Int32 2); Literal (Int32 3);
         Identifier
           (Declarative (#0, [TextPart "a"]))]))

Scopes:
[0] NamesScope           #1
    names=[a<name>]
[1] NamesScope           #2
    names=[a<name>]
```
