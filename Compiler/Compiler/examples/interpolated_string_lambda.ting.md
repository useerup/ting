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
                       ([462c914f-dac9-4fde-88fa-2e7c01c657ba;
                         00000000-0000-0000-0000-000000000000],
                        [TextPart "Word"])),
                  Identifier
                    (Declarative
                       (ee6aaf87-d26d-4462-b641-2743cc11151e, [TextPart "name"]))));
            TextPart " = ";
            HolePart
              (Apply
                 (Identifier
                    (Referential
                       ([462c914f-dac9-4fde-88fa-2e7c01c657ba;
                         00000000-0000-0000-0000-000000000000], [TextPart "int"])),
                  Identifier
                    (Declarative
                       (73285875-65db-4f78-974f-206e7b4eb594, [TextPart "value"]))))];
         Tuple
           [Apply
              (Identifier
                 (Referential
                    ([00000000-0000-0000-0000-000000000000], [TextPart "_->_"])),
               Tuple
                 [Identifier
                    (Referential
                       ([462c914f-dac9-4fde-88fa-2e7c01c657ba;
                         00000000-0000-0000-0000-000000000000],
                        [TextPart "name"]));
                  Identifier
                    (Referential
                       ([462c914f-dac9-4fde-88fa-2e7c01c657ba;
                         00000000-0000-0000-0000-000000000000],
                        [TextPart "value"]))])]]))

Scopes:
[0] NamesScope           462c914f-dac9-4fde-88fa-2e7c01c657ba
    names=[name<name>, value<name>]
```
