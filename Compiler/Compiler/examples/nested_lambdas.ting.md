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
           (Declarative (bc96f21d-6454-4ba7-9738-3cda3f27409f, [TextPart "x"]));
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
                             (b52691f9-a4ef-4d02-97cb-aa834890af1a,
                              [TextPart "x"]));
                        Identifier
                          (Declarative
                             (42833eb4-3802-42f2-a293-bf59b6f80c96,
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
                                            (b115585a-46a3-47d3-8b46-95e726f3767e,
                                             [TextPart "x"]));
                                       Identifier
                                         (Declarative
                                            (10d40d8a-10ec-4eb0-a416-7bc4b4850c9e,
                                             [TextPart "y"]))]);
                                 Identifier
                                   (Declarative
                                      (412b0731-e467-42c5-9549-d1e1019843b1,
                                       [TextPart "z"]))]);
                           Identifier
                             (Referential
                                ([fad4b95d-b05e-4727-aebb-4080811d47c8;
                                  6ed76590-44a5-4dd6-981a-94c87647d340;
                                  96a0d9d3-843a-4f09-b0bd-acb437f17d6e;
                                  00000000-0000-0000-0000-000000000000],
                                 [TextPart "x"]))]))]))]))

Scopes:
[0] NamesScope           96a0d9d3-843a-4f09-b0bd-acb437f17d6e
    names=[x<name>]
[1] NamesScope           6ed76590-44a5-4dd6-981a-94c87647d340
    names=[x<name>, y<name>]
[2] NamesScope           fad4b95d-b05e-4727-aebb-4080811d47c8
    names=[x<name>, y<name>, z<name>]
```
