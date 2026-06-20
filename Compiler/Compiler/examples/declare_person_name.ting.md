```ting
let person.Name = "Zaphod"
```
```ting-output
Input:
let person.Name = "Zaphod"

Compilation succeeded.

CoreExp:
Local
  (CoreExp+LocalScope,
   Apply
     (Identifier
        (Referential ([00000000-0000-0000-0000-000000000000], [TextPart "_=_"])),
      Tuple
        [Projection
           (CoreExp+LocalScope,
            Identifier
              (Referential
                 ([67252c1c-268a-4aff-8c5b-1c67d8c78075;
                   00000000-0000-0000-0000-000000000000], [TextPart "person"])),
            Identifier
              (Declarative
                 (4196778c-1be8-4ffa-82d8-fc45ae32708e, [TextPart "Name"])));
         Literal (String "Zaphod")]))

Scopes:
[0] NamesScope           67252c1c-268a-4aff-8c5b-1c67d8c78075
    names=[Name<extension property>]
[1] NamesScope           5d86bb23-0c86-4839-afde-f7632bb63aa0
    names=[]
```
