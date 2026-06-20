```ting
{ int x \ x % 2 == 0}
```
```ting-output
Input:
{ int x \ x % 2 == 0}

Compilation succeeded.

CoreExp:
Set
  [Local
     (CoreExp+LocalScope,
      Apply
        (Identifier
           (Referential
              ([00000000-0000-0000-0000-000000000000], [TextPart "_\_"])),
         Tuple
           [Apply
              (Identifier
                 (Referential
                    ([2972a6ce-5075-427d-82e5-61757993496f;
                      00000000-0000-0000-0000-000000000000], [TextPart "int"])),
               Identifier
                 (Declarative
                    (5110529e-8d41-4bcf-a347-aa36416641be, [TextPart "x"])));
            Apply
              (Identifier
                 (Referential
                    ([00000000-0000-0000-0000-000000000000], [TextPart "_==_"])),
               Tuple
                 [Apply
                    (Identifier
                       (Referential
                          ([00000000-0000-0000-0000-000000000000],
                           [TextPart "_%_"])),
                     Tuple
                       [Identifier
                          (Referential
                             ([2972a6ce-5075-427d-82e5-61757993496f;
                               00000000-0000-0000-0000-000000000000],
                              [TextPart "x"])); Literal (Int32 2)]);
                  Literal (Int32 0)])]))]

Scopes:
[0] NamesScope           2972a6ce-5075-427d-82e5-61757993496f
    names=[x<name>]
```
