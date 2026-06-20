```ting
let x in y
```
```ting-output
Input:
let x in y

Compilation succeeded.

CoreExp:
Local
  (CoreExp+LocalScope,
   Apply
     (Identifier
        (Referential ([00000000-0000-0000-0000-000000000000], [TextPart "_in_"])),
      Tuple
        [Identifier
           (Declarative (c106604c-bd50-49c3-9df5-d5d72fbac02e, [TextPart "x"]));
         Identifier
           (Referential
              ([065f394d-693f-4c1c-bd98-1dc8ec08902c;
                00000000-0000-0000-0000-000000000000], [TextPart "y"]))]))

Scopes:
[0] NamesScope           065f394d-693f-4c1c-bd98-1dc8ec08902c
    names=[x<name>]
```
