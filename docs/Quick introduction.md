# Ting Programming Language - Quick Introduction

## What is Ting?

Ting is a modern, expressive programming language designed to combine powerful logical reasoning with object-oriented flexibility. It features a sophisticated set-based type system while maintaining readability and productivity.

## Key Features

- **Set-based Type System** - Types are treated as mathematical sets, enabling powerful type operations and clear semantics
- **Pure Logic Programming** - True logical programming without cuts or other impure features, preserving declarative reasoning
- **Object-orientation** - Structured programming with elegant object models and inheritance
- **Advanced Type System** - First class types, refinement types, and dependent types for expressing complex constraints
- **Type Inference** - Strong static typing without unnecessary verbosity
- **Declarative Syntax** - Clean, readable code focused on what to compute rather than how

## Installation

```bash
# macOS or Linux
curl -sSL https://get.ting-lang.org | sh

# Windows
winget install TingLang
```

## Your First Ting Program

Create a file named `hello.ting`:

```ting
"Hello, World!"
```

Run it with:

```bash
ting run hello.ting
```

## Basic Syntax

### Variables and Sets

```ting
let x: Integer = 5;                 // Explicit typing
let names = {"Alice", "Bob"};       // Set literal
let even_numbers: Set[Integer] = {n | n in 0..100, n % 2 == 0};  // Set comprehension
```

### Logic Programming

```ting
// Define logical relations
relation parent(Person, Person);
relation ancestor(Person, Person);

// Rules
rule ancestor(X, Y) :- parent(X, Y);
rule ancestor(X, Z) :- parent(X, Y), ancestor(Y, Z);

// Facts
fact parent("Alice", "Bob");
fact parent("Bob", "Charlie");

// Queries
query ancestor("Alice", X);  // Returns {"Bob", "Charlie"}
```

### Types as First-Class Citizens

```ting
// Type definition
Person = {.
  Name: string,
  Age: int ?? >= 0  // Refinement type
.}

// Dependent types
Vector = int n -> Array.Of float n;
Vector.Concat = ((var Vector n1) v1, (var Vector n2) v2) -> Vector (n1+n2) 

// Type operations
Adult = Person ?? .(Age >= 18)
Child = Person - Adult
```

### Object-Orientation

```ting
class Shape {
  method area(): Float;
}

class Circle extends Shape {
  let radius: Float;
  
  constructor(r: Float) {
    radius = r;
  }
  
  method area(): Float {
    return pi * radius * radius;
  }
}

let c = Circle(5.0);
println(c.area());
```

## Control Flow

```ting
// Pattern matching with type refinement
match shape {
  case c: Circle => println("Circle with radius: {c.radius}");
  case r: Rectangle => println("Rectangle: {r.width} x {r.height}");
  case _ => println("Unknown shape");
}

// Logic-based conditionals
when x in NaturalNumbers & {n | n % 2 == 0} do {
  println("{x} is an even natural number");
}

// Iteration over sets
for x in {1, 2, 3, 4, 5} {
  println(x * x);
}
```

## Advanced Features

```ting
// Higher-order relations
relation map(relation(A) -> B, Set[A], Set[B]);

// Type-level functions
type Matrix[m: Natural, n: Natural] = Array[Array[Float, n], m];

// Refinement types with constraints
type PositiveInteger = {n: Integer | n > 0};
type SortedArray[T: Ordered] = {arr: Array[T] | ∀i,j. i < j → arr[i] ≤ arr[j]};
```

## Error Handling

```ting
// Results with type refinement
result divide(a: Integer, b: Integer): Integer {
  require b != 0 else "Division by zero";
  
  return a / b;
}

// Usage
match divide(10, 2) {
  case Ok(result) => println("Result: {result}");
  case Error(msg) => println("Error: {msg}");
}
```

## Next Steps

