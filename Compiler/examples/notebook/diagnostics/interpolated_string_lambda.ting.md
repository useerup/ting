```ting
$"{Word name} = {int value}" => (name->value)
```
```ting-output
Compilation succeeded.

Local
  (CoreExp+LocalScope,
   Apply
     (Identifier
        (Resolved (#0, [TextPart "_=>_"])),
      Tuple
        [InterpolatedString
           [HolePart
              (Apply
                 (Identifier
                    (Referential
                       ([#1;
                         00000000-0000-0000-0000-000000000000],
                        [TextPart "Word"])),
                  Identifier
                    (Declarative
                       (#2, [TextPart "name"]))));
            TextPart " = ";
            HolePart
              (Apply
                 (Identifier
                    (Resolved
                       (#3, [TextPart "int"])),
                  Identifier
                    (Declarative
                       (#4, [TextPart "value"]))))];
         Tuple
           [Apply
              (Identifier
                 (Resolved
                    (#5, [TextPart "_->_"])),
               Tuple
                 [Identifier
                    (Resolved
                       (#2, [TextPart "name"]));
                  Identifier
                    (Resolved
                       (#4, [TextPart "value"]))])]]))

Scopes:
[0] NamesScope           #1
    names=[name<name>, value<name>]

Diagnostics:
- unresolved name 'Word'
```
