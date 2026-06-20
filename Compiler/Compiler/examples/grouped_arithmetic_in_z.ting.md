```ting
(x + y) * (a - b) in z
```
```ting-output
Input:
(x + y) * (a - b) in z

Compilation succeeded.

CoreExp:
Apply
  (Identifier
     (Referential ([00000000-0000-0000-0000-000000000000], [TextPart "_in_"])),
   Tuple
     [Apply
        (Identifier
           (Referential
              ([00000000-0000-0000-0000-000000000000], [TextPart "_*_"])),
         Tuple
           [Tuple
              [Apply
                 (Identifier
                    (Referential
                       ([00000000-0000-0000-0000-000000000000], [TextPart "_+_"])),
                  Tuple
                    [Identifier
                       (Referential
                          ([00000000-0000-0000-0000-000000000000],
                           [TextPart "x"]));
                     Identifier
                       (Referential
                          ([00000000-0000-0000-0000-000000000000],
                           [TextPart "y"]))])];
            Tuple
              [Apply
                 (Identifier
                    (Referential
                       ([00000000-0000-0000-0000-000000000000], [TextPart "_-_"])),
                  Tuple
                    [Identifier
                       (Referential
                          ([00000000-0000-0000-0000-000000000000],
                           [TextPart "a"]));
                     Identifier
                       (Referential
                          ([00000000-0000-0000-0000-000000000000],
                           [TextPart "b"]))])]]);
      Identifier
        (Referential ([00000000-0000-0000-0000-000000000000], [TextPart "z"]))])

Scopes:
No scopes collected.
```
