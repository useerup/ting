```ting
a==2 then a==3 then 0 else 1 else 2
```
```ting-output
Compilation succeeded.

Apply
  (Identifier
     (Resolved (#0, [TextPart "_then_"])),
   Tuple
     [Apply
        (Identifier
           (Resolved (#1, [TextPart "_==_"])),
         Tuple
           [Identifier
              (Referential
                 ([00000000-0000-0000-0000-000000000000], [TextPart "a"]));
            Literal (Int32 2)]);
      Apply
        (Identifier
           (Resolved (#0, [TextPart "_then_"])),
         Tuple
           [Apply
              (Identifier
                 (Resolved
                    (#1, [TextPart "_==_"])),
               Tuple
                 [Identifier
                    (Referential
                       ([00000000-0000-0000-0000-000000000000], [TextPart "a"]));
                  Literal (Int32 3)]);
            Apply
              (Identifier
                 (Resolved
                    (#2, [TextPart "_else_"])),
               Tuple
                 [Literal (Int32 0);
                  Apply
                    (Identifier
                       (Resolved
                          (#2,
                           [TextPart "_else_"])),
                     Tuple [Literal (Int32 1); Literal (Int32 2)])])])])

Scopes:
No scopes collected.

Diagnostics:
- unresolved name 'a'
- unresolved name 'a'
```
