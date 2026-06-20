```ting
person.(Name,Age)
```
```ting-output
Input:
person.(Name,Age)

Compilation succeeded.

CoreExp:
Projection
  (CoreExp+LocalScope,
   Identifier
     (Referential ([00000000-0000-0000-0000-000000000000], [TextPart "person"])),
   Tuple
     [Identifier
        (Referential ([00000000-0000-0000-0000-000000000000], [TextPart "Name"]));
      Identifier
        (Referential ([00000000-0000-0000-0000-000000000000], [TextPart "Age"]))])

Scopes:
[0] NamesScope           07ac30aa-fab7-4aa7-b76a-fc76ba15ed45
    names=[]
```
