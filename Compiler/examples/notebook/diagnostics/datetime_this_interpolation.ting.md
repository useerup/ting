```ting
DateTime this => $"{Year}-{Month}-{Day}"
```
```ting-output
Compilation succeeded.

Local
  (Exp+LocalScope,
   Apply
     (Identifier
        (Resolved (#0, [TextPart "_=>_"])),
      Tuple
        [Apply
           (Identifier
              (Resolved
                 (#1, [TextPart "DateTime"])),
            This (Declarative (#2, [])));
         StringLiteral
           [HolePart
              (Identifier
                 (Referential
                    ([#3;
                      00000000-0000-0000-0000-000000000000], [TextPart "Year"])));
            TextPart "-";
            HolePart
              (Identifier
                 (Referential
                    ([#3;
                      00000000-0000-0000-0000-000000000000], [TextPart "Month"])));
            TextPart "-";
            HolePart
              (Identifier
                 (Referential
                    ([#3;
                      00000000-0000-0000-0000-000000000000], [TextPart "Day"])))]]))

Scopes:
[0] NamesScope           #3
    names=[<name>]

Diagnostics:
- unresolved name 'Year'
- unresolved name 'Month'
- unresolved name 'Day'
```
