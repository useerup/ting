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
                 ([8f5b44bb-008b-4a93-b01f-42e53a84a013;
                   00000000-0000-0000-0000-000000000000], [TextPart "person"])),
            Identifier
              (Declarative
                 (516e4b0e-1cfa-4bc7-a7d3-03ee9ec3e328, [TextPart "Name"])));
         Literal (String "Zaphod")]))

Scopes:
[0] NamesScope           8f5b44bb-008b-4a93-b01f-42e53a84a013
    names=[Name<extension property>]
[1] NamesScope           ef5af0c5-1189-499c-a587-98d7cdb52f06
    names=[]
```
