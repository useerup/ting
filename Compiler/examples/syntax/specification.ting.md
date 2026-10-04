# Specification examples

Expressions taken from the language specification.

```ting
((value = 1) & (value = 1)) in value
```

```ting-output
(in (& (= value 1) (= value 1)) value)
```

```ting
value = 1 in (value = 2 in value)
```

```ting-output
(in (= value 1) (in (= value 2) value))
```

```ting
value = 1 in record(value)
```

```ting-output
(in (= value 1) (record value))
```

```ting
this = 1 in (this = 2 in this)
```

```ting-output
(in (= this 1) (in (= this 2) this))
```

```ting
(int x => x * 2 + k) \ (k = 42)
```

```ting-output
(\ (=> (apply int x) (+ (* x 2) k)) (= k 42))
```

```ting
(int x => c) \ (c = x * 2)
```

```ting-output
(\ (=> (apply int x) c) (= c (* x 2)))
```

```ting
float x ? != 0 => total (1 / x)
```

```ting-output
(=> (? (apply float x) (section != 0)) (total (/ 1 x)))
```

```ting
NonZeroArg = { x ? != 0 => total _ }
```

```ting-output
(= NonZeroArg (set (=> (? x (section != 0)) (total _))))
```

```ting
Reciprocal =
    float x => 1 / x
    contract (float ^^ float) & NonZeroArg & Forward
```

```ting-output
(=
  Reciprocal
  (contract
    (=> (apply float x) (/ 1 x))
    (& (& (^^ float float) NonZeroArg) Forward)))
```

```ting
{
    (int a, int b) => int c \
        (bound(a, b) & free(c) |
         bound(a, c) & free(b) |
         bound(b, c) & free(a))
}
```

```ting-output
(set
  (=>
    (tuple (apply int a) (apply int b))
    (\
      (apply int c)
      (|
        (|
          (& (apply bound (tuple a b)) (apply free c))
          (& (apply bound (tuple a c)) (apply free b)))
        (& (apply bound (tuple b c)) (apply free a))))))
```

```ting
public Sum = public(int a, int b) => a + b
```

```ting-output
(public
  (= Sum (public (=> (tuple (apply int a) (apply int b)) (+ a b)))))
```

```ting
Sum(1 @ b = 2)
```

```ting-output
(apply Sum (product 1 @ (= b 2)))
```

```ting
let f ?= fn{ 0 -> 1, int n -> n * f (n - 1) } in f 3
```

```ting-output
(let
  (in
    (?=
      f
      (fn (set (-> 0 1) (-> (apply int n) (* n (apply f (- n 1)))))))
    (apply f 3)))
```

```ting
(x -> y).Result
```

```ting-output
(. (-> x y) Result)
```

```ting
let F = int n => n < 0 then void else n * 2
```

```ting-output
(let (= F (=> (apply int n) (then (< n 0) (else void (* n 2))))))
```

```ting
{ record(value = 1), record(other = 2) } ../ value
```

```ting-output
(../ (set (record (= value 1)) (record (= other 2))) value)
```

```ting
record(value = 1) ./ value
```

```ting-output
(./ (record (= value 1)) value)
```

```ting
Persons person => person./record(Name,Age=CalculateAge(DateOfBirth,DateOnly.ToDay))
```

```ting-output
(=>
  (apply Persons person)
  (./
    person
    (record
      (tuple
        Name
        (=
          Age
          (apply CalculateAge (tuple DateOfBirth (. DateOnly ToDay))))))))
```

```ting
expr catch fn{
    (ArgumentException _) -> "Bad arg",
    (NullReferenceException _) -> "Null ref",
    _ -> "Other error"
}
```

```ting-output
(catch
  expr
  (fn
    (set
      (-> (apply ArgumentException _) "Bad arg")
      (-> (apply NullReferenceException _) "Null ref")
      (-> _ "Other error"))))
```

```ting
(ask 0 answer (int request => partial (request ? != 0))) answer (int request => 42)
```

```ting-output
(answer
  (answer
    (ask 0)
    (=> (apply int request) (partial (? request (section != 0)))))
  (=> (apply int request) 42))
```

```ting
ReadPort = (() => ask "port") asks (string ^^ int)
in
    ReadPort () answer (string request => 8080)
```

```ting-output
(in
  (= ReadPort (asks (=> (tuple) (ask "port")) (^^ string int)))
  (answer (apply ReadPort (tuple)) (=> (apply string request) 8080)))
```

```ting
{1, 2} + {2, 3}   =   {* 1, 2, 2, 3 *}
```

```ting-output
(= (+ (set 1 2) (set 2 3)) (multiset 1 2 2 3))
```
