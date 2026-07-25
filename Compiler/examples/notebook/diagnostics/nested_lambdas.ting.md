```ting
x => x + y => x + y + z => x
```
```ting-output
Compilation succeeded.

Local
  (Exp+LocalScope,
   Apply
     (Identifier
        (Resolved (#0, [TextPart "_=>_"])),
      Tuple
        [Identifier
           (Declarative (#1, [TextPart "x"]));
         Local
           (Exp+LocalScope,
            Apply
              (Identifier
                 (Resolved
                    (#0, [TextPart "_=>_"])),
               Tuple
                 [Apply
                    (Identifier
                       (Resolved
                          (#2,
                           [TextPart "_+_"])),
                     Tuple
                       [Identifier
                          (Declarative
                             (#3,
                              [TextPart "x"]));
                        Identifier
                          (Declarative
                             (#4,
                              [TextPart "y"]))]);
                  Local
                    (Exp+LocalScope,
                     Apply
                       (Identifier
                          (Resolved
                             (#0,
                              [TextPart "_=>_"])),
                        Tuple
                          [Apply
                             (Identifier
                                (Resolved
                                   (#2,
                                    [TextPart "_+_"])),
                              Tuple
                                [Apply
                                   (Identifier
                                      (Resolved
                                         (#2,
                                          [TextPart "_+_"])),
                                    Tuple
                                      [Identifier
                                         (Declarative
                                            (#5,
                                             [TextPart "x"]));
                                       Identifier
                                         (Declarative
                                            (#6,
                                             [TextPart "y"]))]);
                                 Identifier
                                   (Declarative
                                      (#7,
                                       [TextPart "z"]))]);
                           Identifier
                             (Resolved
                                (#5,
                                 [TextPart "x"]))]))]))]))

Scopes:
[0] NamesScope           #8
    names=[x<name>]
[1] NamesScope           #9
    names=[x<name>, y<name>]
[2] NamesScope           #10
    names=[x<name>, y<name>, z<name>]

Diagnostics:
- illegal shadowing of 'x' in nested local scope
```
