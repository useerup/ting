```ting
public { 1, 2, 3, `a` }
```
```ting-output
Input:
public { 1, 2, 3, `a` }

Compilation succeeded.

CoreExp:
Publish
  (CoreExp+LocalScope, Public,
   Set
     [Literal (Int32 1); Literal (Int32 2); Literal (Int32 3);
      Identifier
        (Declarative (d48147e3-14d6-4988-a28c-09b5abe6aa85, [TextPart "a"]))])

Scopes:
[0] NamesScope           33ab31d4-326e-4021-b853-dfb25625383d
    names=[a<name>]
```
