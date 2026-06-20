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
                    (e26300bf-739e-4545-a4d8-6689fe95869c, [TextPart "answer"]));
               Literal (Int32 42)]);
         Identifier
           (Referential
              ([ddbc25ef-a011-4205-ab8b-653cb1a99ee5;
                00000000-0000-0000-0000-000000000000],
               [TextPart "Flollop";
                HolePart
                  (Identifier
                     (Referential
                        ([ddbc25ef-a011-4205-ab8b-653cb1a99ee5;
                          00000000-0000-0000-0000-000000000000],
                         [TextPart "answer"])))]))]))

Scopes:
[0] NamesScope           ddbc25ef-a011-4205-ab8b-653cb1a99ee5
    names=[answer<name>]
```
