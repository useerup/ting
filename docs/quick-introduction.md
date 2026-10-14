---
layout: default
title: Introduction to Ting
---

# Introduction to Ting

Ting is a logic programming language whose types are sets. One relational core carries the work that mainstream languages split across three separate mechanisms - a type system, a null/option/exception story, and a control-flow story. Programs describe relations between values; the language binds, constrains and searches over them. At this stage it is an experimental language.

A few tiny examples show the shape of the language. A set can be written directly as a refinement:

```ting
Even = { let int x \ x % 2 == 0 }
```

A dependent type can describe values that depend on another value:

```ting
DivisibleBy = int d => { let int n \ n % d == 0 }
```

A function can be defined only for some arguments, and simply produce no result for the rest:

```ting
Lookup = fn{ "a" -> 1, "b" -> 2 }
```

And an expression can *nondeterministically* denote several values at once:

```ting
MightBeOneOrTwoOrThree = 1 ; 2 ; 3
```

Nondeterminism can be controlled by collecting the nondeterministic values into a set. This example is the set of Fibonacci numbers:

```ting
{ let f = (a,b) => a;f(b,a+b) in f(0,1) }
```


## Status

**Ting is at the design stage.** There is no release, no installer, and no way to write a useful program in it today. Nothing on this page is available as a download.

What exists and works:

