# Ting Language Specification

> Note: This is a draft


## 1. Design Model

Ting is logic programming language. The logic oriented core allows it to unify the functional- and object-oriented paradigms.

## 2. The Core Expression

Ting's semantics are defined over a small internal form called the **core expression**. Surface syntax, operators and declaration forms are all lowered into it before any meaning is assigned, so the core expression — not the surface grammar — is the normative object of this specification. Later sections describe surface constructs by stating what they lower to.

Two properties explain why the core is as small as it is.

**There are no operators in the core.** Every infix and prefix operator is surface syntax for the application of an ordinary function. `a + b` applies the function named `_+_` to the tuple `(a, b)`; `x => body` applies `_=>_`; `!p` applies `!_`. The core has one application form and no operator forms at all. Operator functions live in a scope fixed by the grammar, so they are never looked up lexically and can never be shadowed by a user declaration.

**There is no declaration form.** Declaring and referencing are not different constructs. They are the same identifier occurrence, interpreted against a different kind of scope (§2.4).

### 2.1 Forms

#### Application

`apply f a` applies `f` to `a`. Juxtaposition (`f x`), every infix operator and every prefix operator lower to this form. An infix operator applies its function to a two-element tuple.

#### Aggregates

- `tuple (e₁, …, eₙ)` — positional product.
- `set {e₁, …, eₙ}` — set.
- `list [e₁, …, eₙ]` — list.
- `string` — a sequence of literal text segments and embedded expressions (an uninterpolated string is the degenerate case of a single literal segment).

The surface forms `tuple E`, `tuples E` and `records E` contribute no core form of their own: `tuple E` lowers to `E`, `tuples E` to a single-element set, and `records E` to a single-element set containing a record.

#### Literal

A primitive term: integer, floating-point, decimal, boolean or string.

#### Identifier

An identifier occurrence together with its **binding role** (§2.4). A name is not necessarily a fixed string — an interpolated identifier's name is itself a sequence of text segments and embedded expressions, and resolution compares names after those expressions are evaluated.

#### `this`

The `this` keyword. It carries a binding role exactly as an identifier does, and is declarative or referential by the same rule.

#### Scope introduction

