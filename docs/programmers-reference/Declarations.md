## Declarations

Ting has no dedicated declaration syntax. Identifiers are declared inline, by appearing in a declarative scope.

    let a = 21 in a * 2     // expression value is 42

If you know other languages with "let bindings", this may look familiar. In Ting, though, the important part is not the surface form. The important part is scope.

`=` is not an assignment operator here; it is a relational operator. The left-hand identifier is only declared because it appears in a declarative position.

When one or more declarative scopes cover an expression, the expression is said to be declarative. When only referential scopes cover an expression, the expression is said to be referential.

Whether an identifier is being declared or referenced depends entirely on the scope in which it appears.

So in the above example this is what goes on: 
- `let` introduces a new local scope (covers only the `let` expression itself) and makes its operand *declarative*
- `in` splits this declarative expression into a declarative left side `a = 21` and a referential right side `a * 2`.
- `=` further splits the declarative left side from `in` into a declarative left side `a` and a referential right side `21`.

So the expression declares a variable `a` *locally within the expression*, binds it to the value `21` and the entire expression is then the value `a * 2`, i.e. `42`.


### Declarative and referential scopes

Every expression evaluates in either a *declarative* or a *referential* scope:

- In a **declarative scope**, an identifier not yet known in the current scope is added to that scope (i.e., declared).
- In a **referential scope**, an identifier must already be declared in the current scope or an enclosing one; if it is not found, an error is reported.

Scopes can be nested. When resolving an identifier the innermost scope is searched first. If not found there, the search continues outward until the identifier is found or there are no more scopes, in which case an error is reported.

The expression

    let x = 5 in 2*x^2 + 5*x - 10 

contains both declarative and referential sub-expressions:

- `let` creates a new declarative scope spanning `x = 5 in 2*x^2 + 5*x - 10`.
- `x` to the left of `=` is *declarative* — it is being declared here.
- The `x`'s after `in` are *referential* — they reference the `x` declared above.

`let` is a local scope operator. It often appears together with `in`, but it does not require `in`.

`in` is a separate operator. It is right-referential, so when it appears in a declarative scope the left side stays declarative and the right side becomes referential.

`in` is still useful in purely referential positions, because constrains the value of the right operand to those that satisfy the left operand proposition. `x>0 in x` means that the value of this expression (which is just `x`) is only defined if x is greater than zero.

    let int a = x * 2 & a = y * 3

This uses `let` without `in`. It constrains `x` and `y` to combinations of values for which there exists an integer `a` such that `a = x * 2` and `a = y * 3`. In that sense, `let` behaves like an existential constraint over a local scope.

### Declaring operators

The following unary operators establish a new declarative scope over their operand:

| Operator | Scope created |
|---|---|
| `public` | Module-level with public visibility |
| `private` | Module-level with private visibility |
| `protected` | Module-level with protected visibility |
| `local` | Module-level with file-scoped visibility |
| `record` | Record scope over its operand |
| `records` | Record scope over each record in the resulting set |
| `tuple` | Tuple scope over its operand |
| `tuples` | Tuple scope over each tuple in the resulting set |
| `fn` | Function-definition scope over its operand |

### Local declarative operators

The following binary operators establish a new declarative local scope over both operands. Because these operators are also right-referential, the scope is only declarative for the left operand:

