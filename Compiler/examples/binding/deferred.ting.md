# Occurrences that need semantic analysis

Properties are resolved against their receiver, not lexically.

```ting
p = record(Name = "Bob") in p.Name
```

```ting-output
Declarations:
  #1 p: local (in at 1,26)
  #2 this: this (record at 1,5)
  #3 Name: field (record at 1,5)
Identifiers:
  1,1 p: declares #1
  1,12 Name: declares #3
  1,29 p: references #1
  1,31 Name: deferred: property
```

A property declared through property access is hosted on its receiver.

```ting
int..Double = x => x * 2 in 21.Double
```

```ting-output
Declarations:
  #1 Double: hosted on members of int (in at 1,26)
  #2 x: local (=> at 1,17)
Identifiers:
  1,1 int: references builtin
  1,6 Double: declares #1
  1,15 x: declares #2
  1,20 x: references #2
  1,32 Double: deferred: property
```

Inside a lambda whose parameter is `this`, a name not found closer may be a property of `this`.

```ting
int..Double = x => x * 2 in int this => Double
```

```ting-output
Declarations:
  #1 Double: hosted on members of int (in at 1,26)
  #2 x: local (=> at 1,17)
  #3 this: this (=> at 1,38)
Identifiers:
  1,1 int: references builtin
  1,6 Double: declares #1
  1,15 x: declares #2
  1,20 x: references #2
  1,29 int: references builtin
  1,33 this: declares #3
  1,41 Double: deferred: implicit this of => at 1,38
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
n = 1 in $`Name{n}`
```

```ting-output
Declarations:
  #1 n: local (in at 1,7)
Identifiers:
  1,1 n: declares #1
  1,10 Name{}: deferred: interpolated name
  1,17 n: references #1
```

Sections of `.` denote property access.

```ting
people = {} in people |> map .Name
```

```ting-output
Declarations:
  #1 people: local (in at 1,13)
Identifiers:
  1,1 people: declares #1
  1,16 people: references #1
  1,26 map: unresolved
  1,31 Name: deferred: property
Errors:
  (1,26-1,29): unresolved name 'map'
```
