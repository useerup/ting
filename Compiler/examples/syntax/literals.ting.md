# Numbers and literals

```ting
[42, 667_776, 3.14, 123u, 123s, 123us, 123L, 123UL, 123f, 123d, 123.45d, 123.45m]
```

```ting-output
(list
  42
  667776
  3.14
  123u
  123s
  123us
  123L
  123UL
  123f
  123d
  123.45d
  123.45m)
```

```ting
[true, false, void, this, (), [], {}, {* *}]
```

```ting-output
(list true false void this (tuple) (list) (set) (multiset))
```

A fraction needs a digit after the point, so ranges and properties are not swallowed.

```ting
1...n
```

```ting-output
(... 1 n)
```

```ting
2147483648
```

```ting-output
error (1,1-1,11): numeric literal '2147483648' is not representable as int
```

```ting
1.5u
```

```ting-output
error (1,1-1,5): numeric literal '1.5u' is not representable as uint
```

```ting
1__000
```

```ting-output
error (1,2-1,3): a digit separator must be followed by a digit
```

```ting
12abc
```

```ting-output
error (1,1-1,4): invalid numeric literal suffix
```