- `local S e` — evaluate `e` under a new local scope `S`. `let E` lowers to this, as do the local-scope operators `=>`, `\` and `where`.
- `scoped S e` — evaluate `e` under an explicitly supplied scope. `./` and `../` lower to this.
- `publish A S e` — declare the names introduced by `e` in scope `S` with access modifier `A`. `public`, `private`, `protected` and `internal` lower to this.
- `record S e` — evaluate `e` as a record scope; every name `e` declares becomes a field of the record.

Note that the right-referential operators (`=`, `in`, `:`, `<`, `==`, …) introduce **no** scope. They only decide how their operands are interpreted against the scopes already in force.

#### Projection

- `project r m` — `r.m`. Selects or declares member `m` on `r` itself.
- `projectMembers r m` — `r..m`. Selects or declares member `m` on every member of the set `r`.

In both, the receiver is referential and the member is interpreted against the receiver, never lexically (§2.5).

#### `void`

The value of an expression that yields nothing.

#### `unknown`

The placeholder value of a name that has been declared but whose defining expression is not yet known. It is not writable in source; it exists so that a declaration is well-formed before its definition has been analyzed.

### 2.2 Semantic primitives

These forms have no surface syntax of their own beyond the constructs that normalize into them. They are what solving and evaluation actually operate on.

| Form | Meaning | Surface |
|---|---|---|
| `equals a b` | structural equality | `a = b` |
| `primary e` | attribute erasure | `primary e` |
| `candidate e` | class erasure | `candidate e` |
| `isDefined e` | definedness test | guards, `;;`, `?` |
| `throw e` | raise an exception | `throw e` |
| `catch e h` | handle an exception | `e catch h` |

`isDefined`, `throw` and `catch` are specified in §13. The remaining three are specified below.

There is deliberately no form for `==`. It is not primitive (§2.3).

### 2.3 Value layers, erasure, and the two equalities

A Ting value is layered. From the outside in:

1. **Attributes** — values attached to a value which carry no relational weight.
2. **Class membership** — the nominal identity conferred by `class` (§10.1).
3. **The structural core** — the value's own structure: literal, tuple, record, set, list or function.

Two intrinsic functions erase one layer each:

- **`primary e`** erases attributes. It is total, deterministic, idempotent and **deep**: it erases attributes throughout the structure, not only at the outermost level. It preserves class membership.
- **`candidate e`** erases class membership, yielding the candidate value the class was constructed over. It preserves attributes.

Both are intrinsics named by keyword, not properties, and this is deliberate. A member present on every value would occupy that name in every programmer-authored record, class and entity, and would be silently shadowed by any field of the same name. Members belong to the constructs that declare them; operations that apply to *all* values are keywords. Being keywords, they are also lowercase, which distinguishes them on sight from the PascalCase of declared members.

The two equalities differ exactly in which layers they observe:

| | attributes | class membership | structural core |
|---|---|---|---|
| `=` | **observed** | observed | observed |
| `==` | ignored | observed | observed |

`=` is the primitive relation, and it is the *only* attribute-sensitive relation in the language. `==` is not primitive. It is erasure followed by `=`, and it lowers accordingly:

    a == b      lowers to      equals (primary a) (primary b)

The core expression therefore has no value-equality form. The relationship between `=` and `==` is discharged at lowering time rather than carried as a law that later passes must respect: no pass can violate it, and no pass has to know about it.

Every other attribute-blind relation lowers the same way — the operator's own function applied to `primary` of each operand:

    a < b       lowers to      apply _<_ (primary a, primary b)

`=` is the only relation whose operands are not wrapped. Because `primary` is idempotent, wrapping is harmless where an operand is already erased.

Two consequences are worth making explicit:

- `==` remains **class-aware**. Values of different classes are never `==`, even when their candidates compare equal. Crossing a class boundary in a comparison requires promotion (§10.1), which is opt-in per class.
- No comparison ever erases class membership implicitly. `candidate` exists so that shedding a class is always explicit and visible in the source.

### 2.4 Binding roles

Every identifier and `this` occurrence carries one of three roles:

- **Declarative** — the occurrence introduces a name. It carries a declaration identity, and that identity is recorded in *every* declarative scope covering the occurrence. An occurrence covered by more than one declarative scope declares the name in all of them and unifies the results.
- **Referential** — the occurrence refers to a name declared elsewhere. It carries the ordered list of candidate scopes, nearest enclosing first.
- **Resolved** — a referential occurrence that name resolution has paired with a specific declaration identity.

An occurrence is declarative if any declarative scope covers it, and referential otherwise. Nothing in the identifier itself decides this; only the scopes in force at that point do.

### 2.5 Scopes

A scope maps declaration identities to entries. Each entry records the name, the expression the name is bound to (`unknown` until known), and optionally a **host**.

There are three kinds of scope:

- **Local scope** — a plain name table, introduced by `local`, `publish` and `record`.
- **Expression scope** — the declarations of a given expression, made available as a scope (`./`).
- **Expression member scope** — the declarations of the *members* of a given expression, made available as a scope (`../`).

A declaration made through a projection is **hosted** rather than lexical:

- **Direct host** — `int.Zero = 0` hosts `Zero` on `int` itself.
- **Member host** — `int..Double = …` hosts `Double` on each member of `int`.

Hosted names are invisible to bare lexical lookup. They are reachable only through projection on a receiver matching the host.

### 2.6 Resolution

A referential occurrence is resolved in this order:

1. **Lexical** — search the candidate scopes, nearest first, skipping hosted names.
2. **Member** — for `project`/`projectMembers`, resolve the member against the receiver: first as a field of the receiver's record scope, then as a hosted name whose host matches the receiver (a direct host matches the receiver itself; a member host matches the receiver's set).
3. **Implicit `this`** — if a covering scope binds `this`, resolve the name as a member of the set `this` belongs to.

If none succeeds, an unresolved-name diagnostic is reported.

Member resolution is **structural, not lexical**. Members are never found by lexical search, and a member whose receiver structure is not yet determined is deferred rather than falling back to a lexical match.

### 2.7 Surface forms and their lowering

This section indexes the surface syntax against the core. It covers the constructs the grammar currently accepts; a handful of operators appear in reference material without yet having a lowering, and are listed in §16.

Two things are recorded for each construct. **Core** is the form of §2.1 or §2.2 it produces. **Binding** is how it affects the binding role (§2.4) of its operands, using two terms:

- **Ambient** — the operand inherits whatever context surrounds the construct. It therefore *declares* where a declarative scope covers it and *refers* where none does. Ambient is the default; it is what makes a construct binding-neutral.
- **Referential** — the operand is a use-site regardless of the surrounding context. A name written there is always read, never introduced.

#### Terms

| Surface | Core |
|---|---|
| `12`, `1.5`, `1.5m`, `true`, `false`, `"text"` | `literal` |
| `$"text{e}text"` | `string` |
| `name`, `` `quoted name` ``, `` $`Name{e}` `` | `identifier` |
| `this` | `this` |
| `void` | `void` |
| `(a, b, …)` | `tuple (a, b, …)` |
| `{a, b, …}` | `set {a, b, …}` |
| `[a, b, …]` | `list [a, b, …]` |
| `(e)` | `e` — parentheses group only |

#### Application

| Surface | Core | Binding |
|---|---|---|
| `f x` | `apply f x` | `f` referential, `x` ambient |
| `⊕ x` | `apply ⊕_ x` | `x` ambient |
| `x ⊕ y` | `apply _⊕_ (x, y)` | per operator class, below |

Juxtaposition makes the callee referential and leaves the argument ambient. This is why `let float a = 42` declares `a`: `float a` reads `float` and, being under a declarative scope, declares `a`.

#### Operator binding classes

Operators are ordinary functions (§2), but they differ in how they interpret their operands. There are four classes.

**Neutral** — both operands ambient; the operator introduces no scope and forces no role:

`else`, `>>`, `<<`, `|>`, `<|`, `|`, `||`, `&`, `&&`, `...`, `...<`, `<...`, `<...<`, `;`, `;;`, `^^`, `**`, `+`, `-`, `*`, `/`, `%`, `/%`, `^`, and the prefix operators `!`, `fn`, `class`.

**Right-referential** — left operand ambient, right operand referential; no scope introduced:

`=`, `in`, `then`, `->`, `catch`, `?`, `??`, `:`, `::`, `:::`, `<`, `<=`, `>=`, `>`, `==`, `!=`, `!:`.

This class is what gives `x = 5` its asymmetry. The right side is a value to be read; the left side inherits the surrounding context and so introduces `x` when a declarative scope covers it. Nothing about `=` itself declares anything.

**Left-declarative** — introduces a local scope `S`; the left operand declares into `S` and the right operand refers to it. `x ⊕ y` lowers to `local S (apply _⊕_ (x, y))`:

`=>` (lambda parameter), `\` (comprehension binder).

**Right-declarative** — the mirror image: `S` is introduced, the right operand declares into it, the left refers to it:

`where`.

#### Scope-introducing and structural forms

These do not lower to function application.

| Surface | Core | Binding |
|---|---|---|
| `let E` | `local S E` | `S` declarative over `E` |
| `public E`, `private E`, `protected E`, `internal E` | `publish A S E` | `S` declarative over `E` |
| `record E` | `record S E` | `S` declarative over `E` |
| `records E` | `set {record S E}` | as `record` |
| `tuple E` | `E` | unchanged |
| `tuples E` | `set {E}` | unchanged |
| `a ./ b` | `scoped S b`, `S` = the declarations of `a` | `a` referential, `b` ambient |
| `a ../ b` | `scoped S b`, `S` = the member declarations of `a` | `a` referential, `b` ambient |
| `a . b` | `project a b` | `a` referential, `b` resolved against `a` |
| `a .. b` | `projectMembers a b` | `a` referential, `b` resolved against `a` |
| `primary E` | `primary E` | `E` ambient |
| `candidate E` | `candidate E` | `E` ambient |

`primary` and `candidate` leave their operand ambient, so `primary a = primary b` still declares `a` where a declarative scope covers it. The declaration is simply weaker: it constrains `a` only up to attribute erasure.

#### Relations

`=` lowers directly to `equals`. Every other relation erases attributes first (§2.3):

    a = b       lowers to      equals a b
    a == b      lowers to      equals (primary a) (primary b)
    a < b       lowers to      apply _<_ (primary a, primary b)

Precedence and associativity are not part of lowering and are specified separately (§14).

## 3. Lexical Structure

### 3.1 Tokenization
- Longest-match tokenization applies.
- Example: `a<=b` tokenizes as `a`, `<=`, `b`.

### 3.2 Identifiers

Regular identifiers start with a Unicode letter or underscore and are followed by zero or more Unicode letters, digits, or underscores.

#### 3.2.1 Quoted identifiers

A quoted identifier is opened by a run of one or more backtick characters and terminated by a run of exactly the same number of backtick characters.

Let M be the number of backticks in the opening run.

- The terminator is the first occurrence of exactly M consecutive backticks that is not part of a longer run.
- Backtick runs inside the identifier with length unequal to M are treated as literal backtick characters.
- A backtick run of length M that is part of a run longer than M does not act as a terminator.
- If the opening backtick run is immediately followed by a single space, and the terminating backtick run is immediately preceded by a single space, both spaces are trimmed from the identifier name.
- Trimming is performed only when both conditions are satisfied simultaneously; a leading space is not trimmed without a trailing space, and vice versa.

Examples:

| Written form | Effective identifier name |
|---|---|
| `` `foo` `` | `foo` |
| `` ` foo` `` | ` foo` (space not trimmed) |
| `` ` foo ` `` | `foo` (spaces trimmed) |
| ``` `` foo`bar `` ``` | ```foo`bar``` (inner single backtick is literal, spaces trimmed) |
| ```` ```foo``bar``` ```` | ```foo``bar``` (inner double-backtick is literal)  |
| ```` ``` foo ``bar ``` ```` | ```foo ``bar``` (inner double-backtick is literal, spaces trimmed)  |

#### 3.2.2 Interpolated identifiers

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
- $`Name{n}`
- $$`Name{{n}}`
- $$$`Field{{{index}}}`

Format specifiers


### 3.3 Whitespace and indentation
- Grammar-shift operators can use indentation-sensitive forms.
- If shift operators end a line, subsequent indented lines are folded.

## 4. Literals

### 4.1 Strings

A string literal produces a value of type `string`, which is an immutable sequence of Unicode characters. Several forms exist, distinguished by their opening delimiter and optional prefix characters.

#### 4.1.1 Regular strings

A regular string is opened by `"` and terminated by the next unescaped `"` on the same line.

- The content may not contain an unescaped `"` or a newline character.
- Escape sequences (§4.1.2) are processed.
- The string value is the sequence of characters produced after escape processing.

