```ting
(x + y) * (a - b) in z
```
```ting-output
Compilation succeeded.

Apply
  (Identifier
     (Resolved (#0, [TextPart "_in_"])),
   Tuple
     [Apply
        (Identifier
           (Resolved (#1, [TextPart "_*_"])),
         Tuple
           [Tuple
              [Apply
                 (Identifier
                    (Resolved
                       (#2, [TextPart "_+_"])),
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
                    (Resolved
                       (#3, [TextPart "_-_"])),
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

Diagnostics:
- unresolved name 'x'
- unresolved name 'y'
- unresolved name 'a'
- unresolved name 'b'
- unresolved name 'z'
```
