```ting
person.Name = "Zaphod"
```
```ting-output
Input:
person.Name = "Zaphod"

Compilation succeeded.

CoreExp:
Apply
  (Identifier
     (Referential ([00000000-0000-0000-0000-000000000000], [TextPart "_=_"])),
   Tuple
     [Projection
        (CoreExp+LocalScope,
         Identifier
           (Referential
              ([00000000-0000-0000-0000-000000000000], [TextPart "person"])),
         Identifier
           (Referential
              ([00000000-0000-0000-0000-000000000000], [TextPart "Name"])));
      Literal (String "Zaphod")])

Scopes:
[0] NamesScope           5b49dc8b-39c3-426c-9758-aaed794611e8
    names=[]
```
