## Sets

A Ting *set* is a collection of unique values. As such, Ting sets are closely modeled after the sets you know from mathematics. 

A set may be defined by listing its elements (extensional set definition) and/or by describing some rule or expression which defines which element are members of the set.



### Materialized set

A *materialized set* is one where all the members are known and identified.


### Built-in sets

The language ships with several built-in sets, and you can define new sets as you please. A number of function and operators also return sets, perhaps by combining other sets.

| Set       | .NET type        | Description                                         |
| --------- | ---------------- | --------------------------------------------------- |
| `byte`    | `System.Byte`    | 8-bit unsigned integers.                            |
| `short`   | `System.Int16`   | 16-bit signed integers.                             |
| `int`     | `System.Int32`   | 32-bit signed integers.                             |
| `long`    | `System.Int64`   | 64-bit signed integers.                             |
| `sbyte`   | `System.SByte`   | 8-bit signed integers.                              |
| `ushort`  | `System.UInt16`  | 16-bit unsigned integers.                           |
| `uint`    | `System.UInt32`  | 32-bit unsigned integers.                           |
| `ulong`   | `System.UInt64`  | 64-bit unsigned integers.                           |
| `float`   | `System.Single`  | Single-precision floating-point numbers.            |
| `double`  | `System.Double`  | Double-precision floating-point numbers.            |
| `decimal` | `System.Decimal` | Fixed-point decimal numbers for precise arithmetic. |
| `char`    | `System.Char`    | Single Unicode characters.                          |
| `string`  | `System.String`  | Immutable sequences of characters.                  |
| `bool`    | `System.Boolean` | Logical truth values `true` or `false`.             |
| `set`     | `Ting.Sets`      | The set of all sets                                 |
| `func`    | `Ting.Functions` | The set of all functions                            |


#### The `bool` set

#### The `sbyte`, `short`, `int`, `long` sets of signed integers

#### The `byte`, `ushort`, `uint`, `ulong` sets of unsigned integers

#### The `float` and `double` sets of floating-point numbers

#### The `decimal` set of all decimal numbers

#### The `func` set of all functions

#### The `set` set of all sets


In addition these types from .NET `System` namespace are available:





### Set operators

Several operators accept a set as one or both operands:

- `:` (*member-of*) returns a `bool` indicating whether the left operand is a member of the right-hand set.
- `::` (*collection-of*) returns a `bool` indicating whether the left operand is a collection whose elements are all members of the right-hand set.
- `:::` (*sequence-of*) returns a `bool` indicating whether the left operand is an ordered sequence of members drawn from the right-hand set.
- `<` and `<=` return `true` when the left operand is a proper subset or subset, respectively, of the right-hand set.
- `>` and `>=` return `true` when the left operand is a proper superset or superset, respectively, of the right-hand set.
- `==` returns true when two sets contains the same elements.



### Set definitions

One way to create a set is through a *set definition*. A set definition consists of a comma-separated list of expressions surrounded by `{` and `}`—mirroring the mathematical notation for sets.

Examples:

    { }                                 // Empty

    { 1, 2, 3 }                         // Extensional

    { x \ x:int && x >= 0 }             // Intensional

    { 1, "abc", Math.Pi, {39, 42} }     // Heterogenous set of an integer, string, double and a set

Each expression in the list may itself be [nondeterministic](Nondeterminism.md). The set definition expands ("unwinds") those possibilities to include every resulting value, which is why the construct works equally well for both *extensional* and *intensional* style definitions familiar from mathematics.

    

    



    { x \ IsOrbitingSun(x) }

    IsOrbitingSun = Mercury;Venus;Earth;Mars;Jupiter;Saturn;Uranus;Neptune --> true



### Set-valued expressions

Some expressions evaluate to sets even though they are not written with braces.

Subsets

    int ?? >= 0         // The set of 32-bit integers greater than or equal to zero

Product sets

    int*string




### Ordered sets

A set definition returns an ordered set by default. The default order is the order of definition. The set `{ 1, 4, 2, 8, 5, 7 }` is thus ordered **not** numerically but rather in that sequence.

An ordered set has the following extension mathods:

- `Following`
- `FollowingCyclic`
- `Previous`
- `PreviousCyclic`


### Enumerable sets

An enumerable set is a set whose members can be enumerated


### Infinite sets



