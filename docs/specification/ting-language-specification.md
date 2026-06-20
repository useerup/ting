# Ting Language Specification

> Note: This is a draft


## 1. Design Model

Ting is logic programming language. The logic oriented core allows it to unify the functional- and object-oriented paradigms.

## 2. Lexical Structure

### 2.1 Tokenization
- Longest-match tokenization applies.
- Example: `a<=b` tokenizes as `a`, `<=`, `b`.

### 2.2 Identifiers

Regular identifiers start with a Unicode letter or underscore and are followed by zero or more Unicode letters, digits, or underscores.

#### 2.2.1 Quoted identifiers

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

#### 2.2.2 Interpolated identifiers

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


### 2.3 Whitespace and indentation
- Grammar-shift operators can use indentation-sensitive forms.
- If shift operators end a line, subsequent indented lines are folded.

## 3. Literals

### 3.1 Strings

A string literal produces a value of type `string`, which is an immutable sequence of Unicode characters. Several forms exist, distinguished by their opening delimiter and optional prefix characters.

#### 3.1.1 Regular strings

A regular string is opened by `"` and terminated by the next unescaped `"` on the same line.

- The content may not contain an unescaped `"` or a newline character.
- Escape sequences (§3.1.2) are processed.
- The string value is the sequence of characters produced after escape processing.

Example: `"Hello, world!"`

#### 3.1.2 Escape sequences

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

#### 3.1.3 Verbatim strings

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

#### 3.1.4 Raw strings

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

#### 3.1.5 Interpolated strings

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

### 3.2 Numeric literals
- Numeric literals include integer and floating forms.
- Numeric separators are supported (example in syntax docs).

### 3.3 Collection literals
- List literal: `[e1, e2, ...]`
- Tuple literal: `(e1, e2, ...)`
- Set literal: `{e1, e2, ...}`
- Multiset literal: `{* e1, e2, ... *}`

## 4. Declarations and Binding

### 4.1 Scope-introducing operators
The reference documents identify these as declaration-forming contexts:
- Visibility and module scope operators: `public`, `private`, `protected`, `local`
- Aggregate scope operators: `record`, `records`, `tuple`, `tuples`
- Function scope operators: `fn`, lambda `=>`
- Local binder: `let`

### 4.2 Local binding
- Canonical form: `let pattern = expr in body`
- Recursive and unification-style examples use `?=`.

### 4.3 Lambda
- Form: `pattern => expression`
- Left side is declarative pattern context.
- Right side is referential expression context under the bound names.

### 4.4 Function cases via fn
- Case function form: `fn{ p1 -> r1, p2 -> r2, ... }`
- Supports typed patterns and guarded patterns shown in examples.

## 5. Types, Sets, and Membership

Ting models types as sets.

### 5.1 Built-in scalar sets
- `byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`
- `float`, `double`, `decimal`
- `char`, `string`, `bool`

### 5.2 Meta sets
- `set`: set of all sets (not generally enumerable)
- `func`: set of all functions (not generally enumerable)

### 5.3 Membership operators
- `:` member-of
- `!:` not member-of
- `::` collection-of
- `:::` sequence-of

### 5.4 Set comparison relations
- `<`, `<=`, `==`, `>=`, `>` are subset/superset/equality relations on sets.

### 5.5 Set construction
- Extensional: `{1,2,3}`
- Intensional/restricted: `{ x \ predicate(x) }`
- Product sets use `*` between set expressions.

## 6. Functions

### 6.1 Functions as relations
Functions map argument values to result values. Function values can be defined by lambdas or case expressions (`fn`).

### 6.2 Sets as identity functions
TODO: describe an implicit promotion from set values to identity functions when a function is expected.

### 6.3 Function composition and application
- Composition operators include `>>` and `<<`.
- Application operators include `|>` and `<|`.
- Juxtaposition application (`f x`) is supported.

## 7. Operators

### 7.1 Arithmetic
- `+`, `-`, `*`, `/`, `^`, `%`, `/%`
- `*` is numeric multiplication or set Cartesian product depending on operand sets.
- `/` includes tuple-set division behavior as documented examples.

### 7.2 Logical operators with undefined-aware variants
- Strict: `&`, `|`, `!`
- Guarded: `&&`, `||`
- Mutually guarded: `&&&`, `|||`

### 7.3 Choice and alternatives
- `;` introduces ordered nondeterministic choice.
- `;;` provides alternative-on-undefined behavior.

### 7.4 Identity and unification
- `=` is a binary relational operator.
- `=` compares two objects structurally (value and attributes) and returns a truth value (`true` or `false`).
- In positions that require truth (for example, restrictions/guards), `a = b` must evaluate to `true`.

### 7.5 Restriction and partial binary application
- `?` is a binary restriction operator.
- `left ? predicate` restricts `left` to values that satisfy `predicate`.
- Any binary operator may be used in prefix position with one operand to form a function by partial application.
- Example shape: `(= rhs)` is a function that accepts `lhs` and evaluates `lhs = rhs`.

### 7.6 Range operators
- `...`, `<...`, `...<`, `<...<`
- Used for inclusive/exclusive interval generation and set/list expansion.

### 7.7 Property and projection
Ting distinguishes between two property operators:

