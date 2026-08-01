# Ting Language Specification

> Note: This is a draft


## Design Model

Ting is logic programming language. The logic oriented core allows it to unify the functional- and object-oriented paradigms.


## Lexical Structure

### Tokenization

Longest-match tokenization applies.

Example: `a<=b` tokenizes as `a`, `<=`, `b`.

The character tokens are tokens formed from non-alphanumeric characters, as opposed to
keyword tokens such as `let`, `where` or `catch`.

The following tokens exists:

- Unary prefix operators. [Unary prefix operators](#Opers)
- Binary infix operators. See 
- Bracketing tokens.

#### Unary prefix

| Token | Name             |
| ----- | ---------------- |
| `!`   | logical negation |

The remaining prefix operators are keyword tokens: `let`, `public`, `private`,
`protected`, `internal`, `record`, `records`, `tuple`, `tuples`, `fn`, `class`,
`primary`, `candidate` and `throw`.

#### Binary infix

Listed in order of increasing binding power. Tokens on the same row bind equally tightly.

| Tokens                           | Role                                                 |
| -------------------------------- | ---------------------------------------------------- |
| `=` `?=`                         | identity, unify                                      |
| `->`                             | mapping                                              |
| `>>` `<<` `\|>` `<\|`            | composition and application                          |
| `=>`                             | lambda                                               |
| `./` `../`                       | expression scope, expression-member scope            |
| `\`                              | such-that / restriction                              |
| `\|` `\|\|` `\|\|\|`             | disjunction, guarded disjunction, mutually guarded disjunction |
| `&` `&&` `&&&`                   | conjunction, guarded conjunction, mutually guarded conjunction |
| `?` `??`                         | restriction, alternative-on-undefined                |
| `:` `::` `:::`                   | member-of, collection-of, sequence-of                |
| `<` `<=` `>=` `>` `==` `!=` `!:` | comparison, attribute-blind equality, non-membership |
| `...` `...<` `<...` `<...<`      | ranges                                               |
| `;` `;;`                         | choice, alternative-on-undefined                     |
| `^^`                             | set of functions                                     |
| `**`                             | non-fusing product                                   |
| `+` `-`                          | addition, subtraction                                |
| `*` `/` `%` `/%`                 | multiplication, division, modulo, divmod             |
| `^`                              | exponentiation                                       |
| `.` `..`                         | property, member property                            |

#### Unary suffix

There are none.

#### Bracketing

Bracketing tokens occur in pairs.

| Tokens  | Encloses                                                                                          |
| ------- | ------------------------------------------------------------------------------------------------- |
| `(` `)` | tuple, grouping                                                                                   |
| `[` `]` | list                                                                                              |
| `{` `}` | set; also a replacement block inside an interpolated string or identifier                         |
| `` ` `` | quoted identifier (a run of one or more backticks, see [Quoted identifiers](#quoted-identifiers)) |
| `"`     | string literal                                                                                    |

#### Punctuation

| Token | Role                                                                                                                            |
| ----- | ------------------------------------------------------------------------------------------------------------------------------- |
| `,`   | separates elements of a bracketed list                                                                                          |
| `@`   | separates the positional section of a product literal from its named section, fusing the positional value one level (see [Products](#products)) |
| `@@`  | as `@`, but non-fusing: the positional value stays a single component (see [Products](#products))                               |
| `$`   | prefixes an interpolated string or identifier (a run of one or more, see [Interpolated identifiers](#interpolated-identifiers)) |

### Comments

A comment is whitespace. It may appear anywhere whitespace may appear, and nowhere else —
in particular, not inside a string literal or a quoted identifier, where `//` and `/*` are
ordinary characters.

There are two forms:

- A **line comment** begins with `//` and extends to the end of the line.
- A **block comment** begins with `/*` and extends to the matching `*/`. Block comments
  **nest**: a `/*` inside a block comment opens an inner comment that must be closed before
  the outer one. This is what makes commenting out a region that already contains a comment
  work. An unterminated block comment is a compile-time error.

Because a comment is whitespace, `a/**/b` is two tokens, not one.

### String literals

A string literal produces a value of type `string`, which is an immutable sequence of Unicode characters. Several forms exist, distinguished by their opening delimiter and optional prefix characters.

#### Regular strings

A regular string is opened by `"` and terminated by the next unescaped `"` on the same line.

- The content may not contain an unescaped `"` or a newline character.
- [Escape sequences](#escape-sequences) are processed.
- The string value is the sequence of characters produced after escape processing.

Example: `"Hello, world!"`

#### Escape sequences

The following escape sequences are recognised inside regular strings and the expression blocks of interpolated strings. Each sequence begins with `\`.

| Sequence       | Unicode    | Meaning                               |
| -------------- | ---------- | ------------------------------------- |
| `\\`           | U+005C     | Backslash                             |
| `\"`           | U+0022     | Quotation mark                        |
| `\n`           | U+000A     | Line feed                             |
| `\r`           | U+000D     | Carriage return                       |
| `\t`           | U+0009     | Horizontal tab                        |
| `\a`           | U+0007     | Alert (bell)                          |
| `\b`           | U+0008     | Backspace                             |
| `\f`           | U+000C     | Form feed                             |
| `\v`           | U+000B     | Vertical tab                          |
| `\0`           | U+0000     | Null                                  |
| `\uHHHH`       | U+HHHH     | Unicode scalar (exactly 4 hex digits) |
| `\UHHHHHHHH`   | U+HHHHHHHH | Unicode scalar (exactly 8 hex digits) |
| `\xH[H[H[H]]]` | U+H…       | Unicode scalar (1–4 hex digits)       |

Any `\` not forming a recognised escape sequence is a compile-time error.

#### Verbatim strings

A verbatim string is opened by `@"` and terminated by the next `"` that is not immediately followed by another `"`.

- Escape sequences are not processed; every character is taken literally.
- A literal `"` inside the content is written as `""`.
- Newline characters are permitted in the content; the string value includes them as-is.

Example:
```
@"C:\Users\alice\Documents"
@"line one
line two"
@"She said ""hello""."
```

#### Raw strings

A raw string is opened by a run of K quotation marks, where K ≥ 3, and terminated by the first occurrence of exactly K consecutive quotation marks that is not part of a longer run.

Let K be the number of quotation marks in the opening run.

- The content of the opener and terminator lines (after the delimiter run) must be empty or consist only of whitespace; newlines follow immediately after each delimiter.
- Quotation-mark runs inside the string with length less than K are literal.
- A quotation-mark run of exactly K that is not part of a longer run terminates the string.
- The indentation of the closing delimiter line establishes the base indentation level. That exact sequence of whitespace characters is stripped from the beginning of every content line. A content line with less indentation than the base is a compile-time error.
- Escape sequences are not processed.

Example (K = 3):
```
"""
    line one
    line two
    """
```
Value: `line one\nline two\n` (assuming 4-space base indentation stripped).

#### Interpolated strings

An interpolated string is a regular string, verbatim string, or raw string immediately preceded by N dollar-sign characters (N ≥ 1), with no whitespace between the last `$` and the opening delimiter.

Let N be the number of leading `$` characters.

- Replacement blocks are permitted in the string content.
- A replacement block consists of exactly N left braces, an expression, then exactly N right braces.
- For N = 1: `{` expression `}`
- For N = 2: `{{` expression `}}`
- In general, N dollar signs require N braces on both sides of each replacement block.
- A brace run with length less than N is always literal text.
- A brace run with length greater than N is parsed left-to-right: consume an N-brace group as a delimiter only where it forms a syntactically valid replacement block; remaining braces that do not form such a group are literal text.
- An unmatched opening delimiter group with no closing delimiter group is a compile-time error.
- Each replacement expression is evaluated; its value must be a `string` or be convertible to a `string`.
- The string value is the concatenation of literal segments and converted replacement values, in order.

Interpolated verbatim strings (`$@"..."` or `@$"..."`) combine verbatim literal rules with interpolation: escape sequences are not processed in literal segments, but replacement block expressions are evaluated normally.

Interpolated raw strings (`$"""..."""`) combine raw string literal rules with interpolation.

Examples:

| Form                  | Example                         |
| --------------------- | ------------------------------- |
| Interpolated regular  | `$"Hello, {name}!"`             |
| Multi-dollar regular  | `$$"Value: {{x + 1}}"`          |
| Interpolated verbatim | `$@"Path: {basePath}\file.txt"` |
| Interpolated raw      | `$"""{"nested"}"""`             |



### Identifiers

Regular identifiers start with a Unicode letter or underscore and are followed by zero or more Unicode letters, digits, or underscores.

#### Quoted identifiers

A quoted identifier is opened by a run of one or more backtick characters and terminated by a run of exactly the same number of backtick characters.

Let M be the number of backticks in the opening run.

- The terminator is the first occurrence of exactly M consecutive backticks that is not part of a longer run.
- Backtick runs inside the identifier with length unequal to M are treated as literal backtick characters.
- A backtick run of length M that is part of a run longer than M does not act as a terminator.
- If the opening backtick run is immediately followed by a single space, and the terminating backtick run is immediately preceded by a single space, both spaces are trimmed from the identifier name.
- Trimming is performed only when both conditions are satisfied simultaneously; a leading space is not trimmed without a trailing space, and vice versa.

Examples:

| Written form                | Effective identifier name                                          |
| --------------------------- | ------------------------------------------------------------------ |
| `` `foo` ``                 | `foo`                                                              |
| `` ` foo` ``                | ` foo` (space not trimmed)                                         |
| `` ` foo ` ``               | `foo` (spaces trimmed)                                             |
| ``` `` foo`bar `` ```       | ```foo`bar``` (inner single backtick is literal, spaces trimmed)   |
| ```` ```foo``bar``` ````    | ```foo``bar``` (inner double-backtick is literal)                  |
| ```` ``` foo ``bar ``` ```` | ```foo ``bar``` (inner double-backtick is literal, spaces trimmed) |

#### Interpolated identifiers

Interpolated identifiers are quoted identifiers which are immediatedly preceded by a number of \$ characters with no white space between the last \$ character and the first \` character.

Let N be the number of leading dollar characters in the opener.

- Replacement blocks are permitted inside interpolated identifiers.
- A replacement block has the form: N left braces, then an expression, then N right braces.
- For N = 1: { expression }
- For N = 2: {{ expression }}
- In general, N dollar signs require N braces on both sides of each replacement block.
- A brace run with length less than N is always literal text.
- A brace run with length greater than N is processed left-to-right as follows:
- Consume N-brace groups as delimiters only when they form a syntactically valid replacement block.
- Braces not consumed as delimiters are literal text.
- Unmatched delimiter groups are invalid.

Each replacement expression is evaluated.
Its value must be a string or be convertible to a string.
The interpolated identifier value is the concatenation of literal segments and converted replacement values.

Examples:
- ``` $`Name{n}` ```
- ``` $$`Name{{n}}` ```
- ``` $$$`Field{{{index}}}` ```

Format specifiers




### Numeric literals
- Numeric literals include integer and floating forms.
- Numeric separators are supported (example in syntax docs).

### Whitespace and indentation
- Grammar-shift operators can use indentation-sensitive forms.
- If shift operators end a line, subsequent indented lines are folded.

## Declarations and Binding

### Scope-introducing operators
The reference documents identify these as declaration-forming contexts:
- Visibility and module scope operators: `public`, `private`, `protected`, `local`
- Aggregate scope operators: `record`, `records`, `tuple`, `tuples`
- Function scope operators: `fn`, lambda `=>`
- Local binder: `let`

### Local binding
- Canonical form: `let pattern = expr in body`
- Recursive and unification-style examples use `?=`.

### Lambda
- Form: `pattern => expression`
- Left side is declarative pattern context.
- Right side is referential expression context under the bound names.

### Function cases via fn
- Case function form: `fn{ p1 -> r1, p2 -> r2, ... }`
- Supports typed patterns and guarded patterns shown in examples.
- The case form lowers to an ordered choice of lambdas, `(p1 => r1) || (p2 => r2) || …`, so cases are tried in order and earlier cases take priority (see [`fn`](#fn)).

## Sets

Ting models types as sets.

### Built-in scalar sets
- `byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`
- `float`, `double`, `decimal`
- `char`, `string`, `bool`

### Built-in meta sets
- `Sets`: set of all sets
- `Funcs`: set of all functions

### Set definitions

A **set definition** is a comma-separated list of expressions enclosed in `{` and `}`.
The expressions in the list are the definition's **element expressions**; the list may
be empty. 

Each element expression contributes every value it can assume, not the expression itself: 
- one value if it is deterministic
- no value at all if it is undefined
- several values if it is nondeterministic.

Members values in a set definition are ordered (see [Sets and multisets](#sets-and-multisets)): the element expressions in the order they are written, and within an element expression the values in the order it produces them. A value
contributed more than once is a member once, at the position of its first contribution.

If an element expression is *countably nondeterministic*, the values it contributes are *ordered*. If an element expression is *uncountably nondeterministic*, the values it contributes are *unordered*. The members of the set as a whole may therefore be only *partially ordered*: every value contributed by an earlier element expression still precedes every value contributed by a later one.

A definition with no element expressions denotes the empty set.



### Multiset definition 

A *multiset definition*, written with `{*` and `*}`, has the same meaning except that
every contribution is retained rather than only the first.


### Set countability



### Product sets

`A * B`, where `A` and `B` are sets, denotes the product set. `*` on sets is *fusing*,
one level deep: the members of `A * B` are the products obtained by fusing a member of
`A` with a member of `B`, so the top-level fields of both become top-level fields of the
member. For scalar operand sets this is the familiar Cartesian product — `int * float`
is the set of two-tuples `(i, f)` for every `i : int` and `f : float`. Because fusion is
one level deep, the product is independent of grouping: `int * int * string` denotes the
set of three-tuples however it associates. On sets, `/` is the inverse of `*`.

`A ** B` denotes the *non-fusing* product: the set of genuine two-tuples whose first
component is a member of `A`, nested whole, and whose second component is a member of
`B`, nested whole. No fusion takes place: members of `(int*int) ** string` are two-tuples
whose first component is itself a two-tuple.

## Functions

### Functions as relations
Functions map argument values to result values. A function permits, at each argument
value, a set of result values: applied to an argument it may be deterministic (one
result), nondeterministic (several results), or undefined (no result). Function values
can be defined by lambdas or case expressions (`fn`).

### Mappings and the underlying collection

A function is backed by a collection of *mappings*. A mapping is an ordinary product:
`x -> y` denotes the product obtained by fusing the value of `x`, one level deep, with a
named field `Result` bound to `y`:

    x -> y   =   (x @ Result = y)

Every field of a mapping — the argument fields and `Result` — participates in its
identity; there is no special pair structure.

Every function `F` has an `AsSet` property that denotes its underlying collection of
mappings. The collection is in general a *multiset*: equal mappings may be present more
than once (see `+` on functions), in which case the function has correspondingly
many solutions at that argument. The `fn` operator is the inverse of `AsSet`: it converts
a collection of mappings into a function.

For a function `F : A ^^ B`, the underlying collection satisfies
`F.AsSet <= A * records(B Result)`; the domain and range are recovered as
`dom F = F.AsSet / records(B Result)` and `range F = F.AsSet..Result`. Functions whose
arguments are themselves nested tuples use `**` domains; the nesting survives because
fusion is one level deep.

Where several mappings of `F` share the same argument part, `F` is nondeterministic at
that argument, yielding the `Result` of each such mapping; multiplicities are observable
as solution multiplicity under backtracking.

Two functions are equal under `==` exactly where their underlying collections are equal:
functions compare extensionally.

> Note (draft): the mechanism that names the result field — a structural named field
> `Result` versus a name published on the last position of a plain tuple by the
> `->`/`fn`/`^^` machinery — is not yet final.

### Named arguments

> Note: this section is a draft. The design depends on the module system and is not yet
> final.

Parameter names may be *published* as part of a function's visible interface by applying
a publishing operator to the parameter pattern:

    public Sum = public(int a, int b) => a + b

A call site to which the names are published may bind arguments by name in the named
section of the argument product:

    Sum(1, 2)
    Sum(1 @ b = 2)
    Sum(@ a = 1, b = 2)

Names published this way are publication metadata attached to the declaration; tuple
values remain purely positional, and application remains unification of the parameter
pattern with the argument. Where no names are published, only positional application is
available. Whether published names attach to the function value itself or to the binding
through which the function is reached is not yet specified.

### Sets as identity functions
TODO: describe an implicit promotion from set values to identity functions when a function is expected.

### Function composition and application
- Composition operators include `>>` and `<<`.
- Application operators include `|>` and `<|`.
- Juxtaposition application (`f x`) is supported.


## Collections

### Collection literals
- List literal: `[e1, e2, ...]`
- Tuple literal: `(e1, e2, ...)`
- Product literal with a named section: `(e1, e2 @ n1 = v1, n2 = v2)` (see [Products](#products))
- Set literal: `{e1, e2, ...}`
- Multiset literal: `{* e1, e2, ... *}`

A collection literal is not a token: its elements are arbitrary expressions, so it is a syntactic form rather than a lexical one.

### Lists
- Ordered sequence values.
- List definitions can expand nondeterministic elements by backtracking.

### Sets and multisets
- Sets are unique-element collections with definition-order semantics in docs.
- Multisets permit duplicates and can preserve generation order.

### Products

Tuples and records are the two extremes of a single product structure. A product value
has zero or more *positional* fields, optionally followed by zero or more *named*
fields. All fields — positional and named — participate in the identity of the product:
two products are equal exactly where all of their fields agree.

- A **tuple** is a product with only positional fields.
- A **record** is a product with only named fields.
- A scalar behaves as a one-field product.

A product literal is written in parentheses. The `@` punctuation separates the
positional section from the named section:

    (p1, p2 @ n1 = v1, n2 = v2)

Before `@` the elements are ambient expressions contributing positional fields, exactly
as in a tuple literal. After `@` the section is declarative and is harvested like the
operand of `record`: every name it declares becomes a named field of the product, and
the truth values of the declaring propositions are discarded.

`(@ Name = "Bob")` is therefore a record literal. A bare `(Name = "Bob")` is unchanged:
it denotes the `bool` value of the proposition `Name = "Bob"`.

The value of the positional section is *fused* into the product one level deep: its
top-level fields become the positional fields of the result. Where `x` is the two-tuple
`(a1, a2)`, the literal `(x @ Result = y)` denotes the product `(a1, a2 @ Result = y)`.
Writing `@@` in place of `@` suppresses the fusion: the value of the positional section
becomes a single field, nested whole. This mirrors the product set operators — `*` fuses
one level deep, `**` does not (see [Product sets](#product-sets)).

> Note (draft): whether the named section of a product literal is purely structural
> construction, or is additionally a binding form resolved against *published* names at
> application sites (see [Named arguments](#named-arguments)), is not yet final. The
> same question applies to record field names: structural and identity-bearing, or
> publication metadata over positional tuples.

### Tuples
- Positional product values: products with only positional fields (see [Products](#products)).
- Optional components are documented by example.
- `tuple` and `tuples` operators provide explicit construction/type-set forms.

### Records
- `record` defines a product with only named fields (see [Products](#products)).
- `records` denotes sets of records.
- Field access uses property selection (`.`).

## Classes, Entities, Enums

### Classes
- Classes are inclusive sets over candidate sets.
- Equal payloads in same class are value-equal.
- Created with `class`, instantiated with `new`.
- Extended with `extend`.

### Entities
- Entities are identity-based and exclusive (only constructed instances).
- Equal payloads do not imply equal entities.
- Escaping entities may require explicit stable identity fields (per docs).

### Enums
- Enumerations are declared with `enums` syntax.
- Tagged variants with payloads are documented by examples.

## This Binding

`this` can be explicitly bound and can also be introduced by context operators.

Documented contexts that bind `this` include:
- `record`, `records`
- `tuple`, `tuples`
- explicit lambda parameters named `this`
- local binding such as `let this = ... in ...`

When `this` is in declarative position, members of its value/type are available in scope by unqualified names.

Name resolution rule for `this`:
- `this` follows normal lexical scoping and shadowing.
- The closest binding always wins.
- A nested `let this = ... in ...` introduces a new `this` that shadows outer or implicit `this` bindings for that nested scope.
- A function/lambda parameter named `this` shadows outer or implicit `this` bindings inside that function body.

## Nondeterminism

TODO: Choice, ranges, and set/list expansion interact via backtracking semantics.
TODO: Undefined values participate in operator-specific ways (notably choice and guarded logical operators).

### Unenumerable nondeterminism

TODO

### Enumerable bounded nondeterminism

TODO

### Enumerable unbounded nondeterminism

TODO

## Exceptions

Exception behavior follows the .NET exception model. No explicit `try` block syntax exists; instead, exception handling is integrated via the binary `catch` operator and the unary prefix `throw` operator.

### Throw operator

The `throw` operator is a unary prefix operator that raises (throws) an exception.

- Syntax: `throw expr`
- The operand expression must evaluate to an exception object.
- Execution of the current expression terminates, and control transfers to the nearest enclosing `catch` operator that can handle the exception.
- If no enclosing `catch` handles the exception, it propagates to the caller.

### Catch operator

The `catch` operator is a binary operator that handles exceptions thrown by its left operand.

The left operand is an expression that is evaluated under an implicit try.

The right operand is a function that handles exceptions.

- Evaluation proceeds as follows:
  - The left operand is evaluated.
  - If no exception is thrown, the result of the left operand is the result of the entire `catch` expression.
  - If an exception `E` is thrown during evaluation of the left operand, **and** the catch function is defined for `E` then the catch function is invoked with `E` as its argument. The result of the `catch` expression is the result of the catch function applied to the exception `E`
  - If the catch function is not defined for the exception type of `E`, the exception `E` continues to propagate.

### Catch function matching

The catch function determines whether it can handle a given exception.

- The catch function must be a function value (a lambda, `fn` expression, or similar).
- Matching is performed using the function's parameter type constraints and patterns.
- If the catch function has a parameter of the exact type `E` or a supertype of `E`, it is considered defined for `E`.

Example:

```ting
expr catch Exception ex => "Error: " + ex.Message   // Catches any exception, binds to ex
expr catch (ArgumentException e) => "Invalid argument: " + e.ParamName   // Catches ArgumentException only
expr catch fn{
  (ArgumentException _) -> "Bad arg",
  (NullReferenceException _) -> "Null ref",
  _ -> "Other error"   // Fallback for any exception type
}
```

### Exception propagation and nesting

Multiple `catch` operators may be nested.

- An exception caught by an inner `catch` does not propagate to an outer `catch` unless the inner catch function re-throws (via `throw`).
- An exception not caught by a `catch` continues propagating outward.
- If no `catch` in the entire expression handles an exception, it propagates to the caller.



## Operators

Ting has no operators of its own. Every prefix and infix operator is surface syntax for
the application of an ordinary function: `a + b` applies `_+_` to the tuple `(a, b)`, and
`!p` applies `!_` to `p`. 

The functions an operator can name are taken from a scope available to the parser, so they are never looked up lexically and cannot be shadowed by a declaration.

### Declarative and referential operators

A **declarative operator** is an operator which introduces a new declarative scope over one of its operand(s).

A **right-referential** operator is a binary infix operator which converts any declarative scope to referential scopes over its right operand.

A **left-referential** operator is a binary infix operator which converts any declarative scope to referential scopes over its left operand.



#### Neutral

A neutral operator forces no role on either operand and introduces no scope. Both operands
are ambient, so a neutral operator expression declares exactly where the surrounding
context declares and refers everywhere else. Neutrality is what allows an operator to be
used unchanged on both sides of a declaration.

Prefix operators are neutral in the same sense: their single operand is ambient.

#### Right-referential

The right operand is referential; the left operand is ambient. No scope is introduced.

This asymmetry is what gives `x = 5` its meaning. The right operand is a value to be read,
while the left operand inherits the surrounding context and therefore introduces `x` when a
declarative scope covers the expression. Nothing about the operator itself declares
anything; the operator only decides how its operands are interpreted against the scopes
already in force.

#### Left-declarative

A left-declarative operator introduces a local scope spanning the whole operator
expression. The left operand is declarative in that scope irrespective of the surrounding
context, and the names it introduces are in scope in the right operand. The right operand
remains ambient, so under a surrounding declarative context it may itself declare.

#### Right-declarative

The mirror image of left-declarative. A local scope spanning the whole operator expression
is introduced, the right operand is declarative in that scope irrespective of the
surrounding context, and the names it introduces are in scope in the left operand.

### Operator Precedence and Associativity

TODO: precedence and associativity are intentionally left unspecified in this draft and will be defined later.

### Binary operators used as unary prefix operators

Any binary operator may be written in prefix position with a single operand. The result is
the function obtained by partial application: `( : rhs)` is a function that accepts `lhs`
and evaluates `lhs : rhs`.



### Unary prefix operators

A unary prefix operator is written before its single operand. The operators listed here
are prefix-only: the grammar accepts no infix form for them. Any binary infix operator may
also be written in prefix position, but that denotes partial application rather than a
distinct operator; see [Binary operators used as unary prefix
operators](#binary-operators-used-as-unary-prefix-operators).

#### `let`

Introduces a local scope spanning its operand. Names declared by the operand are local to
that scope. The canonical form is `let pattern = expr in body`.

#### `public`

Introduces a scope in which the operand is declarative and publishes every name the
operand declares with public access.

#### `private`

As `public`, with private access.

#### `protected`

As `public`, with protected access.

#### `internal`

As `public`, with internal access.

Publishing operators nest. A name declared under two overlapping publishing contexts is
unified, so `protected internal int Answer` declares one `Answer` that is both protected
and internal.

#### `record`

Evaluates its operand as a record scope: every name the operand declares becomes a field
of the resulting record.

#### `records`

Denotes the set of records whose fields are those declared by the operand.

#### `tuple`

Marks its operand as an explicit tuple construction. It contributes no meaning of its own:
`tuple E` denotes `E`.

#### `tuples`

Denotes the set of tuples whose components are those of the operand.

#### `fn`

Neutral. Converts a collection of mappings into a function: `fn S`, where the members of
`S` are mappings (see [Mappings and the underlying
collection](#mappings-and-the-underlying-collection)), denotes the function whose
underlying collection is `S`. Where `S` contains several mappings with the same argument
part the function is nondeterministic at that argument; where `S` is a multiset,
multiplicities are retained as solution multiplicities.

The *case form*, `fn { p1 -> r1, p2 -> r2, … }`, where the cases are declarative
patterns, is a distinct syntactic form: it lowers to the ordered choice of lambdas

    (p1 => r1) || (p2 => r2) || …

so cases are tried in order and the first case defined for the argument decides the
result. The two readings coincide where the cases are ground and disjoint; they differ
where cases overlap — the case form gives the earlier case priority, while `fn` applied
to a set value containing overlapping mappings is nondeterministic there.

#### `!`

Neutral. Logical negation.

For the signature `bool^^bool`:

| operand     | result      |
| ----------- | ----------- |
| `true`      | `false`     |
| `false`     | `true`      |
| *undefined* | *undefined* |

For the signature `set^^set`:

    !A = { x \ !(x : A) }

Denotes the complement of `A`: the set whose members are exactly the values that are not
members of `A`. Membership in the result is undefined for a value whose membership in `A`
is undefined.

For the signature `Funcs^^Funcs`:

    !F = fn { x -> y \ (x -> y) !: F.AsSet }

where `AsSet` is the property of every function that returns its underlying collection
of mappings. Denotes the relational complement of `F`: the function defined by every
mapping that is not among `F`'s mappings. The set builder restricts the
complement to mappings — `fn !(F.AsSet)` would be wrong, since the complement of a
set also contains every value that is not a mapping at all. A function permits, at
each argument value, a set of result values; `!F` permits exactly the values `F` does not
permit. Applied to a member `x` of `dom F`, `!F x` is nondeterministically any value
other than `F x`; applied to a value outside `dom F` it is nondeterministically any value
at all. Negation on functions is an involution — `!!F` denotes `F` — and `F & !F` is
undefined for every argument.

#### `class`

Neutral. Constructs a class over the candidate set denoted by its operand. A class is an
inclusive set; values of different classes are never equal even where their candidates
are.

#### `primary`

Erases attributes. It is total, deterministic, idempotent and deep, and it preserves class
membership.

#### `candidate`

Erases class membership, yielding the candidate value the class was constructed over. It
preserves attributes.

`primary` and `candidate` are keyword intrinsics rather than properties, so that they
cannot be shadowed by a field of the same name.

#### `throw`

Raises the exception denoted by its operand. The operand is ambient, as every prefix
operator's is. `throw` is a semantic form rather than a function application: it produces
no value, so there is nothing for an application to yield. See [Exceptions](#exceptions).

### Binary infix operators

A binary infix operator is written between its two operands.

#### `where`

Right-declarative. Introduces a local scope spanning the expression; the right operand
declares into that scope and the left operand refers to it. The value of the expression is
the value of the left operand on the condition that the right operand evaluates to true.

#### `./`

Expression scope. The declarations of the left operand are made available as a scope for
the right operand, and `this` is bound to the left operand within the right operand.

#### `../`

Expression member scope. The declarations of the *members* of the left operand are made
available as a scope for the right operand, and `this` is bound to a member of the left
operand within the right operand.

#### `=>`

Left-declarative. `pattern => expression` denotes a function. The left operand is a
declarative pattern; the right operand is evaluated with the names the pattern declares in
scope.

#### `->`

Right-referential. Constructs a mapping: the product obtained by fusing the value of the
left operand, one level deep, with a named field `Result` bound to the value of the
right operand:

    x -> y   =   (x @ Result = y)

A mapping is an ordinary product; all of its fields participate in its identity. For a
tuple-valued left operand the fusion splices its top-level fields, so `(a, b) -> y`
denotes `(a, b @ Result = y)`; a left operand built with `**` keeps its nesting, since
fusion is one level deep.

#### `>>`

Neutral. Forward composition. `f >> g` denotes the function that applies `f` to its
argument and then applies `g` to the result.

#### `<<`

Neutral. Reverse composition. `g << f` denotes the same function as `f >> g`.

#### `|>`

Neutral. Forward application. `x |> f` denotes `f x`. Unlike juxtaposition, both
operands are ambient, so neither is forced to be referential.

#### `<|`

Neutral. Backward application. `f <| x` denotes `f x`, with the same binding behaviour as
`|>`.

#### `^^`

Neutral. Denotes the set of functions from the left operand set to the right operand set. Both operands must be sets. `int ^^ string` is the set of functions from
`int` to `string`. The underlying collection of a member `F : A ^^ B` consists of
mappings that fuse a member of `A` with a `Result` that is a member of `B`:
`F.AsSet <= A * records(B Result)`. TODO: whether membership additionally requires
totality on `A` is not yet specified.

#### `**`

Neutral. The non-fusing product. For set operands, `A ** B` denotes the set of two-tuples
whose first component is a member of `A` and whose second component is a member of `B`,
each nested whole: no fusion takes place. Where `*` fuses one level deep —
`int*int*string` is a set of three-tuples — `**` makes nesting expressible: members of
`(int*int) ** string` are two-tuples whose first component is itself a two-tuple.

#### `=`

Right-referential. Relates its two operands and yields `true` or `false`. It
compares values structurally, observing all three value layers — attributes, class
membership and structural core — and is the only attribute-sensitive relation in the
language. In a position that requires truth, such as a guard or a restriction, `a = b`
must evaluate to `true`. Because the operator is right-referential, the left operand
inherits the surrounding context, which is why `x = 5` declares `x` under a declarative
scope; nothing about `=` itself declares anything.

#### `?=`

Right-referential, and binds as tightly as `=`. Declaration by unification: the left
operand is unified with the right rather than asserted equal to it, and where the two
cannot be unified the expression is *undefined* rather than `false`.

The name the left operand declares is in scope on the right, which is what distinguishes
`?=` from `=` in practice: it is how a recursive binding is written.

    let f ?= fn{ 0 -> 1, int n -> n * f (n - 1) } in f 3

#### `in`

Right-referential. The left operand is a proposition and the right operand is any
expression. The value of the expression is the value of the right operand, provided the
left operand evaluates to `true`. If the left operand evaluates to `false` or is
undefined, the expression is undefined.

#### `then`

Right-referential. Expresses implication. If the left operand is `true`, the value
of the expression is the value of the right operand. If the left operand is `false`, the
expression makes no statement about the right operand and is undefined.

#### `else`

Neutral. Constructs an alternative from its two operands, for use as the right operand of
`then`. `a then b else c` groups as `a then (b else c)`.

#### `\`

Left-declarative. Such-that. Introduces a local scope spanning the expression; the
left operand declares into it and the right operand is a proposition over the names
declared. The value is the value of the left operand for those values that satisfy the
right operand. `int x \ x != 0` denotes an `int` that is not zero.

#### `?`

Right-referential. Restriction. `left ? predicate` restricts `left` to the values for
which `predicate` is satisfied.

#### `??`

Right-referential. TODO: its semantics are not yet specified.

#### `&`

Neutral. Strict conjunction.

A non-guarded operator always evaluates both of its operands, so its table lists every
combination of `true`, `false` and *undefined*. The table of a guarded operator uses one
further operand value:

- *ignored* — the operand is either not evaluated, or its result is disregarded even where
  it is undefined.

Where two rows match the same pair of operands they give the same result.

For the signature `bool*bool^^bool`:

| left        | right       | result      |
| ----------- | ----------- | ----------- |
| `true`      | `true`      | `true`      |
| `true`      | `false`     | `false`     |
| `true`      | *undefined* | *undefined* |
| `false`     | `true`      | `false`     |
| `false`     | `false`     | `false`     |
| `false`     | *undefined* | *undefined* |
| *undefined* | `true`      | *undefined* |
| *undefined* | `false`     | *undefined* |
| *undefined* | *undefined* | *undefined* |

Both operands are evaluated: the result is `true` where both operands are `true`, `false`
where either operand is `false`, and undefined where either operand is undefined.

For the signature `set*set^^set`:

    A & B = { x \ x : A & x : B }

Denotes the intersection of `A` and `B`: the set whose members are exactly the values
that are members of both `A` and `B`. Both membership tests apply, so membership in the
result is undefined for a value whose membership in either operand is undefined.

For the signature `Funcs*Funcs^^Funcs`:

    F & G = fn (F.AsSet & G.AsSet)

The set overload applied to the operands' underlying collections of mappings. Denotes the
function that permits, at each argument value, exactly the result values that both
operands permit. Applied to `x` it yields `F x ?= G x`, the unification of the two
results: it is defined at `x` where both operands are defined at `x` and their results
unify, which for deterministic operands means where `F x = G x`.

#### `&&`

Neutral. Guarded conjunction.

For the signature `bool*bool^^bool`:

| left        | right       | result      |
| ----------- | ----------- | ----------- |
| `false`     | *ignored*   | `false`     |
| `true`      | `true`      | `true`      |
| `true`      | `false`     | `false`     |
| `true`      | *undefined* | *undefined* |
| *undefined* | *ignored*   | *undefined* |

If the left operand evaluates to `false` the result is `false`, and the right operand may
be undefined and may not be evaluated. If the left operand evaluates to `true` the result
is that of the right operand, even if the right operand is undefined. If the left operand
is undefined the result is undefined.

For the signature `set*set^^set`:

    A && B = { x \ x : A && x : B }

Denotes the guarded intersection of `A` and `B`. A value that is not a member of `A` is
not a member of the result, even where its membership in `B` is undefined. For a member
of `A`, membership in the result is its membership in `B`. This is the intersection to
use where the right operand's membership is only defined for members of the left operand.

For the signature `Funcs*Funcs^^Funcs`:

Denotes the guarded counterpart of `&` on functions. The guard applies to the domain
test: where `x` is not a member of `dom F` the result is undefined at `x` without
consulting `G`, whose domain membership may itself be undefined there. Where `F` is
defined at `x` the result at `x` is `F x ?= G x`, as for `&`.

#### `&&&`

Neutral. Mutually guarded conjunction. 

For the signature `bool*bool^^bool`:

| left        | right       | result      |
| ----------- | ----------- | ----------- |
| `false`     | *ignored*   | `false`     |
| *ignored*   | `false`     | `false`     |
| `true`      | `true`      | `true`      |
| `true`      | *undefined* | *undefined* |
| *undefined* | `true`      | *undefined* |
| *undefined* | *undefined* | *undefined* |

The guard applies on both sides: if either operand evaluates to `false` the result is
`false`, and undefinedness of the other operand is caught. If both operands evaluate to
`true` the result is `true`. Otherwise the result is undefined.

For the signature `set*set^^set`:

    A &&& B = { x \ x : A &&& x : B }

Denotes the mutually guarded intersection of `A` and `B`. A value that is not a member of
either operand is not a member of the result, even where its membership in the other
operand is undefined. A value that is a member of both operands is a member of the
result. Otherwise membership in the result is undefined.

For the signature `Funcs*Funcs^^Funcs`:

Denotes the mutually guarded counterpart of `&` on functions. The guard applies to both
domain tests: a defined non-membership of `x` in either operand's domain makes the result
undefined at `x` even where the other operand's domain membership there is undefined.
Where both operands are defined at `x` the result is `F x ?= G x`, as for `&`.

#### `|`

Neutral. Strict disjunction.

For the signature `bool*bool^^bool`:

| left        | right       | result      |
| ----------- | ----------- | ----------- |
| `true`      | `true`      | `true`      |
| `true`      | `false`     | `true`      |
| `true`      | *undefined* | *undefined* |
| `false`     | `true`      | `true`      |
| `false`     | `false`     | `false`     |
| `false`     | *undefined* | *undefined* |
| *undefined* | `true`      | *undefined* |
| *undefined* | `false`     | *undefined* |
| *undefined* | *undefined* | *undefined* |

Both operands are evaluated: the result is `true` where either operand is `true`, `false`
where both are `false`, and undefined where either operand is undefined.

For the signature `set*set^^set`:

    A | B = { x \ x : A | x : B }

Denotes the union of `A` and `B`: the set whose members are exactly the values that are
members of either operand. Both membership tests apply, so membership in the result is
undefined for a value whose membership in either operand is undefined.

For the signature `Funcs*Funcs^^Funcs`:

    F | G = fn (F.AsSet | G.AsSet)

The set overload applied to the operands' underlying collections of mappings. Denotes the
function that permits, at each argument value, every result value that either operand
permits. It is defined at `x` where either operand is defined at `x`; both domain tests
apply. Where only one operand is defined at `x` the result is that operand's result.
Where both are defined at `x` with the same result, their mappings are identical and
unify to a single member of the union, so the result there is deterministic; where their
results differ the result is nondeterministically either. For
`F = fn { 1 -> 10, 2 -> 20, 4 -> 40 }` and `G = fn { 2 -> 21, 3 -> 30, 4 -> 40 }`,
`F | G` applied to `1` yields `10`, applied to `3` yields `30`, applied to `4` yields
`40` deterministically, and applied to `2` yields nondeterministically `20` or `21`.

#### `||`

Neutral. Guarded disjunction.

For the signature `bool*bool^^bool`:

| left        | right       | result      |
| ----------- | ----------- | ----------- |
| `true`      | *ignored*   | `true`      |
| `false`     | `true`      | `true`      |
| `false`     | `false`     | `false`     |
| `false`     | *undefined* | *undefined* |
| *undefined* | *ignored*   | *undefined* |

If the left operand evaluates to `true` the result is `true`, and the right operand may be
undefined and may not be evaluated. If the left operand evaluates to `false` the result is
that of the right operand, even if the right operand is undefined. If the left operand is
undefined the result is undefined.

For the signature `set*set^^set`:

    A || B = { x \ x : A || x : B }

Denotes the guarded union of `A` and `B`. A member of `A` is a member of the result, even
where its membership in `B` is undefined. For a value that is not a member of `A`,
membership in the result is its membership in `B`. This is the union to use where the
right operand's membership is only defined for non-members of the left operand.

For the signature `Funcs*Funcs^^Funcs`:

Denotes ordered choice. Applied to `x`, the result is `F x` if and only if `F` is defined
at `x`; `G` is ignored there — it does not narrow the result, and its domain membership
may be undefined. Where `F` is not defined at `x` the result is `G x`. With `F` and `G`
from the example under `|`, `F || G` applied to `2` yields only `20`. Unlike the strict
operators, this is not the set overload on the underlying mappings — `F || G` is not
`fn (F.AsSet || G.AsSet)` — because the guard applies to the domain test, not to mapping
membership.

#### `|||`

Neutral. Mutually guarded disjunction. 

For the signature `bool*bool^^bool`:

| left        | right       | result      |
| ----------- | ----------- | ----------- |
| `true`      | *ignored*   | `true`      |
| *ignored*   | `true`      | `true`      |
| `false`     | `false`     | `false`     |
| `false`     | *undefined* | *undefined* |
| *undefined* | `false`     | *undefined* |
| *undefined* | *undefined* | *undefined* |

The guard applies on both sides: if either operand evaluates to `true` the result is
`true`, and undefinedness of the other operand is caught. If both operands evaluate to
`false` the result is `false`. Otherwise the result is undefined.

For the signature `set*set^^set`:

    A ||| B = { x \ x : A ||| x : B }

Denotes the mutually guarded union of `A` and `B`. A value that is a member of either
operand is a member of the result, even where its membership in the other operand is
undefined. A value that is a non-member of both operands is not a member of the result.
Otherwise membership in the result is undefined.

For the signature `Funcs*Funcs^^Funcs`:

Denotes the mutually guarded counterpart of `|` on functions. As for `|`, the result at
`x` permits every value permitted by either operand. The guard applies to both domain
tests: where one operand's domain membership at `x` is undefined but the other operand is
defined at `x`, the result is the defined operand's result.


#### `:`

Right-referential. Yields `true` when the left operand is a member of the right operand
set.

#### `!:`

Right-referential. Yields `true` when the left operand is not a member of the right
operand set.

#### `::`

Right-referential. Yields `true` when the left operand is a collection all of whose
elements are members of the right operand set.

#### `:::`

Right-referential. Yields `true` when the left operand is an ordered sequence of members
of the right operand set.

#### `<`

Right-referential. For numeric operands, less than. For set operands, proper subset.

#### `<=`

Right-referential. For numeric operands, less than or equal to. For set operands, subset.

#### `>=`

Right-referential. For numeric operands, greater than or equal to. For set operands,
superset.

#### `>`

Right-referential. For numeric operands, greater than. For set operands, proper superset.

#### `==`

Right-referential. Equality that ignores attributes but observes class membership
and structural core. It is not primitive: it denotes `=` applied to the attribute-erased
operands. For set operands it yields `true` when both sets have the same members. The
layers each relation observes, the `primary` and `candidate` intrinsics, and the lowering
that relates `=` and `==` are specified normatively in
[The Core Expression](core-expression.md).

#### `!=`

Right-referential. The negation of `==`.

#### `...`

Neutral. Denotes a range: an expression that can assume any value between the left operand
and the right operand. A range is ordered and nondeterministic; where a range provably
contains a single value it is deterministic. Both bounds are included, so `a ... b` is
equivalent to `x \ x >= a & x <= b`.

#### `<...`

Neutral. As `...`, except that the lower bound is excluded: `a <... b` is equivalent to
`x \ x > a & x <= b`.

#### `...<`

Neutral. As `...`, except that the upper bound is excluded: `a ...< b` is equivalent to
`x \ x >= a & x < b`.

#### `<...<`

Neutral. As `...`, except that both bounds are excluded: `a <...< b` is equivalent to
`x \ x > a & x < b`.

#### `;`

Neutral. Introduces an ordered nondeterministic choice. `a ; b` assumes the value
of `a` until backtracking forces it to assume the value of `b`. An undefined operand is
disregarded; the expression is undefined only when both operands are undefined.

#### `;;`

Neutral. Alternative on undefined. `a ;; b` is `a` unless `a` is undefined, in
which case it is `b`. The operator introduces no nondeterminism of its own, although
either operand may itself be nondeterministic.

#### `+`

Neutral. Numeric addition. For list operands, including `string`, it concatenates.

For set and multiset operands, addition: the collection that retains every contribution
from both operands.

    {1, 2} + {2, 3}   =   {* 1, 2, 2, 3 *}

The result is a multiset wherever the operands overlap: `#(A + B) = #A + #B` always
holds. Unlike `|`, which unifies common members and is idempotent, `+` is not
idempotent. The relation between `|` and `+` on sets mirrors the relation between the
set definition `{ … }` and the multiset definition `{* … *}`: `|` keeps one
contribution of each member, `+` keeps them all.

For the signature `Funcs*Funcs^^Funcs`:

    F + G = fn (F.AsSet + G.AsSet)

The addition overload applied to the operands' underlying collections of mappings. Like
`F | G` it is defined at `x` where either operand is defined at `x` and permits every
result value that either operand permits. The two differ in solution multiplicity:
`F + G` retains both contributions wherever both operands are defined, so it is
nondeterministic on exactly `dom F & dom G` — two solutions at every such argument, even
where the results agree — while `F | G` unifies agreeing mappings. With `F` and `G` from
the example under `|`, `(F + G) 4` yields `40` twice under backtracking, where
`(F | G) 4` yields it once; at `2` both yield `20` or `21`.

#### `-`

Neutral. Subtracts the right operand from the left operand.

#### `*`

Neutral. For numeric operands, the product. For set operands, the fusing product, one
level deep: the members of `A * B` are the products obtained by fusing a member of `A`
with a member of `B`. `int*float` is the set of tuples whose first component is an `int`
and whose second is a `float`, and `int*int*string` is the set of three-tuples
regardless of grouping. On sets, `/` is the inverse of `*`, and `**` is the non-fusing
product. See [Product sets](#product-sets).

#### `/`

Neutral. For numeric operands, divides the left operand by the right operand. For set
operands, divides a tuple set by a set, yielding the tuple set of the remaining
components. Given `S = int*float*string`, `S/string` is `int*float` and
`S/(float*string)` is `int`; a divisor that does not correspond to a trailing group of
components is an error.

#### `%`

Neutral. The remainder of dividing the left operand by the right operand.

#### `/%`

Neutral. Divides the left operand by the right operand and yields the tuple of quotient
and remainder.

#### `^`

Neutral. Raises the left operand to the power of the right operand.

#### `.`

The left operand is an expression that has properties. The right operand is a declarative. 

- In referential context, `whole.part` selects the property `part` of `whole`.
- In declarative context, `whole.part` declares a property `part` directly on `whole`.
- In prefix position, `.part` denotes the function `whole => whole.part`. `.Name` accepts
  a value with a `Name` property and returns it; `.(string Name)` is defined only where
  that property is a member of `string`.

~~~ting
Persons person => person./record(Name,Age=CalculateAge(DateOfBirth,DateOnly.ToDay))
~~~

#### `..`

The left operand must be a set. The right operand is always declarative.

- In referential context, `some..prop` denotes the function from a member of `some` to the
  property `prop` of that member. Every member of `some` must have a property compatible
  with the right operand.
- In declarative context, `some..prop` declares the property `prop` on every member of
  `some`.
- In prefix position, `..prop` denotes the function from a set whose members have a `prop`
  property to the projection of that property over the members.

#### `catch`

Right-referential. The left operand is evaluated under an implicit try; the right operand
is a function that handles exceptions. See [Exceptions](#exceptions) for the full rules.

### Nullary operators

A nullary operator is an operator that takes no operands.

#### `undefined`

The value of `undefined` is *undefined*. Essentially the same as throwing an `UndefinedException`.

