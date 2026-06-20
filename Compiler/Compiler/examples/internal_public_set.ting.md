```ting
internal public { 1, 2, 3, a }
```
```ting-output
Input:
internal public { 1, 2, 3, a }

Compilation succeeded.

CoreExp:
Publish
  (CoreExp+LocalScope, Internal,
   Publish
     (CoreExp+LocalScope, Public,
      Set
        [Literal (Int32 1); Literal (Int32 2); Literal (Int32 3);
         Identifier
           (Declarative (a4735b87-9da6-4a46-a1e3-228b08c4f065, [TextPart "a"]))]))

Scopes:
[0] NamesScope           49a03a97-5c76-466e-9643-863c5368783b
    names=[a<name>]
[1] NamesScope           2890f016-6e75-4259-8750-768896b32b8f
    names=[a<name>]
```
