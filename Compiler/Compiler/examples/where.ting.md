```ting
T * T ^^ T where T:(Integrals|FloatingPoints|Lists)
```
```ting-output
Input:
T * T ^^ T where T:(Integrals|FloatingPoints|Lists)

Compilation succeeded.

CoreExp:
Local
  (CoreExp+LocalScope,
   Apply
     (Identifier
        (Referential
           ([00000000-0000-0000-0000-000000000000], [TextPart "_where_"])),
      Tuple
        [Apply
           (Identifier
              (Referential
                 ([00000000-0000-0000-0000-000000000000], [TextPart "_^^_"])),
            Tuple
              [Apply
                 (Identifier
                    (Referential
                       ([00000000-0000-0000-0000-000000000000], [TextPart "_*_"])),
                  Tuple
                    [Identifier
                       (Referential
                          ([5c00ac37-36dc-4542-b9e5-d7eaf0a36493;
                            00000000-0000-0000-0000-000000000000],
                           [TextPart "T"]));
                     Identifier
                       (Referential
                          ([5c00ac37-36dc-4542-b9e5-d7eaf0a36493;
                            00000000-0000-0000-0000-000000000000],
                           [TextPart "T"]))]);
               Identifier
                 (Referential
                    ([5c00ac37-36dc-4542-b9e5-d7eaf0a36493;
                      00000000-0000-0000-0000-000000000000], [TextPart "T"]))]);
         Apply
           (Identifier
              (Referential
                 ([00000000-0000-0000-0000-000000000000], [TextPart "_:_"])),
            Tuple
              [Identifier
                 (Declarative
                    (4e76d767-cb53-487d-a304-f07a9459b494, [TextPart "T"]));
               Tuple
                 [Apply
                    (Identifier
                       (Referential
                          ([00000000-0000-0000-0000-000000000000],
                           [TextPart "_|_"])),
                     Tuple
                       [Apply
                          (Identifier
                             (Referential
                                ([00000000-0000-0000-0000-000000000000],
                                 [TextPart "_|_"])),
                           Tuple
                             [Identifier
                                (Referential
                                   ([5c00ac37-36dc-4542-b9e5-d7eaf0a36493;
                                     00000000-0000-0000-0000-000000000000],
                                    [TextPart "Integrals"]));
                              Identifier
                                (Referential
                                   ([5c00ac37-36dc-4542-b9e5-d7eaf0a36493;
                                     00000000-0000-0000-0000-000000000000],
                                    [TextPart "FloatingPoints"]))]);
                        Identifier
                          (Referential
                             ([5c00ac37-36dc-4542-b9e5-d7eaf0a36493;
                               00000000-0000-0000-0000-000000000000],
                              [TextPart "Lists"]))])]])]))

Scopes:
[0] NamesScope           5c00ac37-36dc-4542-b9e5-d7eaf0a36493
    names=[T<name>]
```