Example: `"Hello, world!"`

#### 4.1.2 Escape sequences

The following escape sequences are recognised inside regular strings and the expression blocks of interpolated strings. Each sequence begins with `\`.

| Sequence | Unicode | Meaning |
|---|---|---|
| `\\` | U+005C | Backslash |
| `\"` | U+0022 | Quotation mark |
| `\n` | U+000A | Line feed |
| `\r` | U+000D | Carriage return |
| `\t` | U+0009 | Horizontal tab |
| `\a` | U+0007 | Alert (bell) |
| `\b` | U+0008 | Backspace |
| `\f` | U+000C | Form feed |
| `\v` | U+000B | Vertical tab |
| `\0` | U+0000 | Null |
| `\uHHHH` | U+HHHH | Unicode scalar (exactly 4 hex digits) |
| `\UHHHHHHHH` | U+HHHHHHHH | Unicode scalar (exactly 8 hex digits) |
| `\xH[H[H[H]]]` | U+H… | Unicode scalar (1–4 hex digits) |

Any `\` not forming a recognised escape sequence is a compile-time error.

#### 4.1.3 Verbatim strings

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

#### 4.1.4 Raw strings

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

#### 4.1.5 Interpolated strings

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

| Form | Example |
|---|---|
| Interpolated regular | `$"Hello, {name}!"` |
| Multi-dollar regular | `$$"Value: {{x + 1}}"` |
| Interpolated verbatim | `$@"Path: {basePath}\file.txt"` |
| Interpolated raw | `$"""{"nested"}"""` |

### 4.2 Numeric literals
- Numeric literals include integer and floating forms.
- Numeric separators are supported (example in syntax docs).

### 4.3 Collection literals
- List literal: `[e1, e2, ...]`
- Tuple literal: `(e1, e2, ...)`
- Set literal: `{e1, e2, ...}`
- Multiset literal: `{* e1, e2, ... *}`

## 5. Declarations and Binding

### 5.1 Scope-introducing operators
The reference documents identify these as declaration-forming contexts:
- Visibility and module scope operators: `public`, `private`, `protected`, `local`
- Aggregate scope operators: `record`, `records`, `tuple`, `tuples`
- Function scope operators: `fn`, lambda `=>`
- Local binder: `let`

### 5.2 Local binding
- Canonical form: `let pattern = expr in body`
- Recursive and unification-style examples use `?=`.

### 5.3 Lambda
- Form: `pattern => expression`
- Left side is declarative pattern context.
- Right side is referential expression context under the bound names.

### 5.4 Function cases via fn
- Case function form: `fn{ p1 -> r1, p2 -> r2, ... }`
- Supports typed patterns and guarded patterns shown in examples.

## 6. Types, Sets, and Membership

Ting models types as sets.

### 6.1 Built-in scalar sets
- `byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`
- `float`, `double`, `decimal`
- `char`, `string`, `bool`

### 6.2 Meta sets
- `set`: set of all sets (not generally enumerable)
- `func`: set of all functions (not generally enumerable)

### 6.3 Membership operators
- `:` member-of
- `!:` not member-of
- `::` collection-of
- `:::` sequence-of

### 6.4 Set comparison relations
- `<`, `<=`, `==`, `>=`, `>` are subset/superset/equality relations on sets.

### 6.5 Set construction
- Extensional: `{1,2,3}`
- Intensional/restricted: `{ x \ predicate(x) }`
- Product sets use `*` between set expressions.

## 7. Functions

### 7.1 Functions as relations
Functions map argument values to result values. Function values can be defined by lambdas or case expressions (`fn`).

### 7.2 Sets as identity functions
TODO: describe an implicit promotion from set values to identity functions when a function is expected.

### 7.3 Function composition and application
- Composition operators include `>>` and `<<`.
- Application operators include `|>` and `<|`.
- Juxtaposition application (`f x`) is supported.

## 8. Operators

### 8.1 Arithmetic
- `+`, `-`, `*`, `/`, `^`, `%`, `/%`
- `*` is numeric multiplication or set Cartesian product depending on operand sets.
- `/` includes tuple-set division behavior as documented examples.

### 8.2 Logical operators with undefined-aware variants
- Strict: `&`, `|`, `!`
- Guarded: `&&`, `||`
- Mutually guarded: `&&&`, `|||`

### 8.3 Choice and alternatives
- `;` introduces ordered nondeterministic choice.
- `;;` provides alternative-on-undefined behavior.

### 8.4 Identity and unification
- `=` is a binary relational operator.
- `=` compares two objects structurally (structural core, class membership and attributes) and returns a truth value (`true` or `false`).
- `==` compares two objects ignoring their attributes. It is not primitive; it lowers to `=` over the attribute-erased operands.
- The layers each relation observes, the `primary` and `candidate` intrinsics, and the lowering that relates `=` and `==` are specified normatively in §2.3.
- In positions that require truth (for example, restrictions/guards), `a = b` must evaluate to `true`.

### 8.5 Restriction and partial binary application
- `?` is a binary restriction operator.
- `left ? predicate` restricts `left` to values that satisfy `predicate`.
- Any binary operator may be used in prefix position with one operand to form a function by partial application.
- Example shape: `(= rhs)` is a function that accepts `lhs` and evaluates `lhs = rhs`.

### 8.6 Range operators
- `...`, `<...`, `...<`, `<...<`
- Used for inclusive/exclusive interval generation and set/list expansion.

### 8.7 Property and projection
Ting distinguishes between two property operators:

#### 8.7.1 `.` property operator

The right operand is always declarative. It declares the properties that is selected from `whole`.

In **referential context**, `whole.part` references the property `part` of `whole`.


In **declarative context**, `whole.part` declares a property `part` directly on `whole`.


- 
- A referential `.` expression used in prefix position evaluates to a function of the form `whole => whole.part`.

#### 8.7.2 `..` member property operator
- In **referential context**, `some..prop` references the property `prop` of each member of the set `some` and yields a function from members of `some` to their `prop` property.
- In **declarative context**, `some..prop` declares the property `prop` on every member of the set `some`.
- The left operand must be a set.
- The right operand is always declarative.
- A referential `..` expression used in prefix position evaluates to a function from members of the left-hand set to the corresponding property on each member.

### 8.8 Exception operators
- `throw` raises exceptions.
- `catch` handles exceptions.
- There is no separate `try` block syntax.
- `catch` is a binary operator: expression on the left, catch function on the right.
- The left operand is evaluated under an implicit try.
- If an exception is thrown and the catch function is defined for that exception object, the catch function is applied to that exception.
- Otherwise, the exception continues propagating.
- Exception behavior follows the .NET exception model.

### 8.9 Other documented operators
- `\` such-that / restriction operator.
- `in` binding/proposition scope operator.
- grammar-shift operators `>>>` and `<<<`.

## 9. Collections

### 9.1 Lists
- Ordered sequence values.
- List definitions can expand nondeterministic elements by backtracking.

### 9.2 Sets and multisets
- Sets are unique-element collections with definition-order semantics in docs.
- Multisets permit duplicates and can preserve generation order.

### 9.3 Tuples
- Positional product values.
- Optional components are documented by example.
- `tuple` and `tuples` operators provide explicit construction/type-set forms.

### 9.4 Records
- `record` defines structured named fields.
- `records` denotes sets of records.
- Field access uses property selection (`.`).

## 10. Classes, Entities, Enums

### 10.1 Classes
- Classes are inclusive sets over candidate sets.
- Equal payloads in same class are value-equal.
- Created with `class`, instantiated with `new`.
- Extended with `extend`.

### 10.2 Entities
- Entities are identity-based and exclusive (only constructed instances).
- Equal payloads do not imply equal entities.
- Escaping entities may require explicit stable identity fields (per docs).

### 10.3 Enums
- Enumerations are declared with `enums` syntax.
- Tagged variants with payloads are documented by examples.

## 11. This Binding

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

## 12. Nondeterminism

TODO: Choice, ranges, and set/list expansion interact via backtracking semantics.
TODO: Undefined values participate in operator-specific ways (notably choice and guarded logical operators).

### Unenumerable nondeterminism

TODO

### Enumerable bounded nondeterminism

TODO

### Enumerable unbounded nondeterminism

TODO

## 13. Exceptions

Exception behavior follows the .NET exception model. No explicit `try` block syntax exists; instead, exception handling is integrated via the binary `catch` operator and the unary prefix `throw` operator.

### 13.1 Throw operator

The `throw` operator is a unary prefix operator that raises (throws) an exception.

- Syntax: `throw expr`
- The operand expression must evaluate to an exception object.
- Execution of the current expression terminates, and control transfers to the nearest enclosing `catch` operator that can handle the exception.
- If no enclosing `catch` handles the exception, it propagates to the caller.

### 13.2 Catch operator

The `catch` operator is a binary operator that handles exceptions thrown by its left operand.

The left operand is an expression that is evaluated under an implicit try.

The right operand is a function that handles exceptions.

- Evaluation proceeds as follows:
  - The left operand is evaluated.
  - If no exception is thrown, the result of the left operand is the result of the entire `catch` expression.
  - If an exception `E` is thrown during evaluation of the left operand, **and** the catch function is defined for `E` then the catch function is invoked with `E` as its argument. The result of the `catch` expression is the result of the catch function applied to the exception `E`
  - If the catch function is not defined for the exception type of `E`, the exception `E` continues to propagate.

### 13.3 Catch function matching

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

### 13.4 Exception propagation and nesting

Multiple `catch` operators may be nested.

- An exception caught by an inner `catch` does not propagate to an outer `catch` unless the inner catch function re-throws (via `throw`).
- An exception not caught by a `catch` continues propagating outward.
- If no `catch` in the entire expression handles an exception, it propagates to the caller.

## 14. Operator Precedence and Associativity

TODO: precedence and associativity are intentionally left unspecified in this draft and will be defined later.

## 15. Normative Assumptions Used in This Draft

To resolve inconsistencies across reference pages, this draft assumes:
- `.` is one operator with context-sensitive declaration/selection behavior; `..` is member projection over elements.
- Set-to-identity-function promotion occurs only where a function is required by typing context.
- `-->` is defunct and should be treated as legacy spelling to be migrated to `^^`.

## 16. Open Clarifications Required

The following points need authoritative clarification before this draft can be finalized as a strict spec:
1. Exact operator precedence and associativity table.
2. The set of access modifiers. §5.1 lists `public`, `private`, `protected`, `local`; the implementation accepts `public`, `private`, `protected`, `internal`. One spelling must win.
3. Whether `primary` erases attributes on the *elements* of a set or list, or only on the collection itself. §2.3 specifies deep erasure; the interaction with infinite and lazily generated collections is unspecified.
4. Whether class promotion is class-preserving (widening the candidate while keeping the class) or class-shedding (discarding the class), and whether both exist. §2.3 assumes shedding is never implicit.
5. Surface forms documented elsewhere in this draft but absent from the lowering in §2.7, each of which needs either a lowering or removal: `?=` (§5.2), the mutually guarded `&&&` and `|||` (§8.2), and operator sections such as `(= rhs)` (§8.5).

## 17. Conformance Note

This document is a consolidated specification draft extracted from existing reference pages and normalized for consistency. Where source pages conflict or are incomplete, this document marks assumptions explicitly.
