# Operator precedence and associativity

One example per row of the specification's binary infix table, loosest first. Associativity is provisional (specification issue SYN-01).

```ting
a + b * c ^ d - e / f % g /% h
```

```ting-output
(- (+ a (* b (^ c d))) (/% (% (/ e f) g) h))
```
