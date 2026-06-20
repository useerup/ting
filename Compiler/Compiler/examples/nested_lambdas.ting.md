```ting
x => x + y => x + y + z => x
```
```ting-output
Input:
x => x + y => x + y + z => x

Compilation succeeded.

CoreExp:
Local
  (CoreExp+LocalScope,
   Apply
     (Identifier
        (Referential ([00000000-0000-0000-0000-000000000000], [TextPart "_=>_"])),
      Tuple
        [Identifier
           (Declarative (4a9998c0-23b7-4003-81ce-2238e2c68240, [TextPart "x"]));
         Local
           (CoreExp+LocalScope,
            Apply
              (Identifier
                 (Referential
                    ([00000000-0000-0000-0000-000000000000], [TextPart "_=>_"])),
               Tuple
                 [Apply
                    (Identifier
                       (Referential
                          ([00000000-0000-0000-0000-000000000000],
                           [TextPart "_+_"])),
                     Tuple
                       [Identifier
                          (Declarative
                             (d36f6908-2412-4366-9341-affde18eb512,
                              [TextPart "x"]));
                        Identifier
                          (Declarative
                             (390bcf61-bc1f-429c-828b-3a452ff9fbc8,
                              [TextPart "y"]))]);
                  Local
                    (CoreExp+LocalScope,
                     Apply
                       (Identifier
                          (Referential
                             ([00000000-0000-0000-0000-000000000000],
                              [TextPart "_=>_"])),
                        Tuple
                          [Apply
                             (Identifier
                                (Referential
                                   ([00000000-0000-0000-0000-000000000000],
                                    [TextPart "_+_"])),
                              Tuple
                                [Apply
                                   (Identifier
                                      (Referential
                                         ([00000000-0000-0000-0000-000000000000],
                                          [TextPart "_+_"])),
                                    Tuple
                                      [Identifier
                                         (Declarative
                                            (05f190e7-f251-43ae-906d-7d46868a4a89,
                                             [TextPart "x"]));
                                       Identifier
                                         (Declarative
                                            (ab916fb8-d712-4b9e-aa2f-449df8c8b165,
                                             [TextPart "y"]))]);
                                 Identifier
                                   (Declarative
                                      (543aa826-e46f-4093-bcf1-ebabac867298,
                                       [TextPart "z"]))]);
                           Identifier
                             (Referential
                                ([a3825af2-dcb8-498e-8e2d-a0ea62936540;
                                  c5ea8b83-e8f6-44a6-83d2-acd4d1214e78;
                                  a7e927c8-62e9-4ab8-87e5-dadeab21ac0c;
                                  00000000-0000-0000-0000-000000000000],
                                 [TextPart "x"]))]))]))]))

Scopes:
[0] NamesScope           a7e927c8-62e9-4ab8-87e5-dadeab21ac0c
    names=[x<name>]
[1] NamesScope           c5ea8b83-e8f6-44a6-83d2-acd4d1214e78
    names=[x<name>, y<name>]
[2] NamesScope           a3825af2-dcb8-498e-8e2d-a0ea62936540
    names=[x<name>, y<name>, z<name>]
```
