```ting
"zark" => (name->value)
```
```ting-output
Compilation succeeded.

Local
  (Exp+LocalScope,
   Apply
     (Identifier
        (Resolved (#0, [TextPart "_=>_"])),
      Tuple
        [Literal (String "zark");
         Tuple
           [Apply
              (Identifier
                 (Resolved
                    (#1, [TextPart "_->_"])),
               Tuple
                 [Identifier
                    (Referential
                       ([#2;
                         00000000-0000-0000-0000-000000000000],
                        [TextPart "name"]));
                  Identifier
                    (Referential
                       ([#2;
                         00000000-0000-0000-0000-000000000000],
                        [TextPart "value"]))])]]))

Scopes:
[0] NamesScope           #2
    names=[]

Diagnostics:
- unresolved name 'name'
- unresolved name 'value'
```
