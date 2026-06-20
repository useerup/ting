```ting
DateTime this => $"{Year}-{Month}-{Day}"
```
```ting-output
Input:
DateTime this => $"{Year}-{Month}-{Day}"

Compilation succeeded.

CoreExp:
Local
  (CoreExp+LocalScope,
   Apply
     (Identifier
        (Referential ([00000000-0000-0000-0000-000000000000], [TextPart "_=>_"])),
      Tuple
        [Apply
           (Identifier
              (Referential
                 ([506da57f-0a54-4d40-870b-b56147e48140;
                   00000000-0000-0000-0000-000000000000], [TextPart "DateTime"])),
            This (Declarative (2440a6e1-9aa3-4f1e-85ae-5d996304e8fd, [])));
         InterpolatedString
           [HolePart
              (Identifier
                 (Referential
                    ([506da57f-0a54-4d40-870b-b56147e48140;
                      00000000-0000-0000-0000-000000000000], [TextPart "Year"])));
            TextPart "-";
            HolePart
              (Identifier
                 (Referential
                    ([506da57f-0a54-4d40-870b-b56147e48140;
                      00000000-0000-0000-0000-000000000000], [TextPart "Month"])));
            TextPart "-";
            HolePart
              (Identifier
                 (Referential
                    ([506da57f-0a54-4d40-870b-b56147e48140;
                      00000000-0000-0000-0000-000000000000], [TextPart "Day"])))]]))

Scopes:
[0] NamesScope           506da57f-0a54-4d40-870b-b56147e48140
    names=[<name>]
```
