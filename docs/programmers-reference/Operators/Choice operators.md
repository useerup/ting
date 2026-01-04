### Choice operators



#### The `;` *choice* operator

The `;` *choice* operator creates a choice between two values

The value of `a ; b` is nondeterministically `a` or `b`. At any one time the expression can be only one of the values, but backtracking can cause the expression to choose the other value. The operator introduces *ordered nondeterminism* in that it will choose the value of `a` until backtracking forces it to choose the value of `b`

If any of the operands are undefined they are disregarded. Only if both operands are undefined is the value of the expression undefined.

Choice operator is handy when defining *generator functions* together with set- or list definitions to produce an procedurally defined set or list.

##### Example: List of Fibonacci numbers

The following is an example of a list of Fibonacci numbers:

    [ let fib = (a,b)=>a;fib(b,a+b) in fib(0,1) ]

The list evaluates to `[0, 1, 1, 2, 3, 5, …]`, i.e. the Fibonacci sequence, because successive backtracking steps force each new choice produced by the generator. The list is - in principle - infinite, as no upper bound has been specified. This means that the numbers will be generated on-demand. However, the `int` set is a 32-bit signed integer and the generator *will* cause an overflow when the list is enumerated past the overflow point.

Step-by-step:

1. `[` `]` is a [list definition](../Lists.md#list-definitions). The literal contains a single expression, so the list is formed by unwinding every value that expression can produce.
2. `let … in …` introduces a local binding. The declarative part `fib = (a,b) => a ; fib(b, a+b)` must succeed first; once it does, the right-hand expression `fib(0,1)` is evaluated using that binding.
3. The local `fib` function is the generator. The choice operator `;` yields `a` first; only when backtracking demands another value does it evaluate `fib(b, a+b)` which in turn yields the next Fibonacci number. Because `;` is ordered, the sequence the list observes is precisely the Fibonacci sequence.
4. The call `fib(0,1)` seeds the generator with the canonical starting pair. The enclosing list literal consumes each emitted value, giving a deterministic list even though the generator itself is nondeterministic.




#### The `;;` *exclusive choice* operator

The value of `a ;; b` is that of `a` *unless* `a` is undefined in which case it is `b`.

An `;;` expression will not by itself introduce nondeterminism, though `a` and `b` may themselves be nondeterministic expressions.

`a ;; b` is essentially equivalent to `(a catch UndefinedExpressionException _ => b)`, i.e. evaluate `a` and if an `UndefinedExpressionException` is raised from that evaluation then return the value of `b` instead.

