```ting
!(let 42 / 6 - 1)
```
```ting-output
Input:
!(let 42 / 6 - 1)

Compilation succeeded.

CoreExp:
Apply
  (Identifier
     (Referential ([00000000-0000-0000-0000-000000000000], [TextPart "!_"])),
   Tuple
     [Local
        (CoreExp+LocalScope,
         Apply
           (Identifier
              (Referential
                 ([00000000-0000-0000-0000-000000000000], [TextPart "_-_"])),
            Tuple
              [Apply
                 (Identifier
                    (Referential
                       ([00000000-0000-0000-0000-000000000000], [TextPart "_/_"])),
                  Tuple [Literal (Int32 42); Literal (Int32 6)]);
               Literal (Int32 1)]))])

Scopes:
[0] NamesScope           9021a41a-14d2-4edf-8960-f3905ef71300
    names=[]
```
