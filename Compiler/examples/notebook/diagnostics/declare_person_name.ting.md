```ting
let person.Name = "Zaphod"
```
```ting-output
Compilation succeeded.

Local
  (Exp+LocalScope,
   Apply
     (Identifier
        (Resolved (#0, [TextPart "_=_"])),
      Tuple
        [Projection
           (Exp+LocalScope,
            Identifier
              (Referential
                 ([#1;
                   00000000-0000-0000-0000-000000000000], [TextPart "person"])),
            Identifier
              (Declarative
                 (#2, [TextPart "Name"])));
         Literal (String "Zaphod")]))

Scopes:
[0] NamesScope           #1
    names=[Name<extension property> host=person.]
[1] NamesScope           #3
    names=[]

Diagnostics:
- unresolved name 'person'
```
