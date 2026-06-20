```ting
let answer=42 in $`Flollop{answer}`
```
```ting-output
Input:
let answer=42 in $`Flollop{answer}`

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
                    (387a67d7-387b-43de-8115-56a65cb9a250, [TextPart "answer"]));
               Literal (Int32 42)]);
         Identifier
           (Referential
              ([f3654bd9-6ca7-4cef-b930-93af630f49ad;
                00000000-0000-0000-0000-000000000000],
               [TextPart "Flollop";
                HolePart
                  (Identifier
                     (Referential
                        ([f3654bd9-6ca7-4cef-b930-93af630f49ad;
                          00000000-0000-0000-0000-000000000000],
                         [TextPart "answer"])))]))]))

Scopes:
[0] NamesScope           f3654bd9-6ca7-4cef-b930-93af630f49ad
    names=[answer<name>]
```
