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
[0] NamesScope           ec256c5e-82d1-4335-aa60-3aca686cadba
    names=[]
```
