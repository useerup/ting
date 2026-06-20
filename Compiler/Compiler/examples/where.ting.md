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
                          ([ba86d8cc-6e74-41da-b5bf-05b9da6adab2;
                            00000000-0000-0000-0000-000000000000],
                           [TextPart "T"]));
                     Identifier
                       (Referential
                          ([ba86d8cc-6e74-41da-b5bf-05b9da6adab2;
                            00000000-0000-0000-0000-000000000000],
                           [TextPart "T"]))]);
               Identifier
                 (Referential
                    ([ba86d8cc-6e74-41da-b5bf-05b9da6adab2;
                      00000000-0000-0000-0000-000000000000], [TextPart "T"]))]);
         Apply
           (Identifier
              (Referential
                 ([00000000-0000-0000-0000-000000000000], [TextPart "_:_"])),
            Tuple
              [Identifier
                 (Declarative
                    (a6e99b82-2d99-465f-8189-93a4631a5a28, [TextPart "T"]));
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
                                   ([ba86d8cc-6e74-41da-b5bf-05b9da6adab2;
                                     00000000-0000-0000-0000-000000000000],
                                    [TextPart "Integrals"]));
                              Identifier
                                (Referential
                                   ([ba86d8cc-6e74-41da-b5bf-05b9da6adab2;
                                     00000000-0000-0000-0000-000000000000],
                                    [TextPart "FloatingPoints"]))]);
                        Identifier
                          (Referential
                             ([ba86d8cc-6e74-41da-b5bf-05b9da6adab2;
                               00000000-0000-0000-0000-000000000000],
                              [TextPart "Lists"]))])]])]))

Scopes:
[0] NamesScope           ba86d8cc-6e74-41da-b5bf-05b9da6adab2
    names=[T<name>]
```