#### 7.7.1 `.` property operator
- In **referential context**, `whole.part` references the property `part` of `whole`.
- In **declarative context**, `whole.part` declares a property `part` directly on `whole`.
- The right operand is always declarative. It declares the properties that is selected from `whole`.
- A referential `.` expression used in prefix position evaluates to a function of the form `whole => whole.part`.

#### 7.7.2 `..` member property operator
- In **referential context**, `some..prop` references the property `prop` of each member of the set `some` and yields a function from members of `some` to their `prop` property.
- In **declarative context**, `some..prop` declares the property `prop` on every member of the set `some`.
- The left operand must be a set.
- The right operand is always declarative.
- A referential `..` expression used in prefix position evaluates to a function from members of the left-hand set to the corresponding property on each member.

### 7.8 Exception operators
- `throw` raises exceptions.
- `catch` handles exceptions.
- There is no separate `try` block syntax.
- `catch` is a binary operator: expression on the left, catch function on the right.
- The left operand is evaluated under an implicit try.
- If an exception is thrown and the catch function is defined for that exception object, the catch function is applied to that exception.
- Otherwise, the exception continues propagating.
- Exception behavior follows the .NET exception model.

### 7.9 Other documented operators
- `\` such-that / restriction operator.
- `in` binding/proposition scope operator.
- grammar-shift operators `>>>` and `<<<`.

## 8. Collections

### 8.1 Lists
- Ordered sequence values.
- List definitions can expand nondeterministic elements by backtracking.

### 8.2 Sets and multisets
- Sets are unique-element collections with definition-order semantics in docs.
- Multisets permit duplicates and can preserve generation order.

### 8.3 Tuples
- Positional product values.
- Optional components are documented by example.
- `tuple` and `tuples` operators provide explicit construction/type-set forms.

### 8.4 Records
- `record` defines structured named fields.
- `records` denotes sets of records.
- Field access uses property selection (`.`).

## 9. Classes, Entities, Enums

### 9.1 Classes
- Classes are inclusive sets over candidate sets.
- Equal payloads in same class are value-equal.
- Created with `class`, instantiated with `new`.
- Extended with `extend`.

### 9.2 Entities
- Entities are identity-based and exclusive (only constructed instances).
- Equal payloads do not imply equal entities.
- Escaping entities may require explicit stable identity fields (per docs).

### 9.3 Enums
- Enumerations are declared with `enums` syntax.
- Tagged variants with payloads are documented by examples.

## 10. This Binding

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

## 11. Nondeterminism

TODO: Choice, ranges, and set/list expansion interact via backtracking semantics.
TODO: Undefined values participate in operator-specific ways (notably choice and guarded logical operators).

### Unenumerable nondeterminism

TODO

### Enumerable bounded nondeterminism

TODO

### Enumerable unbounded nondeterminism

TODO

## 12. Exceptions

Exception behavior follows the .NET exception model. No explicit `try` block syntax exists; instead, exception handling is integrated via the binary `catch` operator and the unary prefix `throw` operator.

### 12.1 Throw operator

The `throw` operator is a unary prefix operator that raises (throws) an exception.

- Syntax: `throw expr`
- The operand expression must evaluate to an exception object.
- Execution of the current expression terminates, and control transfers to the nearest enclosing `catch` operator that can handle the exception.
- If no enclosing `catch` handles the exception, it propagates to the caller.

### 12.2 Catch operator

The `catch` operator is a binary operator that handles exceptions thrown by its left operand.

The left operand is an expression that is evaluated under an implicit try.

The right operand is a function that handles exceptions.

- Evaluation proceeds as follows:
  - The left operand is evaluated.
  - If no exception is thrown, the result of the left operand is the result of the entire `catch` expression.
  - If an exception `E` is thrown during evaluation of the left operand, **and** the catch function is defined for `E` then the catch function is invoked with `E` as its argument. The result of the `catch` expression is the result of the catch function applied to the exception `E`
  - If the catch function is not defined for the exception type of `E`, the exception `E` continues to propagate.

### 12.3 Catch function matching

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

### 12.4 Exception propagation and nesting

Multiple `catch` operators may be nested.

- An exception caught by an inner `catch` does not propagate to an outer `catch` unless the inner catch function re-throws (via `throw`).
- An exception not caught by a `catch` continues propagating outward.
- If no `catch` in the entire expression handles an exception, it propagates to the caller.

## 13. Operator Precedence and Associativity

TODO: precedence and associativity are intentionally left unspecified in this draft and will be defined later.

## 14. Normative Assumptions Used in This Draft

To resolve inconsistencies across reference pages, this draft assumes:
- `.` is one operator with context-sensitive declaration/selection behavior; `..` is member projection over elements.
- Set-to-identity-function promotion occurs only where a function is required by typing context.
- `-->` is defunct and should be treated as legacy spelling to be migrated to `^^`.

## 15. Open Clarifications Required

The following points need authoritative clarification before this draft can be finalized as a strict spec:
1. Exact operator precedence and associativity table.

## 16. Conformance Note

This document is a consolidated specification draft extracted from existing reference pages and normalized for consistency. Where source pages conflict or are incomplete, this document marks assumptions explicitly.
