```ting
!a in b + c * d
```
```ting-output
Input:
!a in b + c * d

Compilation succeeded.

CoreExp:
Apply
  (Identifier
     (Referential ([00000000-0000-0000-0000-000000000000], [TextPart "_in_"])),
   Tuple
     [Apply
        (Identifier
           (Referential
              ([00000000-0000-0000-0000-000000000000], [TextPart "!_"])),
         Identifier
           (Referential ([00000000-0000-0000-0000-000000000000], [TextPart "a"])));
      Apply
        (Identifier
           (Referential
              ([00000000-0000-0000-0000-000000000000], [TextPart "_+_"])),
         Tuple
           [Identifier
              (Referential
                 ([00000000-0000-0000-0000-000000000000], [TextPart "b"]));
            Apply
              (Identifier
                 (Referential
                    ([00000000-0000-0000-0000-000000000000], [TextPart "_*_"])),
               Tuple
                 [Identifier
                    (Referential
                       ([00000000-0000-0000-0000-000000000000], [TextPart "c"]));
                  Identifier
                    (Referential
                       ([00000000-0000-0000-0000-000000000000], [TextPart "d"]))])])])

Scopes:
No scopes collected.
```
