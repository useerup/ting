```ting
!(let 42 / 6 - 1)
```
```ting-output
Compilation succeeded.

Apply
  (Identifier
     (Referential ([00000000-0000-0000-0000-000000000000], [TextPart "!_"])),
   Tuple
     [Local
        (Exp+LocalScope,
         Apply
           (Identifier
              (Resolved (#0, [TextPart "_-_"])),
            Tuple
              [Apply
                 (Identifier
                    (Resolved
                       (#1, [TextPart "_/_"])),
                  Tuple [Literal (Int32 42); Literal (Int32 6)]);
               Literal (Int32 1)]))])

Scopes:
[0] NamesScope           #2
    names=[]

Diagnostics:
- unresolved name '!_'
```
