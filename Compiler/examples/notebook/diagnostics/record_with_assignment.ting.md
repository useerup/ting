```ting
record(Answer:int) \ (Answer = 42)
```
```ting-output
Compilation succeeded.

Local
  (Exp+LocalScope,
   Apply
     (Identifier
        (Resolved (#0, [TextPart "_\_"])),
      Tuple
        [Record
           (Exp+LocalScope,
            Tuple
              [Apply
                 (Identifier
                    (Resolved
                       (#1, [TextPart "_:_"])),
                  Tuple
                    [Identifier
                       (Declarative
                          (#2,
                           [TextPart "Answer"]));
                     Identifier
                       (Resolved
                          (#3,
                           [TextPart "int"]))])]);
         Tuple
           [Apply
              (Identifier
                 (Resolved
                    (#4, [TextPart "_=_"])),
               Tuple
                 [Identifier
                    (Resolved
                       (#2,
                        [TextPart "Answer"])); Literal (Int32 42)])]]))

Scopes:
[0] NamesScope           #5
    names=[Answer<name>]
[1] NamesScope           #6
    names=[Answer<name>]
```
