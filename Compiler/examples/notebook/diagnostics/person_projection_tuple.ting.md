```ting
person.(Name,Age)
```
```ting-output
Compilation succeeded.

Projection
  (Exp+LocalScope,
   Identifier
     (Referential ([00000000-0000-0000-0000-000000000000], [TextPart "person"])),
   Tuple
     [Identifier
        (Referential ([00000000-0000-0000-0000-000000000000], [TextPart "Name"]));
      Identifier
        (Referential ([00000000-0000-0000-0000-000000000000], [TextPart "Age"]))])

Scopes:
[0] NamesScope           #0
    names=[]

Diagnostics:
- unresolved name 'person'
- unresolved name 'Name'
- unresolved name 'Age'
```
