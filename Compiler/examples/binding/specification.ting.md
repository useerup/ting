# Binding examples from the specification

Repeated declarations in the same scope constrain one binding.

```ting
((value = 1) & (value = 1)) in value
```

```ting-output
Declarations:
  #1 value: local (in at 1,29)
Identifiers:
  1,3 value: declares #1
  1,17 value: declares #1
  1,32 value: references #1
```

A nested scope may not redeclare a name declared in the same local expression.

```ting
value = 1 in (value = 2 in value)
```

```ting-output
Declarations:
  #1 value: local (in at 1,11)
  #2 value: local (in at 1,25)
Identifiers:
  1,1 value: declares #1
  1,15 value: declares #2
  1,28 value: references #2
Errors:
  (1,15-1,20): 'value' is already declared in an enclosing scope
```

`this` is the exception: a nested declaration shadows the outer one.

```ting
this = 1 in (this = 2 in this)
```

```ting-output
Declarations:
  #1 this: this (in at 1,10)
  #2 this: this (in at 1,23)
Identifiers:
  1,1 this: declares #1
  1,14 this: declares #2
  1,26 this: references #2
```

The such-that scope collects the lambda's declarations; `k` is declared there because the lambda frame is referential in the body.

```ting
(int x => x * 2 + k) \ (k = 42)
```

```ting-output
Declarations:
  #1 x: local (=> at 1,8; \ at 1,22)
  #2 k: local (\ at 1,22)
Identifiers:
  1,2 int: references builtin
  1,6 x: declares #1
  1,11 x: references #1
  1,19 k: declares #2
  1,25 k: references #2
```

```ting
(int x => c) \ (c = x * 2)
```

```ting-output
Declarations:
  #1 x: local (=> at 1,8; \ at 1,14)
  #2 c: local (\ at 1,14)
Identifiers:
  1,2 int: references builtin
  1,6 x: declares #1
  1,11 c: declares #2
  1,17 c: references #2
  1,21 x: references #1
```

Record fields are separate from surrounding locals.

```ting
value = 1 in record(value = 2)
```

```ting-output
Declarations:
  #1 value: local (in at 1,11)
  #2 this: this (record at 1,14)
  #3 value: field (record at 1,14)
Identifiers:
  1,1 value: declares #1
  1,21 value: declares #3
```

A bare field name declares the field and references the surrounding name.

```ting
value = 1 in record(value)
```

```ting-output
Declarations:
  #1 value: local (in at 1,11)
  #2 this: this (record at 1,14)
  #3 value: field (record at 1,14)
Identifiers:
  1,1 value: declares #1
  1,21 value: declares field #3, value references #1
```

```ting
record(value)
```

```ting-output
Declarations:
  #1 this: this (record at 1,1)
  #2 value: field (record at 1,1)
Identifiers:
  1,8 value: declares field #2, value unresolved
Errors:
  (1,8-1,13): unresolved name 'value'
```

Fields of the record being defined are reached through `this`.

```ting
record(a = 1, b = this.a + 1)
```

```ting-output
Declarations:
  #1 this: this (record at 1,1)
  #2 a: field (record at 1,1)
  #3 b: field (record at 1,1)
Identifiers:
  1,8 a: declares #2
  1,15 b: declares #3
  1,19 this: references #1
  1,24 a: deferred: property
```

Publishing contexts nest and unify.

```ting
public protected internal Answer = 42
```

```ting-output
Declarations:
  #1 Answer: published (internal at 1,18; protected at 1,8; public at 1,1)
Identifiers:
  1,27 Answer: declares #1
```

A compilation unit has no top-level declarative scope.

```ting
x = 5
```

```ting-output
Identifiers:
  1,1 x: unresolved
Errors:
  (1,1-1,2): unresolved name 'x'
```
