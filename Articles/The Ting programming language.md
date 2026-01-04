# Ting Programming Guide

## Introduction

Ting is a modern programming language designed for clarity, expressiveness, and type safety.  
Built on the foundations of set theory, Ting unifies logical, functional, and object-oriented paradigms, allowing you to model complex systems with mathematical precision and expressive code.  
Its very strong type system enforces correctness and safety, making it ideal for applications where reliability and robustness are essential.  
Ting encourages clear thinking and structured problem solving, leveraging sets, relations, and functions as core abstractions.  
This guide introduces the core concepts, syntax, and features of Ting, helping you get started with writing Ting programs.


## Types

Unlike some other languages that you may know, a *set* in Ting is **not** a data structure, it is a *type*.

Ting has a number of built-in sets:

| Set     | Description                                                                                        |
| ------- | -------------------------------------------------------------------------------------------------- |
| sbyte   | 8-bit signed integer (`-128` to `127`). Useful for small ranges and memory-efficient storage.      |
| short   | 16-bit signed integer (`-32,768` to `32,767`). Used for moderate integer ranges.                   |
| int     | 32-bit signed integer (`-2,147,483,648` to `2,147,483,647`). The default integer type.             |
| long    | 64-bit signed integer (`-9,223,372,036,854,775,808` to `9,223,372,036,854,775,807`). Large values. |
| byte    | 8-bit unsigned integer (`0` to `255`). For raw data, buffers, and small counters.                  |
| ushort  | 16-bit unsigned integer (`0` to `65,535`). For non-negative moderate ranges.                       |
| uint    | 32-bit unsigned integer (`0` to `4,294,967,295`). For large non-negative values.                   |
| ulong   | 64-bit unsigned integer (`0` to `18,446,744,073,709,551,615`). For very large non-negative values. |
| float   | 32-bit IEEE 754 floating-point number. Approximate real numbers, single precision.                 |
| double  | 64-bit IEEE 754 floating-point number. Approximate real numbers, double precision.                 |
| decimal | 128-bit decimal number. High-precision base-10 arithmetic, ideal for financial calculations.       |
| string  | Sequence of Unicode characters. Used for text, supports internationalization.                      |
| char    | Single Unicode character. Represents a single symbol or letter.                                    |
| bool    | Boolean value: `true` or `false`. Used for logical conditions and control flow.                    |

Each set is a mathematical set of values, not a container or collection.  

### Refinement types

A refinement type is a type that is defined by restricting an existing set with a logical predicate.  
This allows you to specify more precise constraints on values, beyond their basic set membership.

For example, you can define a type for positive integers as a refinement of `int`:

```ting
EvenInts = int ?? x=>x%2==0
```

Here, `EvenInts` consists of all `int` values that are divisible by 2
Refinement types enable safer and more expressive code by allowing you to encode invariants and domain-specific rules directly in the type system. 

They are especially useful for catching errors early and documenting intent in APIs and data models. If a function in Ting is not defined for a specific value of set of values, the function should *restrict* its parameter(s) to those values for which it is defined.

For example, `Math.Log` and `Math.Log10` are defined only for posaitive `float`s and `double`s.


## Basic Syntax

Ting uses a clean and readable syntax. Here’s a simple example:

```ting
let x = 42
let message = "Hello, Ting!"
```

## Data Types

Ting supports several basic data types:

- `Int`: Integer numbers
- `Float`: Floating-point numbers
- `Bool`: Boolean values (`true` or `false`)
- `String`: Text

Example:

```ting
let age: Int = 30
let pi: Float = 3.14
let isActive: Bool = true
let name: String = "Alice"
```

## Control Flow

Ting provides standard control flow constructs:

### If Statements

```ting
if age > 18 {
    print("Adult")
} else {
    print("Minor")
}
```

### Pattern Matching

Pattern matching is a powerful feature in Ting:

```ting
match value {
    0 => print("Zero")
    1 => print("One")
    _ => print("Other")
}
```

## Functions

Functions are first-class citizens in Ting:

```ting
fun add(a: Int, b: Int): Int {
    return a + b
}
```

## Example Program

```ting
fun greet(name: String) {
    print("Hello, " + name + "!")
}

greet("World")
```

---

Would you like to expand on any section, add advanced topics, or include more examples?