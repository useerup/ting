# Ting language primer





## Sets

*Sets* are central to Ting. Ting sets are not data structures, rather they are the *types* of Ting.

Ting has a number of built-in sets. Examples of built-in sets are:

- `int`: The set of all 32 bit signed integers.
- `bool`: The set of the two boolean values `true` and `false`.
- `string`: The set of all strings. String are list of unicode characters.
- `char`: The set of all Unicode characters.
- `float`: The set of all 32-bit IEEE 754 floating point values.
- `double`: The set of all 64-bit IEEE 754 floating point values.
- `decimal`: The set of all decimal encoded values.

## Refinement types

    DivisibleBy3 = { int n \ n > 0 &&  n % 3 == 0 }     // { 3, 6, 9, ... }
    DivisibleBy5 = { int n \ n > 0 &&  n % 5 == 0 }     // { 5, 10, 15, ... }

## Set arithmetic

    DivisibleBy3And5 = DivisibleBy3 & DivisibleBy5      // { 15, 30, 45, ... }
    DivisibleBy3Or5 = DivisibleBy3 | DivisibleBy5       // { 3, 5, 6, 9, 10, 12, ... }
    

## Functions



## Dependent types

    PairsDivisible =  { rec(Value:int, Divisor:int) \ Value % Divisor == 0 }

This is a set of records with fields `Value` and `Divisor`. Only records



    DivisibleBy = int d => { int n \ n % d == 0 }



    axiom Divisible = ( a:int, b:int, c:int, ad:int, bd:int ) all 
            c=a*b & a.IsDivisibleBy ad & b.IsDivisibleBy bd then 
            c.IsDivisibleBy ad & c.IsDivisibleBy bd 

The `Divisible` axiom is that
    
    
Extensions

    int..IsDivisibleBy = int n => int d => n % d == 0
    
    (3+7).IsDivisibleBy 5       // returns true



    
    AStrings = string??.(StartsWith "A")



### Polygons

Illustrates

    Polygon = {
        rec
            ordered finite Corners < Points
            Sides = (Corners*(Corners.Rotate 1)).Map (new LineSegment)
            Angles = (Sides * (Sides.Rotate 1)).Map (Angle.FromLineSegments)
            ref
                Sides.Count >= 3
    }

    Triangle = Polygon ?? 
        this => public (Side0, Side1, Side2) = Sides

    Quadrilateral = Polygon ?? 
        this => public (Side0, Side1, Side2, Side3) = Sides

    Kite = Quadrilateral ?? 
        this => Side0.Length == Side1.Length & Side2.Length == Side3.Length

    Trapezoid = Quadrilateral ?? 
        this => Side0.IsParralelTo(Side2)  |  Side1.IsParralelTo(Side3)

    Parallelogram = Quadrilateral ?? 
        this => Side0.IsParallelTo(Side2)  &  Side1.IsParralelTo(Side3) 

    Rectangle = Parallelogram ??
        this => Angles all .IsRightAngle

    Rhombus = Parallelogram ?? 
        this => let length:double in Sides all ./(Length==length)

    Square = Rectangle & Rhombus



### Set definitions

A set is defined through a *set definition*. Like in math, a set is defined using curly braces `{` and `}`. The following are examples of *set definitions*:

    { 1, 2, 3 }             // Simple list-form set definition

    { float x \ x >= 0 }    // Set Non-negative floating point numbers

    { '0'...'9' }           // Set of digits

    { let x:int in x^2 }    // Set of quadratic numbers


### Built-in sets



### Composing new sets

Refinement types


## Functions


### Sets as functions

You may have notices 



## Lists


## Classes

    MyFloats = class float

    MyFloats 3.14



## Identifiers

This is a string: `"abc def 123"`

This is an interpolated string: `$"Name: {person.Name}"`

This is a quoted identifier: `` `QuotedIdentifier` ``

This is an interpolated identifier: `` $`NameOf{person.Name}` ``