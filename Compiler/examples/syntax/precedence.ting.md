# Operator precedence and associativity

One example per row of the specification's binary infix table, loosest first. Associativity is provisional (specification issue SYN-01).

```ting
a where b = 1 where c = 2
```

```ting-output
(where a (where (= b 1) (= c 2)))
```

```ting
x = 1 in y = 2 in x + y
```

```ting-output
(in (= x 1) (in (= y 2) (+ x y)))
```

```ting
f ?= g = h
```

```ting-output
(= (?= f g) h)
```

```ting
a then b else c then d else e
```

```ting-output
(then a (else b (then c (else d e))))
```

```ting
a -> b -> c
```

```ting-output
(-> (-> a b) c)
```

```ting
x |> f >> g <| y
```

```ting-output
(<| (>> (|> x f) g) y)
```

```ting
x => y => x + y
```

```ting-output
(=> x (=> y (+ x y)))
```

```ting
r ./ s ../ v
```

```ting-output
(../ (./ r s) v)
```

```ting
int x \ x != 0 \ x < 10
```

```ting-output
(\ (\ (apply int x) (!= x 0)) (< x 10))
```

```ting
a | b || c ||| d & e && f &&& g
```

```ting-output
(||| (|| (| a b) c) (&&& (&& (& d e) f) g))
```

```ting
x ? p ?? q
```

```ting-output
(?? (? x p) q)
```

```ting
x : A & y :: B | z ::: C
```

```ting-output
(| (& (: x A) (:: y B)) (::: z C))
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

The specification lists the ranges below `;`, so `;` binds tighter and the next expression reads as `1 ... (3 ; 5) <...< 9`.

```ting
1 ... 3 ;  5 <...< 9
```

```ting-output
error (1,14-1,19): '...' and '<...<' do not associate; add parentheses
```

```ting
0 ; 1 ;; 2
```

```ting-output
(;; (; 0 1) 2)
```

```ting
A ^^ B ^^ C
```

```ting-output
(^^ A (^^ B C))
```

```ting
A ** B * C
```

```ting-output
(** A (* B C))
```

```ting
a + b * c ^ d - e / f % g /% h
```

```ting-output
(- (+ a (* b (^ c d))) (/% (% (/ e f) g) h))
```

```ting
a.b..c.d
```

```ting-output
(. (.. (. a b) c) d)
```

Juxtaposition binds tighter than every infix operator, including `.`.

```ting
f a.b
```

```ting-output
(. (apply f a) b)
```

```ting
Console.WriteLine message
```

```ting-output
(. Console (apply WriteLine message))
```
