```ting
protected internal { 1, 2, 3, a }
```
```ting-output
Input:
protected internal { 1, 2, 3, a }

Compilation succeeded.

CoreExp:
Publish
  (CoreExp+LocalScope, Protected,
   Publish
     (CoreExp+LocalScope, Internal,
      Set
        [Literal (Int32 1); Literal (Int32 2); Literal (Int32 3);
         Identifier
           (Declarative (0da9eac3-c38b-498a-91cf-6a0dbccd14b7, [TextPart "a"]))]))

Scopes:
[0] NamesScope           8dd8ecb9-0840-439c-837d-b417c6adc30f
    names=[a<name>]
[1] NamesScope           07383e31-9596-4b16-ab2d-2dd2636a2ba3
    names=[a<name>]
```