- Explore the [full documentation](https://docs.ting-lang.org)
- Try the [interactive playground](https://play.ting-lang.org)
- Join the [community forum](https://forum.ting-lang.org)

Happy logical programming with Ting!


useerup: paste in current editor

GitHub Copilot: # Ting Programming Language - Quick Introduction

## What is Ting?

Ting is a modern, expressive programming language designed to combine powerful logical reasoning with object-oriented flexibility. It features a sophisticated set-based type system while maintaining readability and productivity.

## Key Features

- **Set-based Type System** - Types are treated as mathematical sets, enabling powerful type operations and clear semantics
- **Pure Logic Programming** - True logical programming without cuts or other impure features, preserving declarative reasoning
- **Object-orientation** - Structured programming with elegant object models and inheritance
- **Advanced Type System** - First class types, refinement types, and dependent types for expressing complex constraints
- **Type Inference** - Strong static typing without unnecessary verbosity
- **Declarative Syntax** - Clean, readable code focused on what to compute rather than how

## Installation

```bash
# macOS or Linux
curl -sSL https://get.ting-lang.org | sh

# Windows
winget install TingLang
```

## Your First Ting Program

Create a file named `hello.ting`:

```ting
program main {
  println("Hello, World!");
}
```

Run it with:

```bash
ting run hello.ting
```

## Basic Syntax

### Variables and Sets

```ting
let x: Integer = 5;                 // Explicit typing
let names = {"Alice", "Bob"};       // Set literal
let even_numbers: Set[Integer] = {n | n in 0..100, n % 2 == 0};  // Set comprehension
```

### Logic Programming

```ting
// Define logical relations
relation parent(Person, Person);
relation ancestor(Person, Person);

// Rules
rule ancestor(X, Y) :- parent(X, Y);
rule ancestor(X, Z) :- parent(X, Y), ancestor(Y, Z);

// Facts
fact parent("Alice", "Bob");
fact parent("Bob", "Charlie");

// Queries
query ancestor("Alice", X);  // Returns {"Bob", "Charlie"}
```

### Types as First-Class Citizens

```ting
// Type definition
type Person = {
  name: String,
  age: Integer & {n | n >= 0}  // Refinement type
};

// Dependent types
type Vector[n: Natural] = Array[Float, n];

// Type operations
type Adult = {p: Person | p.age >= 18};
type Child = Person - Adult;  // Set difference
```

### Object-Orientation

```ting
class Shape {
  method area(): Float;
}

class Circle extends Shape {
  let radius: Float;
  
  constructor(r: Float) {
    radius = r;
  }
  
  method area(): Float {
    return pi * radius * radius;
  }
}

let c = Circle(5.0);
println(c.area());
```

## Control Flow

```ting
// Pattern matching with type refinement
match shape {
  case c: Circle => println("Circle with radius: {c.radius}");
  case r: Rectangle => println("Rectangle: {r.width} x {r.height}");
  case _ => println("Unknown shape");
}

// Logic-based conditionals
when x in NaturalNumbers & {n | n % 2 == 0} do {
  println("{x} is an even natural number");
}

// Iteration over sets
for x in {1, 2, 3, 4, 5} {
  println(x * x);
}
```

## Advanced Features

```ting
// Higher-order relations
relation map(relation(A) -> B, Set[A], Set[B]);

// Type-level functions
type Matrix[m: Natural, n: Natural] = Array[Array[Float, n], m];

// Refinement types with constraints
type PositiveInteger = {n: Integer | n > 0};
type SortedArray[T: Ordered] = {arr: Array[T] | ∀i,j. i < j → arr[i] ≤ arr[j]};
```

## Error Handling

```ting
// Results with type refinement
result divide(a: Integer, b: Integer): Integer {
  require b != 0 else "Division by zero";
  
  return a / b;
}

// Usage
match divide(10, 2) {
  case Ok(result) => println("Result: {result}");
  case Error(msg) => println("Error: {msg}");
}
```

## Next Steps

- Explore the [full documentation](https://docs.ting-lang.org)
- Try the [interactive playground](https://play.ting-lang.org)
- Join the [community forum](https://forum.ting-lang.org)

Happy logical programming with Ting!