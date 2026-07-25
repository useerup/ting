```ting
person.Name = "Zaphod"
```
```ting-output
Compilation succeeded.

Apply
  (Identifier
     (Resolved (#0, [TextPart "_=_"])),
   Tuple
     [Projection
        (Exp+LocalScope,
         Identifier
           (Referential
              ([00000000-0000-0000-0000-000000000000], [TextPart "person"])),
         Identifier
           (Referential
              ([00000000-0000-0000-0000-000000000000], [TextPart "Name"])));
      Literal (String "Zaphod")])

Scopes:
[0] NamesScope           #1
    names=[]

Diagnostics:
- unresolved name 'person'
```
