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
                       ([429202df-f2eb-41cd-a866-4e1b59c5e0c5;
                         00000000-0000-0000-0000-000000000000],
                        [TextPart "name"]));
                  Identifier
                    (Referential
                       ([429202df-f2eb-41cd-a866-4e1b59c5e0c5;
                         00000000-0000-0000-0000-000000000000],
                        [TextPart "value"]))])]]))

Scopes:
[0] NamesScope           429202df-f2eb-41cd-a866-4e1b59c5e0c5
    names=[]
```
