```ting
{ int x \ x % 2 == 0}
```
```ting-output
Compilation succeeded.

Set
  [Local
     (CoreExp+LocalScope,
      Apply
        (Identifier
           (Resolved (#0, [TextPart "_\_"])),
         Tuple
           [Apply
              (Identifier
                 (Resolved
                    (#1, [TextPart "int"])),
               Identifier
                 (Declarative
                    (#2, [TextPart "x"])));
            Apply
              (Identifier
                 (Resolved
                    (#3, [TextPart "_==_"])),
               Tuple
                 [Apply
                    (Identifier
                       (Resolved
                          (#4,
                           [TextPart "_%_"])),
                     Tuple
                       [Identifier
                          (Resolved
                             (#2,
                              [TextPart "x"])); Literal (Int32 2)]);
                  Literal (Int32 0)])]))]

Scopes:
[0] NamesScope           #5
    names=[x<name>]
```
