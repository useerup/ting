```ting
let answer=42 in $`Flollop{answer}`
```
```ting-output
Compilation succeeded.

Local
  (Exp+LocalScope,
   Apply
     (Identifier
        (Resolved (#0, [TextPart "_in_"])),
      Tuple
        [Apply
           (Identifier
              (Resolved (#1, [TextPart "_=_"])),
            Tuple
              [Identifier
                 (Declarative
                    (#2, [TextPart "answer"]));
               Literal (Int32 42)]);
         Identifier
           (Referential
              ([#3;
                00000000-0000-0000-0000-000000000000],
               [TextPart "Flollop";
                HolePart
                  (Identifier
                     (Referential
                        ([#3;
                          00000000-0000-0000-0000-000000000000],
                         [TextPart "answer"])))]))]))

Scopes:
[0] NamesScope           #3
    names=[answer<name>]
```
