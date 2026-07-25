```ting
T * T ^^ T where T:(Integrals|FloatingPoints|Lists)
```
```ting-output
Compilation succeeded.

Local
  (Exp+LocalScope,
   Apply
     (Identifier
        (Referential
           ([00000000-0000-0000-0000-000000000000], [TextPart "_where_"])),
      Tuple
        [Apply
           (Identifier
              (Resolved
                 (#0, [TextPart "_^^_"])),
            Tuple
              [Apply
                 (Identifier
                    (Resolved
                       (#1, [TextPart "_*_"])),
                  Tuple
                    [Identifier
                       (Resolved
                          (#2, [TextPart "T"]));
                     Identifier
                       (Resolved
                          (#2, [TextPart "T"]))]);
               Identifier
                 (Resolved
                    (#2, [TextPart "T"]))]);
         Apply
           (Identifier
              (Resolved (#3, [TextPart "_:_"])),
            Tuple
              [Identifier
                 (Declarative
                    (#2, [TextPart "T"]));
               Tuple
                 [Apply
                    (Identifier
                       (Resolved
                          (#4,
                           [TextPart "_|_"])),
                     Tuple
                       [Apply
                          (Identifier
                             (Resolved
                                (#4,
                                 [TextPart "_|_"])),
                           Tuple
                             [Identifier
                                (Referential
                                   ([#5;
                                     00000000-0000-0000-0000-000000000000],
                                    [TextPart "Integrals"]));
                              Identifier
                                (Referential
                                   ([#5;
                                     00000000-0000-0000-0000-000000000000],
                                    [TextPart "FloatingPoints"]))]);
                        Identifier
                          (Referential
                             ([#5;
                               00000000-0000-0000-0000-000000000000],
                              [TextPart "Lists"]))])]])]))

Scopes:
[0] NamesScope           #5
    names=[T<name>]

Diagnostics:
- unresolved name '_where_'
- unresolved name 'Integrals'
- unresolved name 'FloatingPoints'
- unresolved name 'Lists'
```
