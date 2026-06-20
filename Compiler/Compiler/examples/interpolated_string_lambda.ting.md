```ting
$"{Word name} = {int value}" => (name->value)
```
```ting-output
Input:
$"{Word name} = {int value}" => (name->value)

Compilation succeeded.

CoreExp:
Local
  (CoreExp+LocalScope,
   Apply
     (Identifier
        (Referential ([00000000-0000-0000-0000-000000000000], [TextPart "_=>_"])),
      Tuple
        [InterpolatedString
           [HolePart
              (Apply
                 (Identifier
                    (Referential
                       ([a7b45eb2-e51d-4f7d-ad44-659a72c1710b;
                         00000000-0000-0000-0000-000000000000],
                        [TextPart "Word"])),
                  Identifier
                    (Declarative
                       (909cb479-ba29-4d9f-90f2-e709412795cf, [TextPart "name"]))));
            TextPart " = ";
            HolePart
              (Apply
                 (Identifier
                    (Referential
                       ([a7b45eb2-e51d-4f7d-ad44-659a72c1710b;
                         00000000-0000-0000-0000-000000000000], [TextPart "int"])),
                  Identifier
                    (Declarative
                       (4988cb49-9f61-4645-baf2-4a3bedddcff5, [TextPart "value"]))))];
         Tuple
           [Apply
              (Identifier
                 (Referential
                    ([00000000-0000-0000-0000-000000000000], [TextPart "_->_"])),
               Tuple
                 [Identifier
                    (Referential
                       ([a7b45eb2-e51d-4f7d-ad44-659a72c1710b;
                         00000000-0000-0000-0000-000000000000],
                        [TextPart "name"]));
                  Identifier
                    (Referential
                       ([a7b45eb2-e51d-4f7d-ad44-659a72c1710b;
                         00000000-0000-0000-0000-000000000000],
                        [TextPart "value"]))])]]))

Scopes:
[0] NamesScope           a7b45eb2-e51d-4f7d-ad44-659a72c1710b
    names=[name<name>, value<name>]
```
