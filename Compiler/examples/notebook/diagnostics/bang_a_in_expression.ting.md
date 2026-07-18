```ting
!a in b + c * d
```
```ting-output
Compilation succeeded.

Apply
  (Identifier
     (Resolved (#0, [TextPart "_in_"])),
   Tuple
     [Apply
        (Identifier
           (Referential
              ([00000000-0000-0000-0000-000000000000], [TextPart "!_"])),
         Identifier
           (Referential ([00000000-0000-0000-0000-000000000000], [TextPart "a"])));
      Apply
        (Identifier
           (Resolved (#1, [TextPart "_+_"])),
         Tuple
           [Identifier
              (Referential
                 ([00000000-0000-0000-0000-000000000000], [TextPart "b"]));
            Apply
              (Identifier
                 (Resolved
                    (#2, [TextPart "_*_"])),
               Tuple
                 [Identifier
                    (Referential
                       ([00000000-0000-0000-0000-000000000000], [TextPart "c"]));
                  Identifier
                    (Referential
                       ([00000000-0000-0000-0000-000000000000], [TextPart "d"]))])])])

Scopes:
No scopes collected.

Diagnostics:
- unresolved name '!_'
- unresolved name 'a'
- unresolved name 'b'
- unresolved name 'c'
- unresolved name 'd'
```
