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
                          (b57b7cef-3448-41f9-a5f8-83f68308bafb,
                           [TextPart "Answer"]));
                     Identifier
                       (Referential
                          ([535284f2-0bb4-440f-b4f4-60c24b435a9e;
                            59e669e2-c4b3-4974-b796-7e92b7012c4d;
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
                       ([59e669e2-c4b3-4974-b796-7e92b7012c4d;
                         00000000-0000-0000-0000-000000000000],
                        [TextPart "Answer"])); Literal (Int32 42)])]]))

Scopes:
[0] NamesScope           59e669e2-c4b3-4974-b796-7e92b7012c4d
    names=[Answer<name>]
[1] NamesScope           535284f2-0bb4-440f-b4f4-60c24b435a9e
    names=[Answer<name>]
```
