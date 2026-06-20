```ting
internal public { 1, 2, 3, a }
```
```ting-output
Input:
internal public { 1, 2, 3, a }

Compilation succeeded.

CoreExp:
Publish
  (CoreExp+LocalScope, Internal,
   Publish
     (CoreExp+LocalScope, Public,
      Set
        [Literal (Int32 1); Literal (Int32 2); Literal (Int32 3);
         Identifier
           (Declarative (b4cddee5-2e2b-46bb-9eb5-3a18a865f5e0, [TextPart "a"]))]))

Scopes:
[0] NamesScope           8a86980f-fcfb-4536-a0c6-9d800144b206
    names=[a<name>]
[1] NamesScope           a0cec9bf-637a-41c7-a441-c94a3cf43c42
    names=[a<name>]
```
