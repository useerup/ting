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
[0] NamesScope           b9d49984-a751-4218-a7fc-51ef4a8d6130
    names=[]
```
