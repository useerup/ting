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
