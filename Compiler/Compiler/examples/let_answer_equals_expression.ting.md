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
                    (9000ecf5-8db7-4add-8481-7201165725ae, [TextPart "x"]));
               Literal (Int32 21)]);
         Apply
           (Identifier
              (Referential
                 ([00000000-0000-0000-0000-000000000000], [TextPart "_=_"])),
            Tuple
              [Identifier
                 (Referential
                    ([5be6e7b5-6c3f-4a57-8296-40f1a1e3e680;
                      00000000-0000-0000-0000-000000000000], [TextPart "Answer"]));
               Apply
                 (Identifier
                    (Referential
                       ([00000000-0000-0000-0000-000000000000], [TextPart "_*_"])),
                  Tuple
                    [Identifier
                       (Referential
                          ([5be6e7b5-6c3f-4a57-8296-40f1a1e3e680;
                            00000000-0000-0000-0000-000000000000],
                           [TextPart "x"])); Literal (Int32 2)])])]))

Scopes:
[0] NamesScope           5be6e7b5-6c3f-4a57-8296-40f1a1e3e680
    names=[x<name>]
```
