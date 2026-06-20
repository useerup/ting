```ting
{ int x \ x % 2 == 0}
```
```ting-output
Input:
{ int x \ x % 2 == 0}

Compilation succeeded.

CoreExp:
Set
  [Local
     (CoreExp+LocalScope,
      Apply
        (Identifier
           (Referential
              ([00000000-0000-0000-0000-000000000000], [TextPart "_\_"])),
         Tuple
           [Apply
              (Identifier
                 (Referential
                    ([87d3aad5-e56d-4157-aa09-8bd600c1c293;
                      00000000-0000-0000-0000-000000000000], [TextPart "int"])),
               Identifier
                 (Declarative
                    (6477b2fa-f1f0-4d5e-aede-cceeb5be547a, [TextPart "x"])));
            Apply
              (Identifier
                 (Referential
                    ([00000000-0000-0000-0000-000000000000], [TextPart "_==_"])),
               Tuple
                 [Apply
                    (Identifier
                       (Referential
                          ([00000000-0000-0000-0000-000000000000],
                           [TextPart "_%_"])),
                     Tuple
                       [Identifier
                          (Referential
                             ([87d3aad5-e56d-4157-aa09-8bd600c1c293;
                               00000000-0000-0000-0000-000000000000],
                              [TextPart "x"])); Literal (Int32 2)]);
                  Literal (Int32 0)])]))]

Scopes:
[0] NamesScope           87d3aad5-e56d-4157-aa09-8bd600c1c293
    names=[x<name>]
```
