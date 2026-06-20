```ting
let x=21 in Answer = x*2
```
```ting-output
Input:
let x=21 in Answer = x*2

Compilation succeeded.

CoreExp:
Local
  (CoreExp+LocalScope,
   Apply
     (Identifier
        (Referential ([00000000-0000-0000-0000-000000000000], [TextPart "_in_"])),
      Tuple
        [Apply
           (Identifier
              (Referential
                 ([00000000-0000-0000-0000-000000000000], [TextPart "_=_"])),
            Tuple
              [Identifier
                 (Declarative
                    (4eeab16a-60f8-459e-9662-fc54eface60c, [TextPart "x"]));
               Literal (Int32 21)]);
         Apply
           (Identifier
              (Referential
                 ([00000000-0000-0000-0000-000000000000], [TextPart "_=_"])),
            Tuple
              [Identifier
                 (Referential
                    ([01395a04-8b3e-4995-ab8b-d4906b68bdfd;
                      00000000-0000-0000-0000-000000000000], [TextPart "Answer"]));
               Apply
                 (Identifier
                    (Referential
                       ([00000000-0000-0000-0000-000000000000], [TextPart "_*_"])),
                  Tuple
                    [Identifier
                       (Referential
                          ([01395a04-8b3e-4995-ab8b-d4906b68bdfd;
                            00000000-0000-0000-0000-000000000000],
                           [TextPart "x"])); Literal (Int32 2)])])]))

Scopes:
[0] NamesScope           01395a04-8b3e-4995-ab8b-d4906b68bdfd
    names=[x<name>]
```
