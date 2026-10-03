# Prefix operators

Operand extents are provisional (specification issue SYN-01).

```ting
let f = x => x in f 1
```

```ting-output
(let (in (= f (=> x x)) (apply f 1)))
```

```ting
public protected internal Answer = 42
```

```ting-output
(public (protected (internal (= Answer 42))))
```

```ting
!x : A & y
```

```ting-output
(& (! (: x A)) y)
```

```ting
fn { 1 -> 2, 1 -> 3 }
```

```ting-output
(fn (set (-> 1 2) (-> 1 3)))
```

```ting
fn{0 -> 1} 0
```

```ting-output
(fn (apply (set (-> 0 1)) 0))
```

```ting
record(Name = "Bob", Age = 42)
```

```ting-output
(record (tuple (= Name "Bob") (= Age 42)))
```

```ting
records(string Name, int Age)
```

```ting-output
(records (tuple (apply string Name) (apply int Age)))
```

```ting
Writers = class string
```

```ting-output
(= Writers (class string))
```

```ting
primary a = primary b
```

```ting-output
(= (primary a) (primary b))
```

```ting
candidate x.Payload
```

```ting-output
(candidate (. x Payload))
```

```ting
tuples(int a, float b)
```

```ting-output
(tuples (tuple (apply int a) (apply float b)))
```
