# Ting Language Specification

> Note: This is a draft


## Design Model

Ting is logic programming language. The logic oriented core allows it to unify the functional- and object-oriented paradigms.

## Programs

A compilation unit is a single expression held in a single source file. There is no
declaration list at file level: the file *is* the expression, and everything it declares
it declares through scope-introducing forms within that expression.

The surface syntax for combining compilation units into modules is not yet specified.
The semantic boundary required for separate compilation is specified by function
contracts: the defining compilation context may analyze an implementation body, while a
consumer of an exported value may use only its contract. This does not require a second
syntactic category for module-level declarations.


## Lexical Structure

### Tokenization

Longest-match tokenization applies.

Example: `a<=b` tokenizes as `a`, `<=`, `b`.

The character tokens are tokens formed from non-alphanumeric characters, as opposed to
keyword tokens such as `let`, `where` or `catch`.

The following tokens exists:

- Unary prefix operators. [Unary prefix operators](#Opers)
- Binary infix operators. See 
- Nullary operators. The keyword token `void`. See [Nullary
  operators](#nullary-operators).
- Bracketing tokens.

#### Unary prefix

| Token | Name             |
| ----- | ---------------- |
| `!`   | logical negation |
| `-`   | arithmetic negation (see [`-`](#--negation)) |

The remaining prefix operators are keyword tokens: `let`, `public`, `private`,
`protected`, `internal`, `record`, `records`, `tuple`, `tuples`, `fn`, `class`,
`primary`, `candidate`, `total`, `partial`, `throw` and `ask`.

#### Binary infix

Listed in order of increasing binding power. Tokens on the same row bind equally tightly.

| Tokens                           | Role                                                 |
| -------------------------------- | ---------------------------------------------------- |
| `where`                          | right-declarative condition                          |
| `in`                             | left-declarative conditional value                   |
| `=` `?=`                         | identity, unify                                      |
| `then` `else`                    | implication, alternative                             |
| `contract` `asks` `throws`       | function abstraction, effect and exception contracts |
| `->`                             | mapping                                              |
| `>>` `<<` `\|>` `<\|`            | composition and application                          |
| `=>`                             | lambda                                               |
| `./` `../`                       | left-referential expression scope, left-referential expression-member scope            |
| `catch` `answer`                 | exception and effect handling                        |
| `\`                              | such-that / restriction                              |
| `\|` `\|\|` `\|\|\|`             | disjunction, guarded disjunction, mutually guarded disjunction |
| `&` `&&` `&&&`                   | conjunction, guarded conjunction, mutually guarded conjunction |
| `?` `??`                         | restriction, alternative-on-undefined                |
| `:` `::` `:::`                   | member-of, collection-of, sequence-of                |
| `<` `<=` `>=` `>` `==` `!=` `!:` | comparison, attribute-blind equality, non-membership |
| `...` `...<` `<...` `<...<`      | ranges                                               |
| `;` `;;`                         | choice, alternative-on-undefined                     |
| `^^`                             | reversed exponentiation, set of functions            |
| `**`                             | non-fusing product                                   |
| `+` `-`                          | addition, subtraction                                |
| `*` `/` `%` `/%`                 | multiplication, division, modulo, divmod             |
| `^`                              | exponentiation, set of functions, tuple power        |
| `.` `..`                         | property, member property                            |

The alphabetic entries in the table are keyword binary infix operators. The binding
power of `contract` makes `f = p => b contract C` group as
`f = ((p => b) contract C)`.

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

Name resolution compares the resulting names after the replacement expressions have been evaluated. Interpolated identifier references are subject to the same compile-time resolution requirement as other references.

Examples:
- ``` $`Name{n}` ```
- ``` $$`Name{{n}}` ```
- ``` $$$`Field{{{index}}}` ```

Format specifiers




### Numeric literals
- Numeric literals include integer and floating forms.
- Numeric literals are unsigned. A leading `-` is never part of the literal; it is the
  negation operator applied to it. This is what keeps `n-1` a subtraction rather than `n`
  applied to the literal `-1`.
- Numeric separators are supported (example in syntax docs).

### Whitespace and indentation
- Grammar-shift operators can use indentation-sensitive forms.
- If shift operators end a line, subsequent indented lines are folded.

## Declarations and Binding

Ting has no separate declaration syntax. Whether an identifier occurrence declares a
name or refers to one is determined by walking the ordered stack of scopes covering
that occurrence after any enclosing scope-introducing operands have been analyzed and
their scopes materialized. Each frame on the stack is independently either
**declarative** or **referential** at that point in the expression, but the role of
the occurrence is an outcome of the whole walk, not a property of the top frame alone.

Name binding walks the stack from the innermost frame outward, using the scopes that
have already been introduced by surrounding operators and declarations:

1. In a referential frame, a name that is proved to be always declared in that scope resolves the occurrence and ends the walk. The walk continues outward only if the name is proved to be never declared in that scope.
2. If the walk reaches a declarative frame without being resolved by an earlier
  referential frame, the occurrence becomes a declaration. The same declaration
  identity is contributed to that frame and to following declarative frames only for
  as long as they remain adjacent in the walk.
3. The first referential frame encountered after declaration has started is a barrier:
  declaration propagation stops there and does not resume in farther declarative frames.
4. Contributing a name already present in a declarative frame adds a constraint to the
  existing binding; the declarations are unified rather than shadowing one another.
5. If every frame is referential and none contains the name, the occurrence is unresolved and compilation fails.

This makes declaration composition sensitive to the order and mode of individual scope
frames. An inner referential frame can resolve a name before an outer declarative frame
is reached, while a name absent from that inner frame may be declared by the outer one.
Scope-introducing operators such as `./` and `../` first analyze the operand that
defines the scope, then analyze the other operand under that scope; the binding walk is
therefore performed after the relevant scope has been populated. When declaration starts,
only the contiguous declarative segment participates in the same declaration identity;
interleaved referential frames are hard boundaries.

Classification of an occurrence may be deferred during compile-time analysis. An occurrence whose stack begins with a declarative frame declares immediately; every other occurrence stays undecided until the scopes its walk crosses are sufficiently determined. Declarations are settled first, to a fixpoint, and references are resolved only afterwards, so no reference resolves before every reachable declaration has been recorded. An expression scope (`./`) exposes names from its defining operand itself; for a record, these are its fields. An expression member scope (`../`) instead exposes fields of the set's members as functions from those members to field values. Determining whether either scope provides a particular name may require logical resolution of its defining expression.

Every identifier reference must be resolved at compile time. At each referential frame reached by the binding walk, the compiler must establish that the scope always declares the referenced name or that it never declares that name, under the constraints applicable at the occurrence. If it always declares the name, resolution uses that scope. If it never declares the name, the walk continues outward. A scope that may declare the name in some cases but not others is a compilation error for that occurrence. Inability to prove either always or never is also a compilation error; an unknown intermediate scope must not be skipped to select an outer declaration or to declare the name in an outer frame.

Pending classification is an intermediate compiler state only. If analysis can make no further progress and a reference remains unresolved, compilation fails. No reference is deferred to runtime. This requirement concerns the declaration to which a name refers, not whether its bound value is known at compile time.

### Shadowing

Repeated declarative occurrences of a local name in the same declarative scope contribute constraints to the same binding. They do not introduce separate declaration identities and are not redeclaration errors. All contributed constraints apply to that binding; a later occurrence does not replace an earlier constraint.

```ting
((value = 1) & (value = 1)) in value
```

Both occurrences on the left of `in` constrain one `value` binding in the scope introduced by that `in`. The body refers to that binding.

A declarative expression may shadow an ordinary identifier only if that identifier was declared outside the current local expression. Such identifiers include names made available through `public` and names introduced through the open-scope operators `./` and `../`. Introducing a nested declarative scope does not by itself permit shadowing an ordinary identifier declared within the current local expression. Contributing constraints to the same declaration identity is not shadowing. The keyword `this` is an exception: a nested declaration may introduce a new binding that shadows the outer `this`, as specified under [This Binding](#this-binding).

A nested local scope that redeclares an ordinary identifier already declared within the current local expression causes a compilation error. The new declaration is not interpreted as a reference or as an additional constraint on the outer declaration. This error concerns redeclaration, not whether the values assigned to the two declarations agree. Record field declarations are distinct from local binding declarations and are not prohibited by this restriction: a record may declare a field with the same name as a surrounding local binding (see [Records](#records)).

```ting
value = 1 in (value = 2 in value)
```

This expression is a compilation error: the inner `in` declares `value` again, while the outer `in` has already declared it in the local expression. It is not an expression that becomes undefined because `1` and `2` fail to unify.

### Property resolution

A declaration made through property access has a host. For example, `int.Zero = 0` declares `Zero` directly on `int`, whereas `int..Double = expression` declares `Double` on members of `int`. Hosted names are not ordinary lexical declarations: bare lexical lookup skips them, and property access finds them through a receiver matching their host.

In `receiver.property` and `receiver..property`, the receiver is referential. The property expression is interpreted against the receiver, not looked up as an ordinary lexical name. Resolution first seeks a field of the receiver's record structure, then a hosted property whose host matches the receiver. A direct host matches the receiver itself; a member host matches a set to which the receiver belongs.

For an unqualified name, lexical lookup precedes implicit property lookup through a covering `this` binding. Implicit lookup interprets the name as a property of `this`. An undetermined receiver or intervening scope does not authorize a lexical fallback: property references may be deferred only during compile-time analysis and must resolve before compilation can succeed.

The `this` binding supplied by a record does not provide implicit lookup of that record's fields within field expressions. References to those fields require `this.field`, even when no surrounding binding supplies the unqualified name (see [Records](#records)).

### Scope-introducing forms

The following forms introduce declarative scopes:

- Visibility scopes: `public`, `private`, `protected`, `internal`
- Aggregate scopes: `record`, `records`, and the named section of a product
- Local binder: `let`
- Left-declarative operators: conditional value `in`, lambda `=>`, such-that `\`, contract `contract`
- Right-declarative operator: `where`

`let` introduces a local scope that is declarative throughout its operand. A
left-declarative or right-declarative operator instead introduces a mixed-mode local
scope as described under [Declarative and referential operators](#declarative-and-referential-operators).

### Local binding
- Canonical form: `pattern = expr in body`.
- `in` introduces a local scope spanning both operands. Its new frame is declarative over the left operand and referential over the right operand, just as for `=>`. Inherited frames retain their modes.
- `let` is not required to declare the pattern in this form. An explicit `let` introduces an additional scope that is declarative throughout its operand.
- Recursive and unification-style examples use `?=`.

### Lambda

- Form: `pattern => expression`
- The operator introduces one local scope spanning both operands.
- That new scope is declarative in the parameter pattern and referential in the body.
- Scope frames inherited from the surrounding expression keep their existing modes.
- The body can therefore resolve a parameter in the lambda scope while declaring a
  different name in an outer declarative scope.

For example:

    (int x => x * 2 + k) \ k = 42

In the parameter pattern, `x` is contributed to both the lambda scope and the enclosing
such-that scope, since both are declarative there. The application callee `int` is
referential and therefore cannot become a declaration. In the body, the lambda frame is
referential. The body occurrence of `x` resolves there, but `k` is absent, so the walk
continues to the still-declarative such-that frame and declares `k`. On the right of `\`,
that frame is referential and both names can be used as constraints.

The ability of an enclosing declarative scope to collect declarations from a nested
function is intentional:

    (int x => c) \ c = x * 2

This denotes the same relation as `int x => x * 2`. In the such-that scope, `x` and `c`
are nondeterministic logical variables ranging together over the mappings of the
function. The predicate constrains the relationship between each accepted argument and
its result.

### Functions via fn mapping collections

- `fn` is a unary operator over a collection of mappings.
- `fn { p1 -> r1, p2 -> r2, ... }` is unary `fn` applied to a set literal of mappings.
- If several mappings in that collection share the same argument part, the resulting function is nondeterministic at that argument.

## Values and Equality

A value has three layers, from the outside inward:

1. Attributes: values attached to a value, observed by structural equality but ignored by attribute-blind relations.
2. Class membership: the nominal identity conferred by `class`.
3. Structural content: the literal, product, set, list, or function structure of the value.

The two equalities observe these layers as follows:

| Relation | Attributes | Class membership | Structural content |
| --- | --- | --- | --- |
| `=` | observed | observed | observed |
| `==` | ignored | observed | observed |

`primary e` erases attributes throughout the structure, not only at the outermost level. It is total, deterministic, and idempotent, and preserves class membership. `candidate e` erases class membership to yield the candidate value while preserving attributes. Both are keyword intrinsics, not properties that can be shadowed by user-declared fields.

`a == b` has the meaning of `(primary a) = (primary b)`. Attribute-blind relations operate on attribute-erased operands; for example, numeric or set comparison ignores attributes on either operand. Attribute erasure does not erase class membership: values of different classes do not become equal merely because their candidates are equal. Class erasure is explicit through `candidate`.

`primary` and `candidate` preserve the ambient binding context of their operands. In a declarative context, `primary a = primary b` can declare `a` and constrain it up to attribute erasure without constraining its attributes.

## Sets

Ting models types as sets.

### Built-in scalar sets
- `byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`
- `float`, `double`, `decimal`
- `char`, `string`, `bool`

### Unit

`unit` is the singleton set whose only member is the empty tuple:

  unit = { () }

The empty tuple `()` is an ordinary defined value with no fields. It is distinct from
`void`, which is the absence of a value, and from `{}`, which is the empty set.

A function in `unit ^^ A` accepts no information; its sole possible argument is `()`.
A function in `A ^^ unit` produces `()` where it is defined and therefore communicates
definedness without carrying result information.

For the fusing product, `unit` is the identity: `unit * A = A` and `A * unit = A`.
The non-fusing product retains the empty tuple as a component, so the members of
`unit ** A` are `((), a)` for `a : A`, and the members of `A ** unit` are `(a, ())`.

### Built-in meta sets

- `Sets`: set of all sets
- `Funcs`: set of all functions

### Set definitions

A **set definition** is a comma-separated list of expressions enclosed in `{` and `}`.
The expressions in the list are the definition's **element expressions**; the list may
be empty. 

Each element expression contributes every value it can assume, not the expression itself: 
- one value if it is deterministic
- no value at all if it is undefined — an element expression that evaluates to `void`
  contributes nothing, rather than contributing a member
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
can be defined by lambdas or by applying `fn` to a collection of mappings.

### Partial functions

Three sets must be distinguished for a function:

1. Its **declared argument set**, such as `float` in `float ^^ float`.
2. Its **admitted domain**, which refines the declared argument set with restrictions in
   the parameter pattern.
3. Its **defined subset**, the admitted arguments for which evaluation actually returns
   at least one result.

A function is **total** where its defined subset equals its admitted domain. It is
**partial** where an admitted argument may produce no result. Membership in `A ^^ B`
states the declared argument and result sets but says nothing about totality: `A ^^ B`
contains both total and partial functions. `total (A ^^ B)` is the subset guaranteed to
return at least one result for every admitted argument.

`total` applies to sets of functions, not to function bodies. It is primarily useful in
contracts, where it makes a definedness guarantee available across an opaque compilation
boundary. A visible implementation need not be annotated with `total`; the compiler may
prove from its body that it belongs to a total function set.

There are two ways to make a potentially undefined operation explicit. The preferred
way, when its definedness condition is known, is to narrow the admitted domain:

    float x ? != 0 => 1 / x

Zero is not admitted by this function. The function is total because division returns a
result for every argument it does admit.

The admitted domain may instead remain broad while the body is marked `partial`:

    float x => partial 1 / x

Here every `float` is admitted, but evaluation may be undefined for an admitted argument.
`partial` tells the compiler not to derive a more precise definedness condition by
inspecting the marked expression. A consumer must conservatively allow any invocation to
be undefined.

`partial` has no second meaning as a filter over function sets. It is comparable to an
unspecified throws clause: it records that undefinedness may escape from the marked
expression without describing the condition that causes it. If that undefinedness is
handled before leaving the function, the function may still satisfy a `total` contract.

    float x => (partial 1 / x) ;; 0

Without `partial`, a body may still be inferred to have a smaller defined subset. For
example, the body of `float x => 1 / x` reveals that evaluation is undefined at zero. At
a call site the compiler propagates that potential undefinedness unless it can prove the
actual argument is nonzero. The caller may establish that restriction, handle
undefinedness, or itself remain potentially undefined.

Undefinedness is not a value and is never added to the result set. Evaluation either
continues with a result or transfers to an undefined continuation, essentially by
throwing `UndefinedException`. It cannot be bound, stored, passed, or compared.
Undefinedness is observable only through constructs that handle or observe definedness,
including `;;`, `?`, `;`, guarded logical operators and `catch`.

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
`F.AsSet <= A * records(B Result)`. Its successful mappings recover its defined subset,
`def F = F.AsSet / records(B Result)`, and its successful result values recover
`range F = F.AsSet..Result`. Neither the declared argument set nor the admitted domain
can in general be reconstructed from successful mappings: they are part of the function's
declaration and restrictions. Functions whose arguments are themselves nested tuples use
`**` argument sets; the nesting survives because fusion is one level deep.

Where several mappings of `F` share the same argument part, `F` is nondeterministic at
that argument, yielding the `Result` of each such mapping; multiplicities are observable
as solution multiplicity under backtracking.

Two functions are equal under `==` exactly where their underlying collections are equal:
functions compare extensionally.

> Note (draft): the mechanism that names the result field — a structural named field
> `Result` versus a name published on the last position of a plain tuple by the
> `->`/`fn`/`^^` machinery — is not yet final.

### Function contracts and separate compilation

A function contract is a set of admissible function values. Attaching a contract to an
implementation has the form:

    implementation contract ContractSet

`contract` is left-declarative. It introduces one local scope spanning both operands;
that frame is declarative over the implementation and referential over the contract.
Inherited scope frames retain their modes. Declarations collected from a function inside
the implementation, including its argument and result variables, are therefore available
to an inline contract expression.

The implementation must be a member of `ContractSet`. While compiling the defining
compilation unit, the compiler may inspect the implementation and use its full symbolic
relation. Across a separate-compilation boundary, the compiled implementation remains
callable, but its body is opaque: consumers may use only constraints entailed by the
contract set. Facts inferred from the hidden body do not cross the boundary.

Contracts are sets rather than individual functions so ordinary set operators compose
their requirements. Intersection requires a function to satisfy both contracts:

    NonZeroArg = { x ? != 0 => _ }
    Forward = { bound x => _ }

    Reciprocal =
        float x => 1 / x
        contract total (float ^^ float) & NonZeroArg & Forward

`!= 0` is the binary `!=` operator used in prefix position, and therefore denotes the
predicate `value => value != 0`. The restriction `x ? != 0` admits every value satisfying
that predicate; by itself it is not restricted to numbers. `float ^^ float` separately
states the visible argument and result sets, while `total` guarantees a result for every
admitted argument. `_` leaves the corresponding value
unconstrained, so the set expression ranges over every function satisfying the stated
restriction rather than exposing a particular result relation.

The contract is deliberately allowed to reveal less than the compiler can infer from the
implementation. A complete opaque contract must explicitly state every fact on which a
consumer may rely, including argument and result sets, admitted-domain restrictions,
supported binding modes, and any totality guarantee. If the contract does not use
`total`, the consumer must conservatively allow the function to be undefined at an
admitted argument even where the hidden implementation is known to be total in its
defining compilation unit.

### Binding status and implementation modes

`bound` and `free` are built-in predicates over the current binding state of logical
variables. They support compiler flow analysis and allow one relation to provide
different implementations for different directions of use. For example, a contract for
integer addition can describe three solving modes:

    {
        (int a, int b) => int c \
            (bound(a, b) & free(c) |
             bound(a, c) & free(b) |
             bound(b, c) & free(a))
    }

The first mode computes `c` from `a` and `b`; the others solve for `b` or `a`. These are
not three different mathematical functions. They are implementations of the same
relation `a + b = c` selected according to available information.

Mode applicability is monotonic with respect to additional bindings. A mode that can
produce a variable when it is free remains usable when that variable is already bound:
the compiler may introduce a fresh free output for the selected implementation, compute
it, and unify it with the bound actual value. Thus, when `a`, `b` and `c` are all bound,
the compiler may use any of the three modes and the call succeeds exactly when the
computed value unifies with the actual value. Directional implementations of one
function are required to realize the same relation, so mode selection may affect
execution strategy but not meaning.

Supported modes and totality guarantees are function metadata rather than successful
mappings. Contract membership and structural equality `=` observe this metadata, while
attribute-erasing equality `==` continues to compare functions extensionally by their
underlying mappings.

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

In juxtaposition `f x`, the callee `f` is referential in every covering scope frame, while the argument `x` retains the ambient frame modes. Thus `float a` reads `float` and declares `a` when a declarative frame covers the argument. A callee cannot become a declaration by passing through an inner scope into an outer declarative frame.


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

Parentheses around a single expression group that expression: `(e)` denotes `e` and does not introduce an additional product layer.

### Records
- `record` defines a product with only named fields (see [Products](#products)).
- `records` denotes sets of records.
- Field access uses property selection (`.`).

A record declares its own fields independently of surrounding local bindings. A field may have the same name as a surrounding binding without redeclaring or constraining that binding.

```ting
value = 1 in record(value = 2)
```

This expression is valid and produces a record whose `value` field is `2`. The surrounding local `value` remains `1`.

A single name used as a record field declaration both declares the field and references the name for its value. This shorthand has the meaning of an explicit same-name field equation:

```ting
value = 1 in record(value)
value = 1 in record(value = value)
```

These expressions are equivalent and produce a record whose `value` field is `1`. In the explicit form, the left occurrence declares the record field and the right occurrence references the surrounding local `value`, not the newly declared field. The shorthand therefore combines two binding roles; it is not solely a declaration or solely a reference.

Within a record field expression, the record's `this` binding denotes the record being defined. `this.field` references a field of that record, including the field whose expression contains the reference or another field of the same record. Thus `this.value` can reference the record's own `value` field, distinct from a surrounding local binding named `value`. Such references are subject to the same compile-time resolution requirement as other property references.

The `this.` qualification is required for these references. An unqualified reference does not fall back to a field of the record when no surrounding binding supplies the name. Consequently, `record(value)` is a compilation error if its reference to `value` cannot otherwise resolve; declaring the record field does not satisfy that reference or turn the shorthand into an unconstrained field declaration.

## Classes

- Classes are inclusive sets over candidate sets.
- Equal payloads in same class are value-equal.
- Created with `class`, instantiated with `new`.
- Extended with `extend`.

## Entities
- Entities are identity-based and exclusive (only constructed instances).
- Equal payloads do not imply equal entities.
- Escaping entities may require explicit stable identity fields (per docs).

## Enums

- Enumerations are declared with `enums` syntax.
- Tagged variants with payloads are documented by examples.


## This Binding

`this` can be explicitly bound and can also be introduced by context operators.

The open-scope operators `./` and `../` do not introduce a `this` binding. Opening an expression scope or an expression member scope does not itself bind or rebind `this`.

Documented contexts that bind `this` include:
- `record`, `records`
- `tuple`, `tuples`
- explicit lambda parameters named `this`
- local binding such as `this = ... in ...`

In a record field expression, the record supplies itself as `this`. References to its own fields require `this.field`; this record context does not make the fields available through unqualified references (see [Records](#records)).

When `this` is in declarative position, members of its value/type are available in scope by unqualified names, except for the record-field context described above.

`this` may be redefined in a nested scope within the same local expression. A new declaration of `this` introduces a distinct binding that shadows the outer `this` within the nested scope; it does not add constraints to the outer binding. This is an exception to the prohibition on redeclaring ordinary local identifiers. References to `this` resolve to the nearest covering binding according to the scope walk and must resolve at compile time. Outside the nested scope, the outer binding remains in effect.

```ting
this = 1 in (this = 2 in this)
```

This expression yields `2`: the inner `in` introduces a new `this` binding, and the final reference resolves to it. The corresponding expression using an ordinary name such as `value` instead of `this` remains a redeclaration error.

## Nondeterminism

TODO: Choice, ranges, and set/list expansion interact via backtracking semantics.
TODO: Undefinedness participates in operator-specific ways (notably choice and guarded
logical operators). Note that undefinedness is the *absence* of a value, not a value: an
undefined operand contributes no solution, and cannot be compared against. See
[`void`](#void).

### Unenumerable nondeterminism

TODO

### Enumerable bounded nondeterminism

TODO

### Enumerable unbounded nondeterminism

TODO


## Axioms and solver rules

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

The `catch` operator is a binary infix operator that catches and handles exceptions thrown while evaluating an expression. The left operand is an expression that is evaluated under an implicit exception trap. The right operand is the exception handler. It is a function which accepts exceptions. Applying the handler is outside that trap, so an exception thrown while evaluating the handler propagates to an enclosing `catch`.

The `catch` operator is right-referential.

Evaluation proceeds as follows:
1. The left operand is evaluated.
2. If no exception is thrown, the result of the left operand is the result of the entire `catch` expression.
3. If an exception `ex` is thrown during evaluation of the left operand, **and** the right operand function is defined for `ex`, then the value of the entire `catch` expression is the value of applying the right operand function to `ex`.
4. If the catch function is not defined for `ex`, then the exception continues to propagate.

Examples:

```ting
expr catch Exception ex => "Error: " + ex.Message   // Catches any exception, binds to ex

expr catch (ArgumentException e) => "Invalid argument: " + e.ParamName   // Catches ArgumentException only

expr catch fn{
  (ArgumentException _) -> "Bad arg",
  (NullReferenceException _) -> "Null ref",
  _ -> "Other error"   // Fallback for any exception type
}
```

The `catch` operator is left-associative.

### Exception contracts

A function may be augmented with a `throws` contract. `F throws E` requires `E` to be a
set of exception values and declares the exceptions that may escape applications of `F`.
The declaration is an upper bound: every exception that may escape must be a member of
`E`, but `F` need not throw every member of `E`. An exception handled within `F` does not
escape `F`. Exceptions thrown while evaluating a catch handler are outside that handler's
trap and may escape the whole `catch` expression.

Without a `throws` augmentation, the throws contract of a function is indeterminate. Its
absence does not state that the function throws no exceptions. `F throws {}` gives `F`
an empty throws set and guarantees that no exception escapes an application of `F`. An
implementation with this augmentation may throw exceptions only where they are caught
before leaving the function.

Within a compilation unit the compiler may infer escaping exceptions from a visible
function body for analysis, but inference does not augment the function with a throws
contract. At a module export boundary, an omitted `throws` contract is collapsed to the
empty set, as if the function had been augmented with `throws {}`. The exported function
therefore guarantees that no exception escapes unless its `throws` contract explicitly
admits that exception. An exported function whose declared or defaulted set omits a
possibly escaping exception is a compilation error.

## Effects

Ting effects allow an expression to request a result from its surrounding evaluation
context. `ask` creates a request, and `answer` installs a function that handles requests
made while evaluating its left operand. Nested `answer` expressions form an ordered
handler stack from the innermost handler to the outermost. A handled `ask` receives the
results of applying its handler, and its containing expression continues from those
results.

### Asking

`ask R` evaluates `R` to a request value and searches the enclosing answer-handler stack
for the nearest handler whose function is defined for that value. The result relation of
`ask R` is the result relation obtained by applying that handler to the request. The
handler may be nondeterministic and return multiple results. Their values, undefinedness,
and solution multiplicity are those of the `ask` expression.

An ask does not terminate its containing expression. It resumes once for every result
returned by the handler. Each resumption continues evaluation of the containing
expression separately, with that handler result in place of `ask R`. Equal results with
distinct solution multiplicity produce distinct resumptions. If the handler returns no
result, the ask has no resumption and is undefined.

### Answering

`answer` is a binary infix operator. In `E answer H`, the left operand `E` is evaluated
with the function `H` pushed onto its answer-handler stack. When `E` asks a request value
that `H` can accept, `H` is applied to that value and each result of the application is
returned to the corresponding `ask` expression as a separate resumption. If `H` cannot
accept the request, lookup continues with the next handler on the enclosing stack.

The right operand is outside the scope of the handler it supplies. Consequently, an ask
made while evaluating `H`, including while applying `H` to a request, is offered only to
enclosing handlers.

`answer` is left-associative, so handlers may be stacked:

  E answer H1 answer H2

This groups as `(E answer H1) answer H2`. `H1` is tried before `H2` for asks made by `E`.
An ask made while evaluating `H1` is outside the scope of `H1` but inside the scope of
`H2`.

### Effect contracts

The compiler must determine statically that every reachable `ask` has a handler on its
answer-handler stack that can accept every request value the ask may produce. An ask not
handled within its containing function escapes that function. At each call site, the
escaping ask is analyzed as if it occurred at that point on the caller's handler stack.

`F asks A` requires `A` to be a set of functions. Its members describe the answer
handlers that may be required by applications of `F`: an ask admitted by one of those
functions may escape `F`, and the result set of that function describes the results the
ask may receive. The declaration is an upper bound and does not require `F` to make every
ask described by every member of `A`. An ask handled within `F` does not escape `F`.
Asks made by an answer handler are not handled by that same handler and may escape the
whole `answer` expression.

A function need not be augmented with `asks`. Without that augmentation its asks
contract is indeterminate: no conclusion can be drawn from the function value about
which answer handlers an application may require. In particular, absence of an `asks`
augmentation does not state that the function makes no asks.

`F asks {}` gives `F` an empty asks set and guarantees that no ask escapes an application
of `F`. An implementation with this augmentation may contain asks only where they are
handled before leaving the function.

Within a compilation unit the compiler may infer escaping asks from a visible function
body for analysis, but inference does not augment the function with an asks contract. At
a module export boundary, an omitted `asks` contract is collapsed to the empty set, as
if the function had been augmented with `asks {}`. The exported function therefore
guarantees that no ask escapes unless its `asks` contract explicitly admits a required
handler function. An exported function whose declared or defaulted set does not admit a
handler for a possibly escaping ask is a compilation error. A complete program in which
an ask can reach the end of its handler stack is also a compilation error.

For example, where `unit` is the singleton set containing `()`:

```ting
Writers = class string
Readers = class unit

ask Readers () >> ask Writers
answer || >>>
    Writers s => Console.WriteLine s
    Readers () => Console.ReadLine
```

The first ask produces the request `Readers ()`. The `Readers` case answers it with the
line returned by `Console.ReadLine`. Composition passes that result to `ask Writers`,
which produces a `Writers` request containing the line. The `Writers` case answers that
request by applying `Console.WriteLine`. Both asks are covered by the single handler
function formed with guarded choice.



## Operators

Ting's ordinary operators are surface syntax for applying functions: `a + b` applies `_+_` to the tuple `(a, b)`, and `!p` applies `!_` to `p`. Scope operators and semantic markers such as `contract`, `asks`, `throws`, `partial`, `throw`, `ask` and `answer` have additional scope, analysis, or control-flow rules specified in their respective sections. This specification does not prescribe the compiler's internal expression representation.

The functions an operator can name are taken from a scope available to the parser, so they are never looked up lexically and cannot be shadowed by a declaration.

### Declarative and referential operators

A **declarative operator** introduces a new local scope spanning its operands. The new
scope is declarative over one operand and referential over the other. It does not change
the modes of inherited scope frames.

A **right-referential** operator introduces no scope. Over its right operand it converts
every declarative frame already on the scope stack to referential, preserving the frames
and their order. Its left operand is analyzed first and may itself introduce the scope
used by the right operand. The right operand is then analyzed against that materialized
scope stack.

A **left-referential** operator is the mirror image: over its left operand it converts
every declarative frame already on the stack to referential, while its right operand is
ambient.



#### Neutral

A neutral operator forces no role on either operand and introduces no scope. Both operands
are ambient, so a neutral operator expression declares exactly where the surrounding
context declares and refers everywhere else. Neutrality is what allows an operator to be
used unchanged on both sides of a declaration.

Prefix operators are neutral in the same sense: their single operand is ambient.

#### Right-referential

The right operand is referential in every covering scope; the left operand is ambient.
No scope is introduced. Concretely, every declarative frame covering the operator is
viewed as referential while traversing the right operand.

This asymmetry is what gives `x = 5` its meaning. The right operand is a value to be read,
while the left operand inherits the surrounding context and therefore introduces `x` when a
declarative scope covers the expression. Nothing about the operator itself declares
anything; the operator only decides how its operands are interpreted against the scopes
already in force.

#### Left-declarative

A left-declarative operator introduces a local scope spanning the whole operator
expression. The new frame is declarative in the left operand and referential in the
right. Inherited frames keep their modes in both operands. Consequently, a name absent
from the new referential frame on the right may still be declared in an inherited
declarative frame.

This is not equivalent to placing an invisible `let` around a right-referential
operator. `let` makes its frame declarative throughout its operand, whereas a
left-declarative operator creates one frame whose mode differs between its operands.

#### Right-declarative

The mirror image of left-declarative. A local scope spanning the whole operator expression
is introduced. Its new frame is referential in the left operand and declarative in the
right, while inherited frames retain their modes.

### Operator Precedence and Associativity

TODO: precedence and associativity are intentionally left unspecified in this draft and will be defined later.

### Binary operators used as unary prefix operators

Any binary operator may be written in prefix position with a single operand. The result is
the function obtained by partial application: `( : rhs)` is a function that accepts `lhs`
and evaluates `lhs : rhs`.

`-` is the one operator for which the prefix position is not always partial application:
written immediately before its operand it is negation instead. See [`-`](#--negation).



### Unary prefix operators

A unary prefix operator is written before its single operand. The operators listed here
are prefix-only: the grammar accepts no infix form for them. Any binary infix operator may
also be written in prefix position, but that denotes partial application rather than a
distinct operator; see [Binary operators used as unary prefix
operators](#binary-operators-used-as-unary-prefix-operators).

#### `let`

Introduces a local scope spanning its operand. The new frame is declarative throughout the operand. Names declared by the operand are local to that scope. `let` is not required in `pattern = expr in body`, because `in` introduces its own left-declarative scope. An explicit `let` adds a separate frame; `in` preserves inherited frame modes, whereas the right-referential `=` makes all covering frames referential over its right operand.

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

A single-name field declaration also references that name for its value, as specified under [Records](#records).

#### `records`

Denotes the set of records whose fields are those declared by the operand.

`records E` has the meaning of `{ record E }`, collecting the record values described by the operand.

#### `tuple`

Marks its operand as an explicit tuple construction. It contributes no meaning of its own:
`tuple E` denotes `E`.

#### `tuples`

Denotes the set of tuples whose components are those of the operand.

`tuples E` has the meaning of `{ E }`, collecting the tuple values described by the operand.

#### `fn`

Neutral. Converts a collection of mappings into a function: `fn S`, where the members of
`S` are mappings (see [Mappings and the underlying
collection](#mappings-and-the-underlying-collection)), denotes the function whose
underlying collection is `S`. Where `S` contains several mappings with the same argument
part the function is nondeterministic at that argument; where `S` is a multiset,
multiplicities are retained as solution multiplicities.

`fn { p1 -> r1, p2 -> r2, ... }` is ordinary unary `fn` applied to a set literal of
mappings. It introduces no additional ordered-dispatch semantics beyond the mapping
collection itself. Where several mappings in that collection share the same argument part,
the resulting function is nondeterministic at that argument.

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
permit. Applied to a member `x` of `def F`, `!F x` is nondeterministically any value
other than `F x`; applied to a value outside `def F` it is nondeterministically any value
at all. Negation on functions is an involution — `!!F` denotes `F` — and `F & !F` is
undefined for every argument.

#### `-` (negation)

Neutral. Arithmetic negation: `-x` denotes the additive inverse of `x`.

`-` is also a binary infix operator, and prefix position is otherwise reserved for the
partial application of a binary operator. Spacing alone decides between the three
readings:

- `-` with whitespace before it and none after it is **negation**.
- `- ` with whitespace on both sides, applied to a single operand, is the **partial
  application** of subtraction, per [Binary operators used as unary prefix
  operators](#binary-operators-used-as-unary-prefix-operators).
- Every other combination is **subtraction**.

The rule is purely lexical and admits no exception:

| Expression | Reading                                    |
| ---------- | ------------------------------------------ |
| `n - 1`    | subtraction                                |
| `n-1`      | subtraction                                |
| `n- 1`     | subtraction                                |
| `n -1`     | `n` applied to the negation of `1`         |
| `f -1 -2`  | `f` applied to `-1`, then to `-2`          |
| `x + -1`   | negation                                   |
| `[1, -1]`  | negation                                   |
| `(-1)`     | subtraction, and therefore ill-formed      |
| `( -1)`    | negation                                   |

Nothing precedes the `-` in `(-1)`, so it is the binary operator and has no left operand.
A negated value that opens a bracketed construct, or a source file, must be written with
the space: `( -1)`, `[ -1, 2]`.

The rule exists because a negative value has to be usable as a juxtaposed argument
without taking the subtraction away from `n-1`.

Negation binds more tightly than every infix operator, because `-x` is a term. In
particular `-a.b` denotes `(-a).b` and `-a^b` denotes `(-a)^b`, in the same way that the
juxtaposition in `f a.b` denotes `(f a).b`.

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

#### `total`

Accepts a set of functions and denotes the subset guaranteed to return at least one
result for every argument in each function's admitted domain. It does not change those
functions or narrow their domains. A bare function set such as `A ^^ B` includes both
total and partial functions; `total (A ^^ B)` adds the definedness guarantee.

`total` is a set filter and has no expression-body meaning. Membership may be established
by analysis of a visible implementation or supplied by an opaque contract.

#### `partial`

Marks its operand as potentially undefined without changing its admitted values or
successful results. The operand is ambient. The compiler does not inspect through the
marker to infer a narrower definedness condition; it conservatively assumes that the
expression may transfer to the undefined continuation for any admitted bindings.
`partial` neither catches undefinedness nor turns it into a value.

#### `throw`

Unary prefix semantic form. The operand is ambient. `throw exception` raises the
exception denoted by its operand rather than returning a value. Its semantics are
specified under [Throw operator](#throw-operator).

#### `ask`

Unary prefix semantic form. The operand is ambient. `ask request` makes the request
denoted by its operand. Its semantics are specified under [Asking](#asking).

### Binary infix operators

A binary infix operator is written between its two operands.

#### `where`

Right-declarative. Introduces a local scope spanning the expression; the right operand
declares into that scope and the left operand refers to it. The value of the expression is
the value of the left operand on the condition that the right operand evaluates to true.

#### `./`

Left-referential expression scope. The left operand is analyzed first to produce a scope, and that scope is then made available to the right operand. The operator does not introduce a `this` binding.

When the left operand denotes a record, its fields are available as unqualified names in the right operand.

```ting
record(value = 1) ./ value
```

This expression yields `1`: the right-hand `value` resolves directly to the record's field. This is explicit scope opening by `./`, distinct from implicit field lookup within a record field expression, where `this.field` is required.

#### `../`

Left-referential expression member scope. The left operand is analyzed first to produce a member scope, and that scope is then made available to the right operand. For a set operand, fields of its members are exposed as functions from members of the set to field values. The operator does not introduce a `this` binding.

For a set `S` exposing a field accessor `field`, `S ../ field` denotes the function `S member => member.field`. The accessor may be partial: the field need not be present on every member of `S`, and applying the accessor to a member without that field is undefined. The operator does not evaluate the right operand separately for each member and yield the field values nondeterministically. References to the exposed functions must still resolve at compile time. This requirement concerns the availability of the accessor in the member scope, not whether the accessor is defined for every member of the set.

```ting
{ record(value = 1), record(value = 2) } ../ value
```

This expression is equivalent to:

```ting
{ record(value = 1), record(value = 2) } r => r.value
```

The result is a function that maps each record in the set to its `value` field, not the nondeterministic values `1` and `2`.

```ting
{ record(value = 1), record(other = 2) } ../ value
```

This expression is valid and denotes a partial function. Applying it to `record(value = 1)` yields `1`; applying it to `record(other = 2)` is undefined. The absence of `value` on some members does not prevent compile-time resolution of the accessor named `value`.

If no member of the set provides the requested field, the accessor is always undefined and the expression is a compilation error. This case does not produce an everywhere-undefined accessor or cause lookup of the requested name to continue in an outer scope.

```ting
{ record(other = 2) } ../ value
```

This expression is a compilation error because the `value` accessor is undefined for every member of the set.

#### `=>`

Left-declarative. `pattern => expression` denotes a function. The left operand is a
declarative pattern in the new lambda frame; that frame is referential in the right
operand. Inherited frames retain their modes. Names declared by the pattern are therefore
available in the body, while a body name absent from the lambda frame may still declare
in an inherited declarative frame.

#### `contract`

Left-declarative. `implementation contract ContractSet` requires the implementation to
be a member of the function set denoted by the right operand. The new contract frame is
declarative over the implementation and referential over the contract expression, so the
contract can refer to argument and result declarations collected from a nested function.
The value is the implementation paired with its opaque external description; see
[Function contracts and separate compilation](#function-contracts-and-separate-compilation).

#### `asks`

Right-referential. `F asks A` augments the function `F` with the asks contract `A`, whose
value must be a set of functions. Its semantics, including indeterminate local contracts
and the empty default at export boundaries, are specified under
[Effect contracts](#effect-contracts).

#### `throws`

Right-referential. `F throws E` augments the function `F` with the throws contract `E`,
whose value must be a set of exception values. Its semantics, including indeterminate
local contracts and the empty default at export boundaries, are specified under
[Exception contracts](#exception-contracts).

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

Neutral. `^` with its operands reversed: `A ^^ B` denotes `B ^ A`. It is therefore
defined for exactly the signatures `^` is defined for, and carries no meaning of its own.

Its principal use is the set of functions from the left operand set to the right operand
set, which is the reading that puts the domain first. `int ^^ string` is the set of
functions from `int` to `string`, the same set as `string ^ int`. The underlying
collection of a member `F : A ^^ B` consists of mappings that fuse a member of `A` with a
`Result` that is a member of `B`: `F.AsSet <= A * records(B Result)`. `A` and `B`
constrain the declared argument and result sets; `A ^^ B` includes both total and partial
functions. Use `total (A ^^ B)` where totality over the admitted domain is required.
Without that guarantee, potential undefinedness propagates to uses where it cannot be
ruled out from a visible body.

For the signature `int*set`, `3 ^^ string` denotes `string ^ 3`, the set of three-tuples
of strings.

Because the signatures are those of `^` reversed, they are `num*num`, `set*set` and
`int*set`. There is no `set*int` signature: `string ^^ 2` is ill-formed. In particular it
is not shorthand for `string ^^ string`, which must be written out.

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

Declarations contributed by the left operand are available when the now-referential
scope frames are searched on the right, so recursive bindings can be written with `?=`.
Its distinction from `=` is semantic: failed unification is undefined rather than the
boolean value `false`.

    let f ?= fn{ 0 -> 1, int n -> n * f (n - 1) } in f 3

#### `in`

Left-declarative. Introduces a local scope spanning both operands, with a new frame that is declarative over the left operand and referential over the right operand. Inherited scope frames retain their modes, just as for `=>`. Thus `pattern = expr in body` declares the pattern without requiring `let`.

The left operand is a proposition and the right operand is any expression. The value of the expression is the value of the right operand, provided the left operand evaluates to `true`. If the left operand evaluates to `false` or is undefined, the expression is undefined.

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
combination of `true`, `false` and *undefined*. *undefined* is not a third truth value:
it is the row for an operand that yields no value at all (see [`void`](#void)), and a
result of *undefined* means the expression yields no value either. The table of a guarded
operator uses one further operand value:

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

Denotes the guarded counterpart of `&` on functions. The guard applies to the
definedness test: where `x` is not a member of `def F` the result is undefined at `x`
without consulting `G`, whose definedness may itself be undecidable there. Where `F` is
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
definedness tests: a defined non-membership of `x` in either operand's defined subset
makes the result undefined at `x` even where the other operand's definedness there is
undecidable.
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
permits. It is defined at `x` where either operand is defined at `x`; both definedness tests
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
at `x`; `G` is ignored there — it does not narrow the result, and its definedness
may be undefined. Where `F` is not defined at `x` the result is `G x`. With `F` and `G`
from the example under `|`, `F || G` applied to `2` yields only `20`. Unlike the strict
operators, this is not the set overload on the underlying mappings — `F || G` is not
`fn (F.AsSet || G.AsSet)` — because the guard applies to the definedness test, not to mapping
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
`x` permits every value permitted by either operand. The guard applies to both
definedness tests: where one operand's definedness at `x` is undecidable but the other operand is
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

The left operand may be any collection, and in particular it may be a *multiset*:
duplicate elements are admitted, and each contribution is tested for membership. This is
what distinguishes `::` from `<=`. Both lift membership from a value to a collection of
values, but `<=` relates two *sets*, so it unifies duplicate members and cannot say
anything about multiplicity. `{* 1, 1, 2 *} :: int` holds, where the corresponding `<=`
reading does not exist. `::` says nothing about the order of the elements.

#### `:::`

Right-referential. Yields `true` when the left operand is an ordered sequence of members
of the right operand set.

It differs from `::` only in additionally requiring that the collection is ordered. Like
`::` it admits a multiset on the left, so duplicates are permitted:
`{* 1, 1, 2 *} ::: int` holds wherever that multiset preserves its generation order.

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

Right-referential. Equality that ignores attributes but observes class membership and structural content. It denotes `=` applied to the attribute-erased operands: `(primary a) = (primary b)`. For set operands it yields `true` when both sets have the same members. See [Values and Equality](#values-and-equality) for the value layers and erasure rules.

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
nondeterministic on exactly `def F & def G` — two solutions at every such argument, even
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

Neutral. Exponentiation.

For numeric operands, raises the left operand to the power of the right operand.

For set operands, `B ^ A` denotes the set of functions from `A` to `B`. This is the usual
reading of exponentiation on sets: the exponent is the domain and the base is the
codomain. `string ^ int` is the set of functions from `int` to `string`.

For the signature `set*int`, `A ^ n` denotes the set of `n`-tuples over `A`, which is `A`
fused with itself `n` times. `string^3` is the set of three-tuples of strings, the same
set as `string*string*string`. This is the same reading: `n` stands for the set of the
first `n` index positions, and a tuple is a function from index to component.

The defined signatures are therefore `num*num`, `set*set` and `set*int`. There is no
`int*set` signature: `2 ^ string` is ill-formed.

`^^` is `^` with its operands reversed, so `A ^^ B` denotes `B ^ A` in every signature.

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

Right-referential binary infix operator. In `expression catch handler`, the right operand
is an exception-handler function for the left operand. Its semantics are specified under
[Catch operator](#catch-operator).

#### `answer`

Right-referential binary infix operator. In `expression answer handler`, the right
operand is an answer-handler function for asks made by the left operand. Its semantics
are specified under [Answering](#answering).

### Nullary operators

A nullary operator is an operator that takes no operands. Ting has one, the keyword token
`void`.

#### `void`

`void` denotes *the absence of a value*. It is the only way to write undefinedness
directly in source.

**`void` is not a value.** It is not a member of any set — not even of a set that
contains everything — it cannot be bound to a name, held in a collection, or passed as an
argument. An expression that yields `void` yields nothing at all; there is no object
there for anything to observe. In particular `void` cannot be compared to anything:
`void = void`, `x == void` and `x != void` are not propositions that hold or fail, they
are themselves undefined. Definedness is therefore never tested by comparing against
`void`; the operators that observe definedness are `;;`, `?`, `;`, the guarded logical
operators, and `catch`.

`void` is not a unit value and not a null value. An empty tuple `()`, an empty set `{}`
and an empty list `[]` are all values, and each is distinct from `void`.

Where an expression evaluates to `void` the expression is *undefined*, which is
essentially the same as throwing an `UndefinedException` at that point. `void` is what
makes that outcome writable, so it is how a function is made explicitly undefined
somewhere in its domain:

    let F = int n => n < 0 then void else n * 2

`F` declares that it accepts an `int`; for a negative `int` it has no value to offer. It
does not *return* `void`, and it does not return nothing either: for such an argument it
does not return at all. Control leaves by the undefinedness path, which is equivalent to
throwing an `UndefinedException`. See [Partial functions](#partial-functions).

