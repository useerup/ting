```ting
let x in y
```
```ting-output
Compilation succeeded.

Local
  (CoreExp+LocalScope,
   Apply
     (Identifier
        (Resolved (#0, [TextPart "_in_"])),
      Tuple
        [Identifier
           (Declarative (#1, [TextPart "x"]));
         Identifier
           (Referential
              ([#2;
                00000000-0000-0000-0000-000000000000], [TextPart "y"]))]))

Scopes:
[0] NamesScope           #2
    names=[x<name>]

Diagnostics:
- unresolved name 'y'
```
