# Ordered pairs

`->` is right-referential: its left operand is ambient. Inside `fn { ... }` on the right of `=` every frame is referential, so the pattern variable `n` has no declarative frame to land in.

```ting
f = fn{ 0 -> 1, int n -> n * f (n - 1) } in f 3
```

```ting-output
Declarations:
  #1 f: local (in at 1,42)
Identifiers:
  1,1 f: declares #1
  1,17 int: references builtin
  1,21 n: unresolved
  1,26 n: unresolved
  1,30 f: references #1
  1,33 n: unresolved
  1,45 f: references #1
Errors:
  (1,21-1,22): unresolved name 'n'
  (1,26-1,27): unresolved name 'n'
  (1,33-1,34): unresolved name 'n'
```

An explicit `let` inside the case makes the pattern declarative. This is how the specification writes such cases.

```ting
f = fn{ 0 -> 1, let int n -> n * f (n - 1) } in f 3
```

```ting-output
Declarations:
  #1 f: local (in at 1,46)
  #2 n: local (let at 1,17)
Identifiers:
  1,1 f: declares #1
  1,21 int: references builtin
  1,25 n: declares #2
  1,30 n: references #2
  1,34 f: references #1
  1,37 n: references #2
  1,49 f: references #1
```

A lambda declares its pattern variables.

```ting
f = (0 => 1) || (int n => n * f (n - 1)) in f 3
```

```ting-output
Declarations:
  #1 f: local (in at 1,42)
  #2 n: local (=> at 1,24)
Identifiers:
  1,1 f: declares #1
  1,18 int: references builtin
  1,22 n: declares #2
  1,27 n: references #2
  1,31 f: references #1
  1,34 n: references #2
  1,45 f: references #1
```
