```ting
let x in y
```

Flollop

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
           (Declarative (a380fa01-e7fa-4802-9e00-36c70ad9e6ac, [TextPart "x"]));
         Identifier
           (Referential
              ([c9df73f7-34b5-4d8e-9fcd-87ad1d9a4be6;
                00000000-0000-0000-0000-000000000000], [TextPart "y"]))]))

Scopes:
[0] NamesScope           c9df73f7-34b5-4d8e-9fcd-87ad1d9a4be6
    names=[x<name>]
```

```ting
let x in z
```
```ting-output
Input:
let x in z

Compilation succeeded.

CoreExp:
Local
  (CoreExp+LocalScope,
   Apply
     (Identifier
        (Referential ([00000000-0000-0000-0000-000000000000], [TextPart "_in_"])),
      Tuple
        [Identifier
           (Declarative (2d03776f-078a-4d75-af38-d93842c7338c, [TextPart "x"]));
         Identifier
           (Referential
              ([8c3b9b7f-11f3-45b7-8cfd-7165d9957f8e;
                00000000-0000-0000-0000-000000000000], [TextPart "z"]))]))

Scopes:
[0] NamesScope           8c3b9b7f-11f3-45b7-8cfd-7165d9957f8e
    names=[x<name>]
```
