```ting
protected internal { 1, 2, 3, a }
```
```ting-output
Input:
protected internal { 1, 2, 3, a }

Compilation succeeded.

CoreExp:
Publish
  (CoreExp+LocalScope, Protected,
   Publish
     (CoreExp+LocalScope, Internal,
      Set
        [Literal (Int32 1); Literal (Int32 2); Literal (Int32 3);
         Identifier
           (Declarative (07470c5c-7c80-4a71-9461-3ba020d0e3ce, [TextPart "a"]))]))

Scopes:
[0] NamesScope           ecfcd769-cbd2-4942-8805-f1694c1378e2
    names=[a<name>]
[1] NamesScope           36fb1e6c-870d-4f0e-b0c7-32153f9de46f
    names=[a<name>]
```
