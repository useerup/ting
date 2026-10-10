# Ordered pairs

`->` is right-referential: its left operand is ambient. Inside `fn { ... }` on the right of `?=` every frame is referential, so the pattern variable `n` has no declarative frame to land in.

```ting
let f ?= fn{ 0 -> 1, int n -> n * f (n - 1) } in f 3
```

```ting-output
Declarations:
  #1 f: local (in at 1,47; let at 1,1)
Identifiers:
  1,5 f: declares #1
  1,22 int: references builtin
  1,26 n: unresolved
  1,31 n: unresolved
  1,35 f: references #1
  1,38 n: unresolved
  1,50 f: references #1
Errors:
  (1,26-1,27): unresolved name 'n'
  (1,31-1,32): unresolved name 'n'
  (1,38-1,39): unresolved name 'n'
```

An explicit `let` inside the case makes the pattern declarative. This is how the specification writes such cases.

```ting
let f ?= fn{ 0 -> 1, let int n -> n * f (n - 1) } in f 3
```

```ting-output
Declarations:
  #1 f: local (in at 1,51; let at 1,1)
  #2 n: local (let at 1,22)
Identifiers:
  1,5 f: declares #1
  1,26 int: references builtin
  1,30 n: declares #2
  1,35 n: references #2
  1,39 f: references #1
  1,42 n: references #2
  1,54 f: references #1
```

A lambda declares its pattern variables.

```ting
let f ?= (0 => 1) || (int n => n * f (n - 1)) in f 3
```

```ting-output
Declarations:
  #1 f: local (in at 1,47; let at 1,1)
  #2 n: local (=> at 1,29)
Identifiers:
  1,5 f: declares #1
  1,23 int: references builtin
  1,27 n: declares #2
  1,32 n: references #2
  1,36 f: references #1
  1,39 n: references #2
  1,50 f: references #1
```