| Operator | Description |
|---|---|
| `let` | Local scope introducer |
| `=>` | Lambda function |
| `\` | Such that |


### Right-referential operators

Some operators, when appearing in a declarative scope, keep the declarative scope for their *left* operand but switch to a *referential* scope for their *right* operand. These are called **right-referential** operators:

| Operator | Description |
|---|---|
| `=` |  |
| `==` |  |
| `!==` |  |
| `<` |  |
| `<=` |  |
| `>` |  |
| `>=` |  |
| `:` |  |
| `::` |  |
| `:::` |  |
| `in` |  |



This is why `public y = x * 2` works as expected: `public` establishes a declarative scope; `=` keeps that declarative scope for `y` (which is declared) but switches to referential for `x * 2` (where `x` must already be known).

The same rule explains why `let x = 5 in x + 2` works: `let` opens the local declarative scope, `x = 5` declares `x`, and `in` makes the right-hand expression referential with respect to that binding.

### Operator reference

#### `let`

`let` creates a new declarative local scope. Bindings introduced by `let` are not visible outside the expression. It is often paired with `in`, but it does not have to be:

    let answer = 42 in $"The answer is {answer}"    // "The answer is 42"

`let` also supports recursive bindings via `?=` (see `?=` below):

    let f ?= fn{ 0 -> 1, int n?>0 -> n * f(n-1) }
    in  f 5    // 120  (5!)

    // Quicksort
    let qs ~ Span.Of Comparable a => let p=a.[0] in qs(a??<p) + (a??==p) + qs(a??>p)

`let` can also act as an existential-style constraint when it is combined with a right- or left-referential operator instead of `in`:

    let int a = x * 2 & a = y * 3

Here `let` does not introduce a binding that escapes. It only establishes a local scope in which the constraint can be satisfied.

#### `public`, `private`, `protected`, `local`

These visibility modifiers declare identifiers in the enclosing module or record with the specified visibility. They establish a declarative scope over their operand:

    public y = x * 2    // declares y; x is referenced from an outer scope

#### `in`

`in` is a right-referential operator. It is typically combined with `let` to scope bindings over the right-hand expression, but it is not part of `let` itself:

    let f = (0=>1)||(x=>x*f(x-1))
    in  f 5    // 120  (5!)

The Fibonacci sequence as a lazy list:

    [ let f = (a,b) => a ; f(b,a+b) in f(0,1) ]

#### `=>` — lambda

`=>` creates a function. Its left operand (the parameter pattern) is *declarative*; the declared parameter names are then in scope as a referential context for the right operand (the body):

    x => x * 2              // maps x to x * 2
    (a, b) => a + b         // function of a two-element tuple

Parameter patterns may include type constraints and guards:

    int n?>0 => n * 2       // defined only for positive integers
    string s => s + rest => rest

#### `->` — ordered pair

`->` creates an ordered pair (pattern → value). It is used inside `fn{...}` to define function cases. The left side is a declarative pattern; the right side is the result with the pattern bindings in scope:

    let f = fn{ 0 -> 1, 1 -> 1, int n?>1 -> n * f(n-1) } in f

#### `\` — such that

`\` (read *such that*) constrains a value by a predicate. The left operand is declarative and the right operand is a referential predicate over those names. In other words the operator creates a new local scope and is right-referential within that scope:

    { int n \ n > 0 }           // the set of positive integers
    { int n \ n % 2 == 0 }      // the set of even integers


#### `record` and `records`

`record` is a prefix operator whose operand is *declarative*. All identifiers declared by the operand become fields of the record. `this` is implicitly bound to the record instance inside the operand:

    person  = record(Name:string, Age:int)
    persons = records(Name:string, Age:int)    // shorthand for { record(...) }

#### `?=` — unify

`?=` combines declaration with unification semantics. If the right-hand value cannot be unified with the left-hand pattern the expression is *undefined* (i.e., the branch fails). This enables recursive self-referential bindings — a name bound with `?=` is in scope on the right-hand side:

    // Anonymous factorial function
    let f ?= fn{ 0 -> 1, int n?>0 -> n * f(n-1) }

    // Guarded recursive binding
    let fact ?= int n?>=0 => n==0 then 1 else n * fact(n-1) in fact 6    // 720

    // Tuple destructuring
    let (x,y) = (0.7, 0.7) in x + y

### Declaring `this`

When `this` appears in a declarative position, the current scope is extended with whatever `this` is bound to, making all members of that value directly accessible without qualification.

The following is a function which accepts a DateTime member and formats it as a string:

    DateTime this => $"{Year}-{Month}-{Day}"

Because `this` is in the declarative parameter position of the lambda and its type can be inferred as `DateTime`, all `DateTime` members (`Year`, `Month`, `Day`, `Hour`, `Minute`, `Second` etc) are in scope in the lambda body without qualification.

    let this = DateTime.Now in $"{Year}-{Month}-{Date}"


### Examples

The following is a record with a field `Answer` which is bound to the value 42:

    record(Answer:int) \ Answer=42

Here there are 2 scopes in play:
1. The `\` operator introduces a local scope which is declarative in its left hand operand (`record(Answer:int)`).
2. The `record` operator introduces a *record scope* covering the field expression (`(Answer:int)`).

The identifier `Answer` occurs within a scope stack where both these scopes are declarative. Thus it declares Answer in both scopes and unifies the values.


The following function parses a string:

    $"{Word name} = {int value}" => (name,value)

The parameter pattern is declarative: it declares `name` and `value` inside the lambda body. The body is referential, so it can return the declared names as a tuple.