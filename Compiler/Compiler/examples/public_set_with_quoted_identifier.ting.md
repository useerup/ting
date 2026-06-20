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
        (Declarative (c53031b5-e8d7-4fa3-98b3-1a34b09ea13a, [TextPart "a"]))])

Scopes:
[0] NamesScope           38c62d20-22b9-4b11-a28b-19bedcdf5587
    names=[a<name>]
```
