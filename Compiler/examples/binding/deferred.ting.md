# Occurrences that need semantic analysis

Properties are resolved against their receiver, not lexically.

```ting
let p = record(Name = "Bob") in p.Name
```

```ting-output
Declarations:
  #1 p: local (in at 1,30; let at 1,1)
  #2 this: this (record at 1,9)
  #3 Name: field (record at 1,9)
Identifiers:
  1,5 p: declares #1
  1,16 Name: declares #3
  1,33 p: references #1
  1,35 Name: deferred: property
```

A property declared through property access is hosted on its receiver.

```ting
let int..Double = x => x * 2 in 21.Double
```

```ting-output
Declarations:
  #1 Double: hosted on members of int (in at 1,30; let at 1,1)
  #2 x: local (=> at 1,21)
Identifiers:
  1,5 int: references builtin
  1,10 Double: declares #1
  1,19 x: declares #2
  1,24 x: references #2
  1,36 Double: deferred: property
```

Inside a lambda whose parameter is `this`, a name not found closer may be a property of `this`.

```ting
let int..Double = x => x * 2 in int this => Double
```

```ting-output
Declarations:
  #1 Double: hosted on members of int (in at 1,30; let at 1,1)
  #2 x: local (=> at 1,21)
  #3 this: this (=> at 1,42)
Identifiers:
  1,5 int: references builtin
  1,10 Double: declares #1
  1,19 x: declares #2
  1,24 x: references #2
  1,33 int: references builtin
  1,37 this: declares #3
  1,45 Double: deferred: implicit this of => at 1,42
```

`./` and `../` expose names that are only known once their left operand is analyzed.

```ting
record(value = 1) ./ value
```

```ting-output
Declarations:
  #1 this: this (record at 1,1)
  #2 value: field (record at 1,1)
Identifiers:
  1,8 value: declares #2
  1,22 value: deferred: names of ./ at 1,19
```

```ting
{ record(value = 1), record(value = 2) } ../ value
```

```ting-output
Declarations:
  #1 this: this (record at 1,3)
  #2 value: field (record at 1,3)
  #3 this: this (record at 1,22)
  #4 value: field (record at 1,22)
Identifiers:
  1,10 value: declares #2
  1,29 value: declares #4
  1,46 value: deferred: names of ../ at 1,42
```

Interpolated identifiers are compared after their replacement blocks are evaluated.

```ting
let n = 1 in $`Name{n}`
```

```ting-output
Declarations:
  #1 n: local (in at 1,11; let at 1,1)
Identifiers:
  1,5 n: declares #1
  1,14 Name{}: deferred: interpolated name
  1,21 n: references #1
```

Sections of `.` denote property access.

```ting
let people = {} in people |> map .Name
```

```ting-output
Declarations:
  #1 people: local (in at 1,17; let at 1,1)
Identifiers:
  1,5 people: declares #1
  1,20 people: references #1
  1,30 map: unresolved
  1,35 Name: deferred: property
Errors:
  (1,30-1,33): unresolved name 'map'
```
