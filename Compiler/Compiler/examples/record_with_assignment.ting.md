```ting
record(Answer:int) \ (Answer = 42)
```
```ting-output
Input:
record(Answer:int) \ (Answer = 42)

Compilation succeeded.

CoreExp:
Local
  (CoreExp+LocalScope,
   Apply
     (Identifier
        (Referential ([00000000-0000-0000-0000-000000000000], [TextPart "_\_"])),
      Tuple
        [Record
           (CoreExp+LocalScope,
            Tuple
              [Apply
                 (Identifier
                    (Referential
                       ([00000000-0000-0000-0000-000000000000], [TextPart "_:_"])),
                  Tuple
                    [Identifier
                       (Declarative
                          (72086fcf-a68b-4955-8a59-4ddcfdcb295c,
                           [TextPart "Answer"]));
                     Identifier
                       (Referential
                          ([f50fd1ba-583c-474f-9ece-6a775574be07;
                            a4310fe6-2fe8-4450-9b9f-6758dff93fd1;
                            00000000-0000-0000-0000-000000000000],
                           [TextPart "int"]))])]);
         Tuple
           [Apply
              (Identifier
                 (Referential
                    ([00000000-0000-0000-0000-000000000000], [TextPart "_=_"])),
               Tuple
                 [Identifier
                    (Referential
                       ([a4310fe6-2fe8-4450-9b9f-6758dff93fd1;
                         00000000-0000-0000-0000-000000000000],
                        [TextPart "Answer"])); Literal (Int32 42)])]]))

Scopes:
[0] NamesScope           a4310fe6-2fe8-4450-9b9f-6758dff93fd1
    names=[Answer<name>]
[1] NamesScope           f50fd1ba-583c-474f-9ece-6a775574be07
    names=[Answer<name>]
```
