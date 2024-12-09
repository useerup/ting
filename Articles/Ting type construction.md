I am still developing my **Ting** logic programming language. 

This is a very old itch of mine, which I unfortunately cannot help scratching.

Originally the language was developed from classic logic. Although fascination with Prolog was what made my friend and I start out on this, the language itself is not derived from Prolog nor any other language that we know of, besides mathematical/classic logic.

Here I would like to describe how the language features both *sets* (collected data types) and *types* (constructive data types), in the hope that you will provide me with some constructive feedback, challenges, comments, and/or encouragement.


# Sets

Set members are *collected*: A set contains all objects that satisfy the set condition.

A set can be defined through a *set constructor* or through a *set expression*.

A *set constructor* is a list of expressions enclosed by `{` and `}`.

    OneTwoThree = {1, 2, 3}

    Names = { "Alice", "Bob" }

    Disparates = { "Zaphod", 42, true }

    Empty = {}


The above sets are alle constructed from a list of expression where each expression is simply a constant value.

An expression in a set constructor can also be *non-deterministic*, in which case the set will contain *all* of the possible values of that expression. A set constructor unrolls the nondeterminism of the expressions.

    PositiveInts = { int _ ? > 0 }

    Halves = { int _ / 2f }

    Points = { [x:float,y:float] }

Sets are first class objects, and can also be defined through *set expressions*.

    Coordinates2D = double*double

    Coordinates3D = double^3

    NamesOrNumbers = Names | OneTwoThree

    NamesAndAges = Names * (int??>=0)


# Types

Class members are *constructed*: An object is of a given class if and onlty if it has been constructed as a member of that class or a subclass of it.

A class is defined based on a *candidate set* through the `class` operator:

    // Customers is a set of records, each with a number and a name
    Customers = class { [Number:int, Name:string] }

Like with sets, a class is also the identity function onto itself.

    // declare a customer
    Customers c

    Zaphod = Customer [Number=42, Name="Zaphod Beeblebrox"]


# Functions

A function is merely a set of relations. The operator `->` defines a relation between two objects.

    Fibonacci = { 0 -> 0, 1 -> 1, int n?>1 -> these(n-1) + these(n-2) }

The domain of this `Fibonacci` function is the set `{0,1,int _?>1}` (0, 1 and any integer greater then 1) which can be reduced (normalized) to `int??>=0` (the subset of `int` where each member is greater than or equal to zero).

The codomain of `Fibonacci` is the set of fibonacci numbers *{ 0, 1, 2, 3, 5, ... }*

A short form or declaring a function is also the familiar *lambda* `=>`:

    Double = float x => x*2

However, this is equivalent to writing 

    Double = { float x -> x*2 }


# Definedness and dependent sets/dependent types

An example of a dependent set is the domain of the *divide* operator `/`. The divide operator maps to a union of functions (some numeric types omitted for brevity):

    (/) = DivideInts || DivideFloats || DivideDoubles || ...

    DivideInts = (int left, int right ? != 0) => ...

    DivideFloats = (float left, float right ? != 0) => ...

    DivideDoubles = (double left, double right ? != 0) => ...

Each of the divide functions excludes `0` (zero) as a denominator (right operand). The domain of the divide functions are *dependent sets*.

    // `g` accepts a `float` number and returns a function that is defined
    g  =  float x => float y!!(y-x!=0) => y/(y-x)
    
    // f is the same as `x ? != 5 => x/(x-5)`
    f  =  g 5           


