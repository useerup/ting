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

`=` and `?=` do not associate.

```ting
f ?= g = h
```

```ting-output
error (1,8-1,9): '?=' and '=' do not associate; add parentheses
```

```ting
f = p => b contract C throws E
```

```ting-output
(= f (throws (contract (=> p b) C) E))
```

```ting
a -> b -> c
```

```ting-output
(-> (-> a b) c)
```

```ting
x |> f >> g << h
```

```ting-output
(<< (>> (|> x f) g) h)
```

`<|` is right-associative while the rest of its row is left-associative, so it cannot be mixed with them without parentheses.

```ting
f <| g <| x
```

```ting-output
(<| f (<| g x))
```

```ting
x |> f <| y
```

```ting-output
error (1,8-1,10): '|>' and '<|' do not associate; add parentheses
```

```ting
e catch h answer k
```

```ting-output
(answer (catch e h) k)
```

A handler can be an unparenthesized lambda.

```ting
expr catch Exception ex => "Error: " + ex.Message
```

```ting-output
(catch expr (=> (apply Exception ex) (+ "Error: " (. ex Message))))
```

```ting
x => y => x + y
```

```ting-output
(=> x (=> y (+ x y)))
```

```ting
a then b else c then d else e
```

```ting-output
(then a (else b (then c (else d e))))
```

A conditional can be a lambda body without parentheses.

```ting
int n => n < 0 then void else n * 2
```

```ting-output
(=> (apply int n) (then (< n 0) (else void (* n 2))))
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

```ting
0 ; 1 ;; 2
```

```ting-output
(;; (; 0 1) 2)
```

The ranges bind tighter than `;`, so this is a choice between two ranges.

```ting
1 ... 3 ;  5 <...< 9
```

```ting-output
(; (... 1 3) (<...< 5 9))
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

`.` and `..` bind tighter than juxtaposition, which binds tighter than every other infix operator.

```ting
f a.b
```

```ting-output
(apply f (. a b))
```

```ting
Console.WriteLine message
```

```ting-output
(apply (. Console WriteLine) message)
```
