# Operator precedence and associativity

One example per row of the specification's binary infix table, loosest first. Associativity is provisional (specification issue SYN-01).

```ting
f ?= g = h
```

```ting-output
(= (?= f g) h)
```

```ting
a | b || c ||| d & e && f &&& g
```

```ting-output
(||| (|| (| a b) c) (&&& (&& (& d e) f) g))
```

```ting
a < b == c
```

```ting-output
error (1,7-1,9): '<' and '==' do not associate; add parentheses
```

```ting
a < b < c
```

```ting-output
error (1,7-1,8): '<' and '<' do not associate; add parentheses
```

```ting
a + b * c ^ d - e / f % g /% h
```

```ting-output
(- (+ a (* b (^ c d))) (/% (% (/ e f) g) h))
```
