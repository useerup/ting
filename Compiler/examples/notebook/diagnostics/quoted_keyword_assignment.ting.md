```ting
`then` = 42
```
```ting-output
Compilation succeeded.

Apply
  (Identifier
     (Resolved (#0, [TextPart "_=_"])),
   Tuple
     [Identifier
        (Referential ([00000000-0000-0000-0000-000000000000], [TextPart "then"]));
      Literal (Int32 42)])

Scopes:
No scopes collected.

Diagnostics:
- unresolved name 'then'
```
