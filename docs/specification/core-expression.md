# The Core Expression

Ting's semantics are defined over a small internal form called the **core expression**. Surface syntax, operators and declaration forms are all lowered into it before any meaning is assigned, so the core expression — not the surface grammar — is the normative object of this specification. Later sections describe surface constructs by stating what they lower to.

Two properties explain why the core is as small as it is.

**There are no operators in the core.** Every infix and prefix operator is surface syntax for the application of an ordinary function. `a + b` applies the function named `_+_` to the tuple `(a, b)`; `x => body` applies `_=>_`; `!p` applies `!_`. The core has one application form and no operator forms at all. Operator functions live in a scope fixed by the grammar, so they are never looked up lexically and can never be shadowed by a user declaration.

**There is no declaration form.** Declaring and referencing are not different constructs. They are the same identifier occurrence, interpreted against a different kind of scope (§2.4).

##  Forms

### Application

`apply f a` applies `f` to `a`. Juxtaposition (`f x`), every infix operator and every prefix operator lower to this form. An infix operator applies its function to a two-element tuple.

### Aggregates

- `tuple (e₁, …, eₙ)` — positional product.
- `set {e₁, …, eₙ}` — set.
- `list [e₁, …, eₙ]` — list.
- `string` — a sequence of literal text segments and embedded expressions (an uninterpolated string is the degenerate case of a single literal segment).

The surface forms `tuple E`, `tuples E` and `records E` contribute no core form of their own: `tuple E` lowers to `E`, `tuples E` to a single-element set, and `records E` to a single-element set containing a record.

### Literal

A primitive term: integer, floating-point, decimal, boolean or string.

### Identifier

An identifier occurrence together with its **binding role** (§2.4). A name is not necessarily a fixed string — an interpolated identifier's name is itself a sequence of text segments and embedded expressions, and resolution compares names after those expressions are evaluated.

### `this`

The `this` keyword. It carries a binding role exactly as an identifier does, and is declarative or referential by the same rule.

### Scope introduction