|                      |                                                                                                                        |
| -------------------- | ---------------------------------------------------------------------------------------------------------------------- |
| Specification        | [`specification/ting-language-specification.md`](specification/ting-language-specification.md), draft, actively edited |
| Compiler front end   | F# on .NET 10, parser built on FParsec                                                                                 |
| Pipeline implemented | parse -> bind (scope marking) -> resolve (name/member resolution) -> partial evaluation                                |
| Test suite           | 67 snapshot examples, all green                                                                                        |
| Editor support       | VS Code extension with a TextMate grammar, in [`ting/`](https://github.com/useerup/ting/tree/main/ting)                |

What does not exist yet:

- No type checker and no type inference. The set-theoretic type system is specified but not implemented.
- No constraint solver. The examples below that describe search *describe* it; nothing solves them.
- No code generation, no runtime, no REPL.
- No module system. `import` appears in older documents as a sketch; it is not implemented.
- No package manager, no standard library, no installers, no playground.

The evaluator handles deterministic arithmetic, string, boolean, tuple, record, lambda and projection expressions, and reports a value for those. Everything else compiles as far as name resolution and stops there.

Every Ting fence on this page has been run through the compiler front end and parses and resolves cleanly, unless it is explicitly marked **design sketch**. A design sketch is valid per the specification but depends on machinery that is not built yet.

## Who this is for

This is for people who are interested in language design itself: in set-theoretic type systems, in constraint and logic programming, in what a language looks like when undefinedness is a semantic property rather than a value. If you want to read a specification and argue with it, you are the audience.

This is not for people looking for a language to build something in. There is no something to build with, yet. It is also not an academic project - there are no papers behind it. It is one person's design work, worked out in public.

## Why Ting?

Take a mainstream statically typed language and count the mechanisms it uses to answer questions about values.

- There is a **type system**, which answers "what values can be here" - but only in a vocabulary of nominal classes and a fixed set of type constructors. Ask it for "an integer between 1 and 8" and it has no way to say so; you fall back to a runtime check.

- There is a separate **absence** mechanism - `null`, or `Option`, or `Result`, or exceptions, usually several at once - which answers "is there a value here at all". It is separate from the type system, and it forces you to represent absence as a value, which means wrapping and unwrapping it everywhere it flows.

- And there is a third, **control flow**, which answers "which value ended up here". It is imperative even in functional languages, and it is where the search that your problem actually requires gets hand-coded as loops and recursion.

These three answer variations on one question, and they do not compose with each other. Refinement in the type system does not help the absence mechanism. The absence mechanism does not help the search. So you write the same constraint three times: once as a type, once as a validity check, once as a loop condition.

Ting's wager is that one mechanism does all three. If types are sets, then "an integer between 1 and 8" is just a set (a subset to be precise). If a partial function is one that has no result at some arguments, then absence needs no representation - there is simply nothing there. And if an expression may denote several values, then search is what evaluation already does. The three sections below are the same idea seen from three sides.

## Sets are the types

**The problem.** In C# or TypeScript, `Age` is `int` and the rule that it cannot be negative lives in a constructor, a validation attribute, or a comment. The type system knows about `int`; it does not know about `Age`. Widening from "the type" to "the set of values I actually mean" requires leaving the type system.

**The idea.** A type in Ting *is* a set, and a set is an ordinary value. `int` is the set of 32-bit integers. Sets are built with the same operators as everything else.

```ting
public Even = { let int x \ x % 2 == 0 }
```

`\` is *such-that*. `x` is declared over `int` on its left, in the frame of `let`, and constrained on its right. `Even` is the set of even integers - and it is a value, which can be passed to a function, returned from one, or stored.

The same construction covers what other languages need distinct features for:

```ting
public (
    NonZero = let int x \ x != 0,
    Colour = { "red", "green", "blue" },
    Rank = class (1 ... 8),
    Person = records(string Name, int Age)
)
```

A refinement type, an enumeration, a nominal type over a bounded range, and a record type, all written as sets. `class` makes a set *nominal*: `Rank` accepts only values explicitly constructed as `Rank`, so it is distinct from any other class over `1 ... 8`.

Membership is an ordinary operator, and sets compose:

```ting
let Even = { let int x \ x % 2 == 0 } in 4 : Even
```

```ting
let Person = records(string Name, int Age) in { let Person p \ p.Age >= 18 }
```

The second is the set of adults, derived from the set of persons by the same `\` used to derive `Even` from `int`. There is no separate subtyping feature: it is set restriction.

**What it buys.** Types, generics, enums, unions, intersections, refinement types and the patterns you match against stop being separate features with separate syntax and separate limits. `&`, `|` and `!` on sets mean intersection, union and complement; `*` means product; `^^` builds the set of functions from one set to another. The type system is the language.

## Undefinedness is not a value

**The problem.** Rust and Haskell get this much righter than C# does - `Option` is far better than `null` - but they still answer "there is no result here" by producing a value that says so. That value has to be constructed, propagated, and destructured, and every function that might not have an answer changes its type, which changes the type of every caller. The absence is real; the wrapper is bookkeeping.

**The idea.** In Ting a function that has no result at some argument produces *nothing*. Not a null, not a `None`, not a sentinel. `void` is how you write that in source, and it is emphatically not a value:

> `void` is not a member of any set - not even of a set that contains everything - it cannot be bound to a name, held in a collection, or passed as an argument. In particular `void` cannot be compared to anything: `void = void`, `x == void` and `x != void` are not propositions that hold or fail, they are themselves undefined.

Partiality is therefore not a property of a function's type. It is a property of the function at an argument:

```ting
public Lookup = fn{ "a" -> 1, "b" -> 2 }
```

`Lookup` is a function on strings that is defined at two of them. Applying it to `"c"` yields no result. Its declared domain did not change, its type did not change, and no caller had to be rewritten to accommodate a wrapper.

Definedness is observed where you want to observe it, by operators that are built for it - `;;`, `?`, `;`, the guarded logical operators, and `catch`:

```ting
let Lookup = fn{ "a" -> 1, "b" -> 2 } in Lookup "c" ;; 0
```

`;;` is *alternative on undefined*: its left operand unless that is undefined, otherwise its right. It is the recovery point, and it is the only place the partiality had to be mentioned. The same operator handles arithmetic that has no answer:

```ting
let d = 0 in 10 / d ;; 0
```

The guarded logical operators make the same distinction inside propositions. `&` evaluates both operands and is undefined if either is; `&&` short-circuits, so a false left operand gives `false` even where the right operand is undefined; `&&&` guards both sides. Each has a normative truth table in the specification, with *undefined* as a row rather than as a third truth value - because it is the row for an operand that yields no value at all.

**What it buys.** No `null`, no `Option`, no `Result`, no checked exceptions, and no distinction between "this function is partial" and "this function returns a wrapper". A partial function is just a function. Recovery is local and explicit.

## Nondeterminism is first class

**The problem.** Prolog showed that relations are a good way to describe a problem, and then had to introduce the cut to make programs terminate - an operator with no declarative reading, which breaks the very property that made the relational description worth having. Everywhere else, search is not in the language at all: you hand-code it as loops and mutable accumulators, and the relation you were describing disappears into the mechanics.

**The idea.** An expression in Ting may denote more than one value. `;` is ordered choice:

```ting
1 ; 2 ; 3
```

That expression assumes `1` until backtracking forces it to assume `2`, and so on. Ranges are nondeterministic in the same way - `1 ... 8` is an expression that can assume any of eight values - and so is any set used where a value is expected.

This is what makes `\` work. Look again at:

```ting
public NonZero = let int x \ x != 0
```

`int` on the left is nondeterministic over every 32-bit integer; the right operand filters it. Set construction is where nondeterminism gets *collected* rather than propagated: a set definition gathers everything its element expressions can assume, so an unbounded choice inside a set becomes one ordinary set value outside it. That is Ting's answer to the problem the cut was invented for - you contain nondeterminism by collecting it, not by cutting the search tree.

Undefinedness and choice interact deliberately: `;` disregards an undefined operand and is undefined only when both are, so a branch with no result contributes no solution instead of failing the whole expression.

**What it buys.** Search is evaluation, not a library and not hand-written recursion. And because relations are symmetric, a relation can be run in more than one direction - which is where the three pillars turn out to be one.

## Where the three meet

The three sections above are not three features. They are one mechanism described three times, and this is the example that shows it:

```ting
let float x \ 2*x^2 + 10*x - 48 == 0
```

Read it as a **set**: `x` is declared over `float` and restricted by the proposition on the right. It is the set of reals satisfying the equation, exactly the same construction as `let int x \ x % 2 == 0`, with a harder predicate.

Read it as **definedness**: it is a partial description. For most coefficients there is no real root, and where there is none the expression yields nothing at all. It does not fail, throw, or return an empty wrapper.

Read it as **nondeterminism**: `float` is an expression that can assume any float, and the proposition filters it. Evaluating the expression means finding the values it can assume - `3` and `-8`. That is search, expressed as a relation rather than as an algorithm.

You did not write a solver, and you did not write three versions of the constraint. You wrote the constraint once, and it is simultaneously the type, the definedness condition, and the search.

The same expression becomes a function by abstracting the coefficients:

```ting
public Solve = (float a, float b, float c) => let float x \ a*x^2 + b*x + c == 0
```

**Design sketch.** Both compile as far as name resolution today. Actually producing `3` and `-8` needs the solver, which does not exist. The intended design is that the compiler supplies the machinery - binding, truth-valued constraints, nondeterministic search - while domain knowledge such as the quadratic formula arrives from libraries as axioms. See [`index.md`](index.md) for how that is meant to work.

## The ordinary parts

Everything above is the argument for the language. This section is so you can calibrate: these are the boring things, and they look how you would expect.

```ting
"Hello, world!"
```

A program is an expression, and this one evaluates to a string. There is no `main`, no entry-point ceremony, and no statement/expression distinction to learn.

```ting
let x = 21 in x * 2
```

`let ... in` binds locally. This evaluates to `42`.

```ting
public (
    Double = int x => x * 2,
    Sum = (int a, int b) => a + b
)
```

`=>` is the lambda. The left operand is a declarative pattern, so `int x` both names the parameter and gives its set. A publishing operator such as `public` can cover a group of declarations, which is how several names are introduced at once.

```ting
public Fact ?= fn{ 0 -> 1, int n -> n * Fact (n - 1) }
```

`fn{ ... }` is a function given by cases, tried in order. `?=` rather than `=` is what makes the binding recursive: it is declaration by unification, so `Fact` is in scope on the right.

```ting
public Fib ?= fn{ 0 -> 0, 1 -> 1, int n -> Fib(n-1) + Fib(n-2) }
```

Records are products with named fields, written with `@` separating the positional section from the named one:

```ting
let p = (@ Name = "Bob", Age = 42) in p.Age
```

Lists use square brackets, sets use braces, tuples use parentheses:

```ting
let xs = [1, 2, 3] in xs
```

## Frequently asked

|                         |                                                                                                                                                                         |
| ----------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Paradigm                | Logic programming at the core, unifying functional and object-oriented constructs                                                                                       |
| Statically typed        | By design, yes - types are sets. **Not implemented.**                                                                                                                   |
| Type inference          | Planned. Not implemented.                                                                                                                                               |
| Compiled or interpreted | Intended to compile, reducing expressions at compile time and emitting residual code. Today: a front end plus a partial evaluator.                                      |
| Memory management       | Undecided. Not specified.                                                                                                                                               |
| Error handling          | `catch` as a binary operator and `throw` as a prefix operator, over the .NET exception model. No `try` blocks. Separately, undefinedness is *not* an error - see above. |
| Nondeterminism          | In the language: `;`, ranges, set expansion, backtracking. Collected by set construction.                                                                               |
| Object orientation      | `class` for inclusive nominal sets, `entity` for identity-bearing values, members via `.` and `..`                                                                      |
| Operators               | None are primitive. `a + b` applies the function `_+_` to `(a, b)`. Operator names come from a scope available to the parser and cannot be shadowed.                    |
| Modules and imports     | Designed but unspecified; several parts of the specification are blocked on it. Not implemented.                                                                        |
| Standard library        | None.                                                                                                                                                                   |
| Package manager         | None.                                                                                                                                                                   |
| Interop                 | Exceptions follow the .NET model. Nothing further decided.                                                                                                              |
| Tooling                 | A VS Code extension with syntax highlighting. No language server, no debugger, no formatter.                                                                            |
| Self-hosting            | An aspiration, sketched in [`dogfood/compiler.ting`](https://github.com/useerup/ting/blob/main/dogfood/compiler.ting)                                                   |
| Licence and repository  | [github.com/useerup/ting](https://github.com/useerup/ting)                                                                                                              |

## Compared to other languages

**Prolog.** The shared ground is relations and backtracking. The difference is what tames the search: Prolog has the cut, which is operationally defined and destroys the declarative reading. Ting collects nondeterminism into sets instead, which is an ordinary value-level construction. Ting is also typed, and its types are the same sets.

**Haskell and ML.** Ting takes the functional core - functions as values, algebraic data, pattern matching - and replaces the type system with sets, which subsumes algebraic data types, refinement and dependent typing without needing them as separate features. The larger divergence is `Maybe`/`Either`: Ting has no absence values at all, because undefinedness is not represented.

**Verse.** The closest relative in intent - a functional logic language with choice and failure as core notions. The languages differ in how they write choice and how they scope it.

**F# and C#.** Ting's compiler is written in F#, and the influence shows in the syntax. But where F# has discriminated unions, options, active patterns and units of measure as four separate features, Ting intends all four to fall out of sets.

Ting is not trying to be faster, safer or more productive than any of these. It is trying to find out how much of a language collapses into one mechanism when types are sets.

## Where to go next

- [Ting Primer](ting-primer.md) - built-in sets, refinement, functions, worked examples
- [Surprising features](surprising%20features.md) - the parts that catch people out
- [Programmers Reference](programmers-reference/index.md) - feature by feature
- [Language Specification](specification/ting-language-specification.md) - normative, and the most complete document here
- [Documentation overview](index.md) - a longer tutorial-style walkthrough with the constraint examples
- [Worked problems](introduction/index.md) - ten programs the language is meant to be judged by, mostly unwritten

To run the compiler front end over your own snippets:

```
cd Compiler
echo "let x = 21 in x * 2" | dotnet run --project TingCompilerCli
```

It prints the resolved core expression, the scope tree, any diagnostics, and, for the expressions the partial evaluator handles, a value:

```
Compilation succeeded.

local[#0] (id(resolved:#1) ((id(resolved:#2) (id(decl:#3), 21)), (id(resolved:#4) (id(resolved:#3), 2))))

Scopes:
[0] NamesScope           #0
    names=[x<name>]

Value:
42
```

Note that a compilation unit is currently a single expression: several top-level declarations must be grouped under one publishing operator, as in the examples above. Separate top-level declarations wait on the module system.
