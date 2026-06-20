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
           (c556d2a8-c01e-4267-bc7b-aa7f7c5a7381,
            [TextPart "a";
             HolePart
               (Identifier
                  (Referential
                     ([fdd1c2e9-65f4-446c-904f-6f5b30b53d7d;
                       00000000-0000-0000-0000-000000000000], [TextPart "zark"])));
             TextPart "b"]))])

Scopes:
[0] NamesScope           fdd1c2e9-65f4-446c-904f-6f5b30b53d7d
    names=[a{...}b<name>]
```
