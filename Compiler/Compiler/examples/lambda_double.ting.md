```ting
x => x * 2m
```
```ting-output
Input:
x => x * 2m

Compilation succeeded.

CoreExp:
Local
  (CoreExp+LocalScope,
   Apply
     (Identifier
        (Referential ([00000000-0000-0000-0000-000000000000], [TextPart "_=>_"])),
      Tuple
        [Identifier
           (Declarative (16e97de2-30e5-42b1-8c92-b30f42498292, [TextPart "x"]));
         Apply
           (Identifier
              (Referential
                 ([00000000-0000-0000-0000-000000000000], [TextPart "_*_"])),
            Tuple
              [Identifier
                 (Referential
                    ([6d02b79c-74dd-46d5-803c-603e2171616b;
                      00000000-0000-0000-0000-000000000000], [TextPart "x"]));
               Literal (Decimal 2M)])]))

Scopes:
[0] NamesScope           6d02b79c-74dd-46d5-803c-603e2171616b
    names=[x<name>]
```
