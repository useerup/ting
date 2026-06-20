```ting
"zark" => (name->value)
```
```ting-output
Input:
"zark" => (name->value)

Compilation succeeded.

CoreExp:
Local
  (CoreExp+LocalScope,
   Apply
     (Identifier
        (Referential ([00000000-0000-0000-0000-000000000000], [TextPart "_=>_"])),
      Tuple
        [Literal (String "zark");
         Tuple
           [Apply
              (Identifier
                 (Referential
                    ([00000000-0000-0000-0000-000000000000], [TextPart "_->_"])),
               Tuple
                 [Identifier
                    (Referential
                       ([e6d33214-d341-4be4-8220-e1d146c47747;
                         00000000-0000-0000-0000-000000000000],
                        [TextPart "name"]));
                  Identifier
                    (Referential
                       ([e6d33214-d341-4be4-8220-e1d146c47747;
                         00000000-0000-0000-0000-000000000000],
                        [TextPart "value"]))])]]))

Scopes:
[0] NamesScope           e6d33214-d341-4be4-8220-e1d146c47747
    names=[]
```
