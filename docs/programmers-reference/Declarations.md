## Declarations

Identifiers are declared inline within the expression in which they appear. Whether an identifier is being declared or referenced depends on to which scope it is being resolved and whether that scope is *referential* or *declarative*.

Scopes can be nested. When resolving an identifier the innermost scope is considered first. If the identifier can be resolved in that scope, then it is bound to the value of the identifier in that scope. If the identifier can not be resolved in a scope then the next enclosing scope is considered, and so on. The process stops when the identifier has been resolved or there is no more frames, in which case an error is reported.

If a scope is *declarative* and the scope does not already contain the identifier then it is added to that scope.
If a scope is *referential* then the frame is searched for the identifier. If found, then the identifier is bound to that scope. 


### Nested declarative scope


  


### Operators that affect scopes

A number of operators creates scopes and/or converts scopes from declarative to referential.


#### The `let` operator

The `let` operator creates a new declarative local scope spanning its operand. Identifiers declared by `let` is not available outside the expression, i.e. the identifiers are local to the expression. Thus, the `let` operator is often used in combination with other operators such a `in`:

    let answer = 42 in $"The answer is {answer}"    // returns "The answer is 42"

#### The `public`, `private`, `protected`, `local` declarations



#### The `in` operator

The `in` operator is *right-referential*. 

    let f = (0=>1)||(x=>x*f(x-1))
    in  f 5    // calculates factorial 5!

#### The `=>` *lambda* operator

#### The `->` *ordered pair* operator

#### The `\` *such that* operator

#### Unifying expressions

The operators `?` and `=` can be used together to effectively act as a *unify* operator. If unification is not possible, the expression is undefined.

The `?=` operator is right-referential: In declarative context


Examples (aliasing and recursion):

    // Anonymous factorial function
    let f ?= fn{ 0 -> 1, int n?>0 -> n*f(n-1) }

    // The list of Fibonacci numbers
    [ let f = (a,b) => a ; f(b,a+b) in f(0,1) ]

Examples (destructuring/pattern binding):

    // Tuple destructuring
    let ((x,y) = (0.7, 0.7)) in x + y

    // Guarded parameter with refinement in a function alias
    let fact ?= int n?>=0 => n==0 then 1 else n * fact(n-1) in  fact 6                                  // 720




Examples:

    let answer = 42

    x => x * 2

    


    let qs ~ Span.Of Comparable a => let p=a.[0] in qs(a??<p) + (a??==p) + qs(a??>p)


### Declaring `this`

When `this` is in a declarative position, the scope(s) being declared are extended with whatver `this` is bound to.

Example:

    DateTime this => $"{Year}-{Month}-{Date}"

Because 1) `this` appears in a declarative position (argument of a lambda) and 2) it can be inferred that the method accepts a `DateTime` member, `this` is bound to a `DateTime`. 


    let this = DateTime.Now in $"{Year}-{Month}-{Date}"
