# Translation of functions and sets

A lambda becomes a term over the names it captures. An axiom defines `app`, the relation between a function, an argument and a result: the argument matches the pattern and the body yields the result. The axioms are listed after the value; the prover assumes them, together with the fact that distinct literals are different values.

```ting
(x => x + 1) 2
```

```ting-output
Value:
  app(lambda1, 2, value)
Axioms:
  v1 : any & v2 : any all app(lambda1, v1, v2) == (x_1 : any & (v1 = x_1) & (v2 = x_1 + 1))
```

A lambda bound to a name is applied through the name.

```ting
f = (x => x + 1) in f 2
```

```ting-output
Value:
  f_1 : any & (f_1 = lambda1) & app(f_1, 2, value)
Axioms:
  v1 : any & v2 : any all app(lambda1, v1, v2) == (x_2 : any & (v1 = x_2) & (v2 = x_2 + 1))
```

A pattern can restrict the argument.

```ting
(int x => x * 2) 21
```

```ting-output
Value:
  app(lambda1, 21, value)
Axioms:
  v1 : any & v2 : any all app(lambda1, v1, v2) == (x_1 : any & (v1 = x_1) & v1 : int & (v2 = x_1 * 2))
```

Captured names become arguments of the lambda term. The parameter `x` stays inside the lambda; `k` is shared with the surrounding expression.

```ting
let (int x => x * 2 + k) \ (k = 42)
```

```ting-output
Value:
  k_2 : any & (value = lambda1 k_2) & (k_2 = 42)
Axioms:
  k_2 : any & v1 : any & v2 : any all app(lambda1 k_2, v1, v2) == (x_1 : any & (v1 = x_1) & v1 : int & (v2 = x_1 * 2 + k_2))
```

A set definition becomes a term with a membership axiom. Used as a function, a set is the identity restricted to its members.

```ting
{ 1, 2 }
```

```ting-output
Value:
  value = set1
Axioms:
  v1 : any all v1 : set1 == ((v1 = 1) | (v1 = 2))
  v2 : any & v3 : any all app(set1, v2, v3) == (v2 : set1 & (v3 = v2))
```

```ting
S = { 1, 2 } in 2 : S
```

```ting-output
Value:
  S_1 : any & (S_1 = set1) & (2 : S_1 & (value = true) | !(2 : S_1) & (value = false))
Axioms:
  v1 : any all v1 : set1 == ((v1 = 1) | (v1 = 2))
  v2 : any & v3 : any all app(set1, v2, v3) == (v2 : set1 & (v3 = v2))
```

In a set of ordered pairs the first contribution at an origin wins. `fn` turns the set into the function that maps each origin to its result. Ordered pairs are injective: equal pairs have equal components.

```ting
f = fn { 0 -> 1, 0 -> 2 } in f 0
```

```ting-output
Value:
  f_1 : any & (f_1 = fn set1) & app(f_1, 0, value)
Axioms:
  x1 : any & x2 : any & y1 : any & y2 : any & (x1 -> x2 = y1 -> y2) all (x1 = y1) & (x2 = y2)
  v1 : any & v2 : any all (v1 -> v2) : set1 == ((v1 = 0) & (v2 = 1) | (v1 = 0) & (v2 = 2) & !(v1 = 0))
  v4 : any & v5 : any all app(set1, v4, v5) == (v4 : set1 & (v5 = v4))
  s : any & a : any & r : any all app(fn s, a, r) == ((a -> r) : s)
```

A recursive function refers to itself through the name it is bound to.

```ting
f = fn { 0 -> 1, let int n -> n * f (n - 1) } in f 3
```

```ting-output
Value:
  f_1 : any & (f_1 = fn (set1 f_1)) & app(f_1, 3, value)
Axioms:
  x1 : any & x2 : any & y1 : any & y2 : any & (x1 -> x2 = y1 -> y2) all (x1 = y1) & (x2 = y2)
  f_1 : any & v1 : any & v2 : any all (v1 -> v2) : set1 f_1 == ((v1 = 0) & (v2 = 1) | (n_2 : any & n_2 : int & (v6 : any & app(f_1, n_2 - 1, v6) & (v2 = n_2 * v6)) & (v1 = n_2)) & !(v1 = 0))
  f_1 : any & v7 : any & v8 : any all app(set1 f_1, v7, v8) == (v7 : set1 f_1 & (v8 = v7))
  s : any & a : any & r : any all app(fn s, a, r) == ((a -> r) : s)
```

A restriction by a general predicate applies the predicate to the value.

```ting
p = (x => x > 0) in 5 ? p
```

```ting-output
Value:
  p_1 : any & (p_1 = lambda1) & (value = 5) & app(p_1, value, true)
Axioms:
  v1 : any & v2 : any all app(lambda1, v1, v2) == (x_2 : any & (v1 = x_2) & (x_2 > 0 & (v2 = true) | !(x_2 > 0) & (v2 = false)))
```
