# Introduction to the Ting programming language

Ting is a logic, functional, and object-oriented programming language. The syntax is simple - everything is an experession - but it has powerful semantics which allow you to express quite complex concepts in a simple and straightforward way.


## Quick look

    {1,2}

    {7,8}

    {1,2}*{7,8}     // The same as { (1,7), (1,8), (2,7), (2,8) }




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

Sets can be constructed through the *set literal* by listing its members:

    { 1, 2, 3 }         // Set of three members
    { }                 // The empty set

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