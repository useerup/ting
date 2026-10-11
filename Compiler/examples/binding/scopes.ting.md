# Scope-introducing operators

`in` is left-declarative: its left operand declares without `let`.

```ting
f = x => x in f 1
```

```ting-output
Declarations:
  #1 f: local (in at 1,12)
  #2 x: local (=> at 1,7)
Identifiers:
  1,1 f: declares #1
  1,5 x: declares #2
  1,10 x: references #2
  1,15 f: references #1
```

`where` is right-declarative.

```ting
y + 1 where y = 2
```

```ting-output
Declarations:
  #1 y: local (where at 1,7)
Identifiers:
  1,1 y: references #1
  1,13 y: declares #1
```

`all` is left-declarative: the condition declares the quantified names and the requirement refers to them.

```ting
x : int all x > 0
```

```ting-output
Declarations:
  #1 x: local (all at 1,9)
Identifiers:
  1,1 x: declares #1
  1,5 int: references builtin
  1,13 x: references #1
```

Nested quantifiers each get their own frame, and the inner requirement sees both names.

```ting
x : int all y : int all x < y
```

```ting-output
Declarations:
  #1 x: local (all at 1,9)
  #2 y: local (all at 1,21)
Identifiers:
  1,1 x: declares #1
  1,5 int: references builtin
  1,13 y: declares #2
  1,17 int: references builtin
  1,25 x: references #1
  1,29 y: references #2
```

On the right of `=` the quantified name stays local to the `all` frame.

```ting
b = (x : int all x > 0) in b
```

```ting-output
Declarations:
  #1 b: local (in at 1,25)
  #2 x: local (all at 1,14)
Identifiers:
  1,1 b: declares #1
  1,6 x: declares #2
  1,10 int: references builtin
  1,18 x: references #2
  1,28 b: references #1
```

The callee of an application is referential, the argument is ambient.

```ting
float a = 42 in a
```

```ting-output
Declarations:
  #1 a: local (in at 1,14)
Identifiers:
  1,1 float: references builtin
  1,7 a: declares #1
  1,17 a: references #1
```

The right operand of `=` can reference the declaration on its left, which is how recursive definitions are written.

```ting
f = x => f x in f
```

```ting-output
Declarations:
  #1 f: local (in at 1,14)
  #2 x: local (=> at 1,7)
Identifiers:
  1,1 f: declares #1
  1,5 x: declares #2
  1,10 f: references #1
  1,12 x: references #2
  1,17 f: references #1
```

`_` is anonymous: every occurrence is independent.

```ting
(_, _) = (1, 2) in 3
```

```ting-output
Identifiers:
  1,2 _: anonymous
  1,5 _: anonymous
```

Products: the named section declares fields; a bare name there also references the surrounding name.

```ting
p = 1 in (p, 2 @ n = p, p)
```

```ting-output
Declarations:
  #1 p: local (in at 1,7)
  #2 n: field (product at 1,10)
  #3 p: field (product at 1,10)
Identifiers:
  1,1 p: declares #1
  1,11 p: references #1
  1,18 n: declares #2
  1,22 p: references #1
  1,25 p: declares field #3, value references #1
```

Collections are ambient.

```ting
[a, b] = [1, 2] in a + b
```

```ting-output
Declarations:
  #1 a: local (in at 1,17)
  #2 b: local (in at 1,17)
Identifiers:
  1,2 a: declares #1
  1,5 b: declares #2
  1,20 a: references #1
  1,24 b: references #2
```
