# The Ting Programming Language

Ting is a logic programming language whose types are sets. One relational core carries the work that mainstream languages split across three separate mechanisms - a type system, a null/option/exception story, and a control-flow story. Programs describe relations between values; the language binds, constrains and searches over them.

> **Status: design stage.** There is no release and no installer. The specification is a live draft; the compiler implements parsing, binding, name resolution and partial evaluation, and stops there. No type checker, no constraint solver, no code generation, no runtime, no module system, no standard library.

**[Start with the introduction](docs/quick-introduction.md)**

## The idea, three ways

- **Sets are the types.** `int` is the set of 32-bit integers, and a set is an ordinary value. Refinement types, enumerations, records, unions and nominal types are all set constructions rather than separate language features.
- **Undefinedness is not a value.** A function with no result at some argument produces nothing - not `null`, not `None`. `void` is how that is written, and it cannot be bound, stored or compared. Definedness is observed only where you ask for it.
- **Nondeterminism is first class.** An expression may denote several values. Search is evaluation. Nondeterminism is contained by collecting it into sets, not by cutting the search tree.

They are one mechanism, and this is where that shows:

```ting
{ float x \ 2*x^2 + 10*x - 48 == 0 }
```

That is simultaneously a set (the reals satisfying the equation), a partial description (the member expression is undefined when there is no real root), and a search (the values `x` can assume, `3` and `-8`). The constraint is written once and serves as the type, the definedness condition and the algorithm.

## Repository layout

| Path | Contents |
| --- | --- |
| [`docs/quick-introduction.md`](docs/quick-introduction.md) | Introduction to the language, start here |
| [`docs/specification/`](docs/specification) | The language specification. Normative, and the most complete document in the repository. |
| [`docs/programmers-reference/`](docs/programmers-reference) | Feature-by-feature reference |
| [`docs/ting-primer.md`](docs/ting-primer.md) | Shorter tutorial-style primer |
| [`Compiler/`](Compiler) | The compiler, in F# on .NET 10. Parser built on FParsec. |

## Building and testing

Requires the .NET 10 SDK.

```
cd Compiler
dotnet test
```

The test suite includes focused binding and resolution assertions. Notebook tests regenerate the output in `Compiler/examples/notebook/diagnostics/` and verify that regeneration is idempotent; review the resulting diff for unintended changes. To regenerate notebook output separately:

```
dotnet run --project Examples.Tools
```

To compile an expression directly:

```
echo "let x = 21 in x * 2" | dotnet run --project TingCompilerCli
```

This runs the current compiler front end and prints its resolution status, scopes, identifier bindings, and diagnostics. It does not generate executable code.

## Compilation phases

1. Scope annotation records declarations and gives each undecided identifier the ordered stack of active scopes, including each frame's declarative or referential mode. Scope-defining expressions remain in the tree for subsequent analysis.
2. Resolution settles declarations, derives record structure from equality constraints in either direction, and rewrites references and property accesses. It repeats these steps with rebuilt constraints until no further progress is possible. Unknown inner scopes block lookup; they are not skipped in favor of outer scopes.

The front end reports success only when name resolution completes. A missing name produces a failure; a scope whose structure is unknown or not uniquely determined produces an incomplete result. Cycles without enough structural information remain unresolved rather than causing an endless loop. This is structural inference for name resolution, not a general expression unifier: arbitrary function reduction, set constraints, and unification of distinct record scopes still need implementation.

## Contributing

The language is being designed rather than maintained, so the most useful contribution is argument: read the [specification](docs/specification/ting-language-specification.md) and say where it is wrong, unclear, or trying to be two things at once. Issues and discussion are welcome.