- `local S e` — evaluate `e` under a new local scope `S`. `let E` lowers to this, as do the local-scope operators `=>`, `\` and `where`.
- `scoped S e` — evaluate `e` under an explicitly supplied scope. `./` and `../` lower to this.
- `publish A S e` — declare the names introduced by `e` in scope `S` with access modifier `A`. `public`, `private`, `protected` and `internal` lower to this.
- `record S e` — evaluate `e` as a record scope; every name `e` declares becomes a field of the record.

Note that the right-referential operators (`=`, `in`, `:`, `<`, `==`, …) introduce **no** scope. They only decide how their operands are interpreted against the scopes already in force.

### Projection

- `project r m` — `r.m`. Selects or declares member `m` on `r` itself.
- `projectMembers r m` — `r..m`. Selects or declares member `m` on every member of the set `r`.

In both, the receiver is referential and the member is interpreted against the receiver, never lexically (§2.5).

### `void`

The value of an expression that yields nothing.

### `unknown`

The placeholder value of a name that has been declared but whose defining expression is not yet known. It is not writable in source; it exists so that a declaration is well-formed before its definition has been analyzed.

## Semantic primitives

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

## Value layers, erasure, and the two equalities

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

## Binding roles

Every identifier and `this` occurrence carries one of three roles:

- **Declarative** — the occurrence introduces a name. It carries a declaration identity, and that identity is recorded in *every* declarative scope covering the occurrence. An occurrence covered by more than one declarative scope declares the name in all of them and unifies the results.
- **Referential** — the occurrence refers to a name declared elsewhere. It carries the ordered list of candidate scopes, nearest enclosing first.
- **Resolved** — a referential occurrence that name resolution has paired with a specific declaration identity.

An occurrence is declarative if any declarative scope covers it, and referential otherwise. Nothing in the identifier itself decides this; only the scopes in force at that point do.

## Scopes

A scope maps declaration identities to entries. Each entry records the name, the expression the name is bound to (`unknown` until known), and optionally a **host**.

There are three kinds of scope:

- **Local scope** — a plain name table, introduced by `local`, `publish` and `record`.
- **Expression scope** — the declarations of a given expression, made available as a scope (`./`).
- **Expression member scope** — the declarations of the *members* of a given expression, made available as a scope (`../`).

A declaration made through a projection is **hosted** rather than lexical:

- **Direct host** — `int.Zero = 0` hosts `Zero` on `int` itself.
- **Member host** — `int..Double = …` hosts `Double` on each member of `int`.

Hosted names are invisible to bare lexical lookup. They are reachable only through projection on a receiver matching the host.

## Resolution

A referential occurrence is resolved in this order:

1. **Lexical** — search the candidate scopes, nearest first, skipping hosted names.
2. **Member** — for `project`/`projectMembers`, resolve the member against the receiver: first as a field of the receiver's record scope, then as a hosted name whose host matches the receiver (a direct host matches the receiver itself; a member host matches the receiver's set).
3. **Implicit `this`** — if a covering scope binds `this`, resolve the name as a member of the set `this` belongs to.

If none succeeds, an unresolved-name diagnostic is reported.

Member resolution is **structural, not lexical**. Members are never found by lexical search, and a member whose receiver structure is not yet determined is deferred rather than falling back to a lexical match.

## Surface forms and their lowering

This section indexes the surface syntax against the core. It covers the constructs the grammar currently accepts; a handful of operators appear in reference material without yet having a lowering, and are listed in §16.

Two things are recorded for each construct. **Core** is the form of §2.1 or §2.2 it produces. **Binding** is how it affects the binding role (§2.4) of its operands, using two terms:

- **Ambient** — the operand inherits whatever context surrounds the construct. It therefore *declares* where a declarative scope covers it and *refers* where none does. Ambient is the default; it is what makes a construct binding-neutral.
- **Referential** — the operand is a use-site regardless of the surrounding context. A name written there is always read, never introduced.

### Terms

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

### Application

| Surface | Core | Binding |
|---|---|---|
| `f x` | `apply f x` | `f` referential, `x` ambient |
| `⊕ x` | `apply ⊕_ x` | `x` ambient |
| `x ⊕ y` | `apply _⊕_ (x, y)` | per operator class, below |

Juxtaposition makes the callee referential and leaves the argument ambient. This is why `let float a = 42` declares `a`: `float a` reads `float` and, being under a declarative scope, declares `a`.

### Operator binding classes

Operators are ordinary functions (§2), but they differ in how they interpret their operands. There are four classes.

**Neutral** — both operands ambient; the operator introduces no scope and forces no role:

`else`, `>>`, `<<`, `|>`, `<|`, `|`, `||`, `|||`, `&`, `&&`, `&&&`, `...`, `...<`, `<...`, `<...<`, `;`, `;;`, `^^`, `**`, `+`, `-`, `*`, `/`, `%`, `/%`, `^`, and the prefix operators `!`, `fn`, `class`.

**Right-referential** — left operand ambient, right operand referential; no scope introduced:

`=`, `?=`, `?=`, `in`, `then`, `->`, `catch`, `?`, `??`, `:`, `::`, `:::`, `<`, `<=`, `>=`, `>`, `==`, `!=`, `!:`.

This class is what gives `x = 5` its asymmetry. The right side is a value to be read; the left side inherits the surrounding context and so introduces `x` when a declarative scope covers it. Nothing about `=` itself declares anything.

**Left-declarative** — introduces a local scope `S`; the left operand declares into `S` and the right operand refers to it. `x ⊕ y` lowers to `local S (apply _⊕_ (x, y))`:

`=>` (lambda parameter), `\` (comprehension binder).

**Right-declarative** — the mirror image: `S` is introduced, the right operand declares into it, the left refers to it:

`where`.

### Scope-introducing and structural forms

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
| `throw E` | `throw E` | `E` ambient |

`primary` and `candidate` leave their operand ambient, so `primary a = primary b` still declares `a` where a declarative scope covers it. The declaration is simply weaker: it constrains `a` only up to attribute erasure.

### Relations

`=` lowers directly to `equals`. Every other relation erases attributes first (§2.3):

    a = b       lowers to      equals a b
    a == b      lowers to      equals (primary a) (primary b)
    a < b       lowers to      apply _<_ (primary a, primary b)

Precedence and associativity are not part of lowering and are specified separately (§14).
