```ting
x => x * 2m
```
```ting-output
Input:
x => x * 2m

Compilation succeeded.

CoreExp:
Local
  (CoreExp+LocalScope,
   Apply
     (Identifier
        (Referential ([00000000-0000-0000-0000-000000000000], [TextPart "_=>_"])),
      Tuple
        [Identifier
           (Declarative (819dbc9f-2fdf-4ca7-a627-2b900296f2c5, [TextPart "x"]));
         Apply
           (Identifier
              (Referential
                 ([00000000-0000-0000-0000-000000000000], [TextPart "_*_"])),
            Tuple
              [Identifier
                 (Referential
                    ([7f4a072d-c7b9-4863-bb79-9551471ac0e4;
                      00000000-0000-0000-0000-000000000000], [TextPart "x"]));
               Literal (Decimal 2M)])]))

Scopes:
[0] NamesScope           7f4a072d-c7b9-4863-bb79-9551471ac0e4
    names=[x<name>]
```
