## Values

    42

    (42)

    "Zaphod"                        // a string instance


## Tuples

    (,42,)

    ( "Zaphod", 42 )                // A tuple instance

### Nondeterminuistic tuples

An nondetermnistic tuple is a tuple where one or more of the components have not been bound to specific values.

    ( string _, int _ )             // an non-determinuistic tuple instance of a `string` and an `int`

### Algebraic tuples

    string*int                      // set of all tuples of string*int (algebraic)

    

## Records

A record instance 

    (. Name="Zaphod", Age=42 .)     // a record instance

Like tuples, record instances may not be fully bound to specific values. A record where one or more of the properties are not fully bound is a non-deterministic record.

    (. Name:string, Age:int .)          // indeterminate record instance of two fields

    { (. Name:string, Age:int .) }      // set of records

    {. Name:string, Age:int .}          // set of records, short form

    (. Name="Zaphod" .)                 // single-field record
    (. Name:string .)                   // single-field record (indeterminate)

    (. Name:string .) ** (. Age:int .)  // record product 

    Suit = { (|Spades, Diamonds, Clubs, Hearts |) }

    Suit = {| Spades, Diamonds, Clubs, Hearts |}
    Rank adopt ./ = {| Ace, King, Queen, Jack, Number:{10...2...-1} |}
    Card = class Rank * Suit


    Shapes = { --todo-- }
    QuadriLateral = {. 
        { A, B, C, D } < Points
        Corners = [ A, B, C, D ]
        Sides = [ Line(A,B), Line(B,C), Line(C,D), Line(D,A) ]
    .}
    

    card = Cards ( Rank.King, Suit.Hearts )
    

    card |> 
    {
        ( Rank.Ace, ~suit )  ->  
        ( Rank.King, ~suit )  ->  
        ( Rank.Queen, ~suit )  ->  
        ( Rank.Jack, ~suit )  ->  
        ( Rank.Number v, ~suit )  ->  
    }


## Sets

The types of Ting are called *sets*. Sets are not data structures; rather they are collection of values, which may be defined by rules instead of listed. A value can be a member of any number of sets, so unlike for types you cannot say that a value is of a given set (type) in Ting. It may be a member of many sets.


Set constructors unwind nondeterminism and makes the set contain all of the possible values. This is akin to intensional set definition in math:

    { ( string _, int _ ) }         // set of all tuples of string*int 

## Classes

A special kind of sets are *classes*.