```ting
protected { 1, 2, 3, $`a{zark}b` }
```
```ting-output
Input:
protected { 1, 2, 3, $`a{zark}b` }

Compilation succeeded.

CoreExp:
Publish
  (CoreExp+LocalScope, Protected,
   Set
     [Literal (Int32 1); Literal (Int32 2); Literal (Int32 3);
      Identifier
        (Declarative
           (21bc3fc1-90ee-43ae-bb27-c9045be332f4,
            [TextPart "a";
             HolePart
               (Identifier
                  (Referential
                     ([23fd5954-c491-4b2b-968c-3e6abc48d2c2;
                       00000000-0000-0000-0000-000000000000], [TextPart "zark"])));
             TextPart "b"]))])

Scopes:
[0] NamesScope           23fd5954-c491-4b2b-968c-3e6abc48d2c2
    names=[a{...}b<name>]
```
