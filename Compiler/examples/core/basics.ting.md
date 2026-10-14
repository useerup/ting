# Translation of the first-order core

Each program becomes a formula relating it to its `value`. Several solutions are several values; an undefined program has none.

Literals and arithmetic are functional: the value is a term.

```ting
42
```

```ting-output
Value:
  value = 42
```

```ting
1 + 2 * 3
```

```ting-output
Value:
  value = 1 + 2 * 3
```

Division is defined only for a non-zero divisor.

```ting
1 / 0
```

```ting-output
Value:
  false
```

`in` quantifies the names its left operand declares; the left operand is a goal.

```ting
x = 1 in x + 1
```

```ting-output
Value:
  x_1 : any & (x_1 = 1) & (value = x_1 + 1)
```

```ting
x : int & x > 10 in x
```

```ting-output
Value:
  x_1 : any & x_1 : int & x_1 > 10 & (value = x_1)
```

`\` restricts a value; with `let` the name is quantified in the frame of `let`.

```ting
let int x \ x > 0
```

```ting-output
Value:
  x_1 : any & (value = x_1) & value : int & x_1 > 0
```

`;` offers both values, `;;` uses the right operand only where the left one is undefined.

```ting
0 ; 1
```

```ting-output
Value:
  (value = 0) | (value = 1)
```

```ting
1 / 0 ;; 7
```

```ting-output
Value:
  value = 7
```

A proposition in a value position yields `true` where it holds and `false` where it does not.

```ting
x = 3 in x > 0
```

```ting-output
Value:
  x_1 : any & (x_1 = 3) & (x_1 > 0 & (value = true) | !(x_1 > 0) & (value = false))
```

`&` and `|` are strict: both operands must be defined. The guarded forms are not.

```ting
x = 3 in x > 0 | x / 0 > 1
```

```ting-output
Value:
  false
```

```ting
x = 3 in x > 0 || x / 0 > 1
```

```ting-output
Value:
  x_1 : any & (x_1 = 3) & x_1 > 0 & (value = true)
```

`all` quantifies universally; a requirement that is undefined for some case makes it `false`.

```ting
x : int all x * 1 == x
```

```ting-output
Value:
  (x_1 : any all !(x_1 : int) | (x_1 * 1 = x_1)) & (value = true) | (x_1 : any & x_1 : int & !(x_1 * 1 = x_1)) & (value = false)
```

`then` and `else` select by the condition.

```ting
x = 3 in x < 0 then 0 - x else x
```

```ting-output
Value:
  x_1 : any & (x_1 = 3) & (x_1 < 0 & (value = 0 - x_1) | !(x_1 < 0) & (value = x_1))
```

Restriction by a relation section.

```ting
x = 3 in x ? > 0
```

```ting-output
Value:
  x_1 : any & (x_1 = 3) & (value = x_1) & value > 0
```

Constructs that are not translated yet are reported.

```ting
x => x
```

```ting-output
Value:
  unsupported
Errors:
  (1,1-1,7): not yet supported: operator '=>'
```

```ting
int x \ x != 0
```

```ting-output
Value:
  (value = x) & value : int & !(x = 0)
Errors:
  (1,5-1,6): unresolved name 'x'
  (1,9-1,10): unresolved name 'x'
```
