# Brackets and products

```ting
[(), (a), (a, b), (a, b, c)]
```

```ting-output
(list (tuple) a (tuple a b) (tuple a b c))
```

Products: positional section, `@`, named section.

```ting
(p1, p2 @ n1 = v1, n2 = v2)
```

```ting-output
(product p1 p2 @ (= n1 v1) (= n2 v2))
```

```ting
(x @ Result = y)
```

```ting-output
(product x @ (= Result y))
```

```ting
(x @@ Result = y)
```

```ting-output
(product x @@ (= Result y))
```

Collections.

```ting
[[1, 2], {3, 4}, {* 5, 5 *}]
```

```ting-output
(list (list 1 2) (set 3 4) (multiset 5 5))
```

```ting
(a, b
```

```ting-output
error (1,6-1,6): unexpected end of input, expected ',', '@', '@@' or ')'
```

```ting
(a @)
```

```ting-output
error (1,5-1,6): unexpected ')', expected an expression
```
