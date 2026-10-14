## Declarations

Ting has no dedicated declaration syntax. Identifiers are declared inline, by appearing in a declarative scope.

    a = 21 in a * 2     // expression value is 42

If you know other languages with "let bindings", this may look familiar. In Ting, though, the important part is not the surface form. The important part is scope.

`=` is not an assignment operator here; it is a relational operator. The left-hand identifier is only declared because it appears in a declarative position.

When one or more declarative scopes cover an expression, the expression is said to be declarative. When only referential scopes cover an expression, the expression is said to be referential.

Whether an identifier is being declared or referenced depends entirely on the scope in which it appears.

So in the above example this is what goes on: 
- `in` introduces a new local scope covering the expression. The scope is *declarative* in its left side `a = 21` and *referential* in its right side `a * 2`.
- `=` further splits the declarative left side from `in` into a declarative left side `a` and a referential right side `21`.

So the expression declares a variable `a` *locally within the expression*, binds it to the value `21` and the entire expression is then the value `a * 2`, i.e. `42`.


### Declarative and referential scopes

Every expression evaluates in either a *declarative* or a *referential* scope:

- In a **declarative scope**, an identifier not yet known in the current scope is added to that scope (i.e., declared).
- In a **referential scope**, an identifier must already be declared in the current scope or an enclosing one; if it is not found, an error is reported.

Scopes can be nested. When resolving an identifier the innermost scope is searched first. If not found there, the search continues outward until the identifier is found or there are no more scopes, in which case an error is reported.

The expression

    x = 5 in 2*x^2 + 5*x - 10 

contains both declarative and referential sub-expressions:

- `in` creates a new local scope spanning `x = 5 in 2*x^2 + 5*x - 10`, declarative on its left side and referential on its right side.
- `x` to the left of `=` is *declarative* — it is being declared here.
- The `x`'s after `in` are *referential* — they reference the `x` declared above.

`let` is a separate local scope operator. `in` does not need it, because `in` introduces its own scope. `let` is used where no other operator supplies a declarative scope, for example `let int x \ x != 0`.

`in` is left-declarative: the names declared on its left side are local to the `in` expression. In `public a = 42 in Answer = a`, `a` stays local and only `Answer`, which the right side declares in the scope of `public`, is published.

`in` also constrains the value of the right operand to those that satisfy the left operand proposition. `0 < x in x` is only defined where `x` is greater than zero. On the left, `x` is on the referential side of `<`, so it refers to the surrounding `x` rather than declaring a new one.

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

The following binary operators establish a new local scope over both operands. The scope is declarative for the left operand and referential for the right operand:

| Operator | Description |
|---|---|
| `in` | Conditional value |
| `=>` | Lambda function |
| `all` | Universal quantification |
| `contract` | Function contract |

`where` is the mirror image: its scope is referential for the left operand and declarative for the right operand. The unary `let` establishes a local scope that is declarative throughout its operand.


### Right-referential operators

Some operators, when appearing in a declarative scope, keep the declarative scope for their *left* operand but switch to a *referential* scope for their *right* operand. These are called **right-referential** operators:

| Operator | Description |
|---|---|
| `=` |  |
| `?=` |  |
| `==` |  |
| `!=` |  |
| `<` |  |
| `<=` |  |
| `>` |  |
| `>=` |  |
| `:` |  |
| `::` |  |
| `:::` |  |
| `\` | Such that |
| `?` | Restriction |
| `->` | Ordered pair |

A right-referential operator introduces no scope of its own: the names its left operand declares belong to the innermost declarative scope covering it.

This is why `public y = x * 2` works as expected: `public` establishes a declarative scope; `=` keeps that declarative scope for `y` (which is declared) but switches to referential for `x * 2` (where `x` must already be known).

The same rule explains why `x = 5 in x + 2` works: `in` opens a local scope, `x = 5` declares `x` on its declarative left side, and the right side refers to that binding.

### Operator reference

#### `let`

`let` creates a new declarative local scope. Bindings introduced by `let` are not visible outside the expression. Combined with `in` it adds a second scope around the one `in` already introduces:

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

`in` is a left-declarative operator. It introduces a local scope that is declarative over its left operand and referential over its right operand, so it needs no `let`:

    f = (0=>1) || (x=>x*f(x-1))
    in  f 5    // 120  (5!)

The Fibonacci sequence as a lazy list:

    [ f = (a,b) => a ; f(b,a+b) in f(0,1) ]

#### `=>` — lambda

`=>` creates a function. Its left operand (the parameter pattern) is *declarative*; the declared parameter names are then in scope as a referential context for the right operand (the body):

    x => x * 2              // maps x to x * 2
    (a, b) => a + b         // function of a two-element tuple

Parameter patterns may include type constraints and guards:

    int n?>0 => n * 2       // defined only for positive integers
    string s => s + rest => rest

#### `->` — mapping

`->` creates a mapping: an ordinary product that fuses the left side, one level deep, with a named field `Result` bound to the right side (`x -> y` ≡ `(x @ Result = y)`). It is used inside `fn{...}` to define function cases. The left side is a declarative pattern; the right side is the result with the pattern bindings in scope:

    let f = fn{ 0 -> 1, 1 -> 1, int n?>1 -> n * f(n-1) } in f

#### `\` — such that

`\` (read *such that*) constrains a value by a predicate. It is right-referential, like `->` and `?`: the left operand is ambient and the right operand is a referential predicate over the names the left operand declares. `\` introduces no scope of its own, so those names are declared in the innermost declarative frame; in a set definition that is normally an explicit `let`:

    { let int n \ n > 0 }           // the set of positive integers
    { let int n \ n % 2 == 0 }      // the set of even integers


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

    record(Answer : int \ (this.Answer = 42))

`\` introduces no scope of its own. `Answer` is declared in the record scope, the innermost declarative frame, so it becomes a field. Inside a record, fields are referred to through `this`, so the restriction on the right of `\` is written `this.Answer = 42`.


The following function parses a string:

    $"{Word name} = {int value}" => (name,value)

The parameter pattern is declarative: it declares `name` and `value` inside the lambda body. The body is referential, so it can return the declared names as a tuple.