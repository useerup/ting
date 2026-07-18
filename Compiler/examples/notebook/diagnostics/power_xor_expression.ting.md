```ting
int^2 ^^ int
```
```ting-output
Compilation succeeded.

Apply
  (Identifier
     (Resolved (#0, [TextPart "_^^_"])),
   Tuple
     [Apply
        (Identifier
           (Resolved (#1, [TextPart "_^_"])),
         Tuple
           [Identifier
              (Resolved (#2, [TextPart "int"]));
            Literal (Int32 2)]);
      Identifier
        (Resolved (#2, [TextPart "int"]))])

Scopes:
No scopes collected.
```
