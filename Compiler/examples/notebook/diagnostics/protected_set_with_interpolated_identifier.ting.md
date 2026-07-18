```ting
protected { 1, 2, 3, $`a{zark}b` }
```
```ting-output
Compilation succeeded.

Publish
  (CoreExp+LocalScope, Protected,
   Set
     [Literal (Int32 1); Literal (Int32 2); Literal (Int32 3);
      Identifier
        (Declarative
           (#0,
            [TextPart "a";
             HolePart
               (Identifier
                  (Referential
                     ([#1;
                       00000000-0000-0000-0000-000000000000], [TextPart "zark"])));
             TextPart "b"]))])

Scopes:
[0] NamesScope           #1
    names=[a{...}b<name>]
```
