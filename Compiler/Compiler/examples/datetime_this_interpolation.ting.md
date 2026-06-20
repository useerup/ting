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
                 ([b0c5b352-1c4e-424a-90bb-00691bd2e1a3;
                   00000000-0000-0000-0000-000000000000], [TextPart "DateTime"])),
            This (Declarative (6e517f6f-91ed-47cb-8841-8fa259f0a8af, [])));
         InterpolatedString
           [HolePart
              (Identifier
                 (Referential
                    ([b0c5b352-1c4e-424a-90bb-00691bd2e1a3;
                      00000000-0000-0000-0000-000000000000], [TextPart "Year"])));
            TextPart "-";
            HolePart
              (Identifier
                 (Referential
                    ([b0c5b352-1c4e-424a-90bb-00691bd2e1a3;
                      00000000-0000-0000-0000-000000000000], [TextPart "Month"])));
            TextPart "-";
            HolePart
              (Identifier
                 (Referential
                    ([b0c5b352-1c4e-424a-90bb-00691bd2e1a3;
                      00000000-0000-0000-0000-000000000000], [TextPart "Day"])))]]))

Scopes:
[0] NamesScope           b0c5b352-1c4e-424a-90bb-00691bd2e1a3
    names=[<name>]
```
