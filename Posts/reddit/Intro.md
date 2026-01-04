# Ting language quick look

The language is a set-based, logic object-oriented programming language.

## Sets are the structural types

By *set-based* I refer to the idea that sets are the types of the language, i.e. sets are **not** data structures.

Some built-in sets are for example:

    string    // set of all strings
    int       // set of all 32-bit integers
    float     // set of all 32-bit IEEE-754 numbers
    double    // set of all 64 bit IEEE-754 numbers
    char      // set of all unicode characters
    Sets      // set of all sets

Example sets:

    { 1, 2, 3 }  
    { int x \ x % 2 == 0 }      // set of even integers
    {. Name:string, Age:int .}  // set of records
    {| Red, Blue, Green |}      // Set of symbolic values
    float^2                     // set of all 2-tuples of floats
    int => string               // set of all functions from int to string
    
    // A set of discriminated values (a discriminated union)
    {| And of Exp*Exp, Or of Exp*Exp, Not of Exp |}     // Logic expression

Subsets (refinement types) can be created from a set by using the `Where` method or the ?? operator.

    float ?? > 0        // Positive floats
    float ?? != 0       // Nonzero floats
    
    string.Where( ./StartsWith "A" )    

Sets are the *structural types*. They include any value which matches the set condition.

Sets are first class members. They may be stored in variables, passed as parameters, constructed, deconstructed etc.

## Classes are the nominal types

The nominal types of Ting are called *classes*. Unlike most other programming languages, the term *class* does not presuppose any structure of the inhabitants of the class. Classes can be classes of records, classes of tuples or classes of simple values.

    Meters = class float
    NumberOfApples = class int
    NumberOfOranges = class int
    People = class {. Name:string, Age:int .}

Each class created using the `class` keyword is distinct from other classes. The class is constructed from a *candidate set*. Only members of the candidate set can be members of the class.

Unlike plain sets, classes er sets with an additional membership requirement: The value must be explicitly created using the class as constructor.

    var oranges = NumberOfOranges 42
    var apples = NumberOfApples 3

`NumberOfApples` is a distinct set/type from `NumberOfOranges`. Members of `NumberOfApples` cannot be member of `NumberOfOranges`.

## Functions

Functions establish relations between arguments (members of the *domain* of the function) as results (members of the *codomain* of the function). Functions are always anonymous but may be assigned to variables, passed as parameters etc.

Functions can be constructed in multiple ways. The simplest is through the lambda arrow:

    Double = float x -> x * 2

Functions have an underlying set of ordered pairs (remember, Ting is set-based). A function can be created from ordered pairs using the `{>` ... `<}` special constructor

    Ordinals = {> 1-->"first", 2-->"second", 3-->"third" <}

## Logic programming, multi-modality

Being a logic programming language, the fact that functions establish relations between the argument and result can be used to bind "the other way":

    var Ordinal rank = "second"     // binds rank to 2
    
    var 3*x^2 - 10*x + 3 = 0        // binds x to 3 by solving the quadratic equation

## Nondeterminism

Like Prolog, Ting embraces non-determinism. However, Ting also has features to reign in non-determinism which can otherwise spread throughout the program.

One way to introduce non-determinism is to use an operator or a function which is will inherently bind variables non-deterministically. Many of the relational operators are inherently non-deterministic. Some of those operators are:

* `:` membership (of a set)
* `::` collection of members of a set
* `<`,`<=`,`!=`,`>=`,`>`: relational scalar values or set/subset relations.

Sets constructors is a common way to reign in non-determinism. Set constructors "unwinds" nondeterminism. This is an example of a record-structured set:

    {. Name:string, Rank:{"first","second","third"} .}

Here, `Name` can take any string from, and `Rank` can be any of the strings `"first"`, `"second"` or `"third"`. The set unwinds this non-determinism and the set to contains *all* possible combinations of those values. This is equivalent how Ting supports *intensional set definitions*.

### The choice operator

Another way non-determinism can arise is through explicit use of the `;` *choice operator*. This operator will return a value which is non-deterministically either the LHS or the RHS value.

This can be used in generative set definitions:

    // The infinite set of Fibonacci numbers
    let fib = (a,b) -> b;fib(b,a+b) in 
    { (0,1) |> fib  }

## Object-orientation

Sets can be nested, and functions (methods) can be defined scoped to members of sets. Function scoped to members of classes can be abstract or virtual, allowing subtypes (subclasses) to override them.

## Chess-themed example

This is an example program I am working on (work in progress).

### The board

We start by modeling the chess board which consists 64 squares created by a grid of *ranks* (rows) and *files* (columns). The ranks are numbered from 1 to 8 and the files are designated from *a* to *h*. The form a grid of *squares*. We model the rank and files as *classes*, and the squares as *tuples* of those:

    Rank = class { '1' ... '8' }
    File = class { 'a' ... 'h' }
    Square = File * Rank

By defining ranks and files as *classes*, we ensure that they are not mistaken for the characters that are used to *identify* them. This captures the concept, that within the chess domain, a *file* is something on a chess board that is *identified* by a character, but it is not *itself* a character.

### Board arithmetic

For convenience, we overload the `+` operator so that we can use it to add an integer to a rank or to a file to obtain a rank or a file relative to it. We do the same for squares; only for squares should be able to add a tuple of integers instead of a single integer:

    `_+_` override || >>>
        (Rank.ElementAt r, int dr)  =>  Rank.ElementAt( r + dr )
        (File.ElementAt f, int df)  =>  File.ElementAt( f + df )
        (Squares (f,r), (int df, int dr))  =>  (f + df, r + dr )

Note the use of `ElementAt` to bind the index in the argument definitions. All ordered sets has an ElementAt method which accepts a zero-based index an returns the element in at position. However, here we used it *both* ways, as it can also bind its argument if the result is bound.

### Players

The players are identified by a discriminated union. For convenience we also define a *member method* for calculating the *opponent* of a given player. This allows us to write `p.Opponent` when `p` is a member of the `Player` set:

    Player = {| White, Black |} \\ 
        public Opponent = White -> Black || Black -> White

### Pieces

    PieceType = {| King, Queen, Rook, Bishop, Knight, Pawn |}
    
    TODO: Define how pieces move and strike.

### Situations

A *situation* is a state in the game characterized by how the pieces are set up

    Situation = {.
    
        Pieces = Square => Player*PieceType
        AvailableCastlings = Player => {| KingSide, QueenSide |}
        // TODO: en-passant etc.
    .}
    
    // TODO a lot more :-)