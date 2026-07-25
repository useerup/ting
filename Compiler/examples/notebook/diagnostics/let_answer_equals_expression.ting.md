```ting
let x=21 in Answer = x*2
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
                    (#2, [TextPart "x"]));
               Literal (Int32 21)]);
         Apply
           (Identifier
              (Resolved (#1, [TextPart "_=_"])),
            Tuple
              [Identifier
                 (Referential
                    ([#3;
                      00000000-0000-0000-0000-000000000000], [TextPart "Answer"]));
               Apply
                 (Identifier
                    (Resolved
                       (#4, [TextPart "_*_"])),
                  Tuple
                    [Identifier
                       (Resolved
                          (#2, [TextPart "x"]));
                     Literal (Int32 2)])])]))

Scopes:
[0] NamesScope           #3
    names=[x<name>]

Diagnostics:
- unresolved name 'Answer'
```
