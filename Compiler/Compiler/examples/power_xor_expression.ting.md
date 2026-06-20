```ting
int^2 ^^ int
```
```ting-output
Input:
int^2 ^^ int

Compilation succeeded.

CoreExp:
Apply
  (Identifier
     (Referential ([00000000-0000-0000-0000-000000000000], [TextPart "_^^_"])),
   Tuple
     [Apply
        (Identifier
           (Referential
              ([00000000-0000-0000-0000-000000000000], [TextPart "_^_"])),
         Tuple
           [Identifier
              (Referential
                 ([00000000-0000-0000-0000-000000000000], [TextPart "int"]));
            Literal (Int32 2)]);
      Identifier
        (Referential ([00000000-0000-0000-0000-000000000000], [TextPart "int"]))])

Scopes:
No scopes collected.
```
