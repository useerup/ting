# Introduction to the Ting programming language

Ting is a logic, functional, and object-oriented programming language. The syntax simple and should be familiar to anyone with some experience with Haskell, or ML-languages (SML, OCaml, F#) and math *set notation*.


## Quick look


### Structural typing

Sets are structural types: A set includes any value/object that satisfies the set condition. In Ting the following are valid sets:

    EvenNumbers = { int x \ x%2==0 }
    Fruits = { "apple", "banana", "lemon", "orange" }
    OneToFive = { 1, 2, 3, 4, 5 }

    Hours = { 0...23 }
    Minutes = { 0...59 }

The number `2` is a member of `EvenNumbers`, `OneToFive`, `Hours` and `Minutes`. It is not a member of `Fruits`.

Sets can be sets of simple values, tuples, records, ordered pairs, functions or other sets.

When used as a function, a set is its own *identity function*. A set is used as a function when it is used where a function is expected, for instance in a function application:

    int a

This expression restricts `a` to be a member of `int`, and the value of the expression is the value of `a`.

    EvenNumber even     
    Fruit fruit

If a contradiction is found during compilation it is a compilation error. This applies to memberships of sets as well. If a set is applied (as a function) to a value that is not a member of the set, the expression is undefined:

    OneToFive 6     // compilation error: impossible


### Nominal typing

*Classes* are the nominal types of Ting. Unlike some other programming languages, the instances of a class are not inherently structured in any specific way. A class can be a class of tuples, a class of records or a class of simple values like `int`s.

A class is constructed from a *candidate set* through the `class` operator:

    Meters = class float    // defines meters as a separate class from `float`s

    Persons = class rec{ Name:string, Address:string, ZipCode:string }

    Triangles = class { (float A, float B, float C) \ A+B>C & A+C>B & B+C>A }

Members of classes must be explicitly constructed as such. To that end, a class doubles as its own constructor. When used as a function, a class is a function which accepts a member of the class *or* a member of the class's *candidate set*:

    Zaphod = Persons rec(Name="Zaphod Beeblebrox", Address="Betelgeuse III", ZipCode="42" )
    Ford = Persons rec(Name="Ford Prefect", Address="Betelgeuse III", ZipCode="42" )

    Distance = Meters 152


Some built-in classes are:
- `int`: The set of all 32-bit integers.
- `float`: The set of all 32 bit IEEE-754 floating point numbers.
- `string`: The set of all unicode strings.
- `char`: The set of all unicode characters.



### Union types

### Discriminated unions

A discriminated union is a function which accepts a symbolic value and returns

    Status Ok code =>

## Tuples

Tuples are constructed by separating tuple components with `,`. Often you will need to enclode the components with `(` and `)`.

    (1,2)
    (1, "Alfa", 3.14)
  
A 1-tuple is just a value. The 0-tuple is called the *unit value* and is denoted `()`.

    ()      // The unit value
    (42)     // The same as just 42


## Records

Records are constructed by enclosing `,` delimited expressions within `(.` and `.)`.

    Arthur = (. Firstname = "Arthur", LastName="Dent", Age=42 .)

The record constructor defines one or more *properties* of the record. A property can be referenced by using `.`:

    Arthur.FirstName + " " + Arthur.Lastname    // "Arthur Dent"


## Lists

A list is a number of values in an ordered sequence. A list literal lists the items of the list between `[` and `]`.

    [0,1,1,2,3,5,8]

    ['A', 'B','C']

List can contain values of different types

    [0,"Arthur",'Q',3.14] // a list with int, string, char and float in the same list

The empty list

    []      // Empty list (length 0)

Lists have a length

    [].Length               // 0
    ['a','b','c'].Length    // 3

However, for lists of unknown length the length is a ambigous/choice value.


## Strings

Strings are lists of characters. 

    ['A','r','t','h','u','r']       // The same as "Arthur"

A number of functions are defined only for `string`s (lists where all elements are `char`s)


## Sets

Sets are the types of Ting; they are not data structures.

Sets can be constructed through the *set literal* by listing its members:

    { 1, 2, 3 }                     // Set of three members
    { }                             // The empty set
    { "apple", "banana", "cherry" } // Set of strings

If any of the members listed are *non-deterministic*, this non-determinism is unwinded by the set construiction:

    { 0, 11...20 }                  // Set of 0 and all numbers from 11 to 20.
    { 1..._ }                       // Set of positive integers
    { float x \ x > 0 }             // Set of positive `float` numbers
    { int x \ x%2==1 }              // Set of odd numbers
    { x \ IsPrime x }               // The set of all prime numbers
 
In math, set construc

A number of sets are built-in

- `string`: The set of all unicode strings. Alias for `System.String`.
- `char`: The set of all unicode characters
- `int`: The set of all 32-bit integers. Alias for `System.Int32`.
- `float`: The set of all 32-bit IEEE-754 floating-point numbers. Alias for `System.Float32`.
- `double`: The set of all 64-bit IEEE-754 floating-point numbers. Alias for `System.Float64`.
- `bool`: The set of boolean values `true` and `false`.

Sets can be used as functions. When a set is used as a function it acts as its own *identity function*. An identity function is a function which returns the argument passed to it. While it may not seem very useful as a function, it does have the effect that it *constrains* the argument as well as the result, which is why they can be used to declare and constrain identifiers.

    let (float answer) = 42     // float is used as identity function

Here `answer` is known to be a `float` member which is bound to the value `42f`.


## Identifiers






## Strings




     Arthur = 


### Quantifiers

The `all` operator is the *universal quantifier*. Unlike in math, the opwerator is a binary infix operator, meaning that it is placed between the declaration part and the assertion part.

    x:int all (x + 0 = x)



The `let` operator is the *existensial operator*





