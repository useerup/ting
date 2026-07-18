```ting
x => x * 2m
```
```ting-output
Compilation succeeded.

Local
  (CoreExp+LocalScope,
   Apply
     (Identifier
        (Resolved (#0, [TextPart "_=>_"])),
      Tuple
        [Identifier
           (Declarative (#1, [TextPart "x"]));
         Apply
           (Identifier
              (Resolved (#2, [TextPart "_*_"])),
            Tuple
              [Identifier
                 (Resolved
                    (#1, [TextPart "x"]));
               Literal (Decimal 2M)])]))

Scopes:
[0] NamesScope           #3
    names=[x<name>]
```
