This is how I do it in my Ting language.

*Sets* are the structural types of my language. Keep that in mind when I refer to *set* or *sets* below.

## Tuples

Tuples are structured values where the items are identified by position.
   
    Empty = ()
    ThreeNumbers = (1, 2, 3)
    Heterogenous = (1, "Hello", 3.4)
    WithNamedItems = (public float X, public float Y)

The *identity* of a tuple is the combined ordered identity of the items.

## Records

Tuples are structured values where the items are identified by position.
   
    Zaphod = (. Name="Zaphod", Age=42 .)
    Arthur = (. Name="Arthur", Age=42 .)

The *identity* of a record is the combined identity of the named items. Order of items does not matter.
    
## Sets of tuples (structural types of tuples)

    Points2D = Floats*Floats
    Coordinates = { (public float X, public float Y) }
    
## Sets of records (structural record types)

    ComplexNumbers = { (. Real:Floats, Img:Floats .) }
    Passengers = { Zaphod, Arthur }

    People = {. Name:Strings, Age:Ints .}   // { (. <fields> .) } can be shortened to {. <fields> .}



## Tagged sets

Given sets such as `Ints` and `Strings`, we can also create a *tagged* set:


    PhoneNumbers = Phone of Strings
    Addresses = Address of {. StreetName:Strings, HouseNumber:Ints, ZipCode:Strings, City:Strings .}

A tagged set is essentially a one-field record set.




## Product types (product sets)

Product sets (product types) can be created from tuple-structured sets as well as record-structured sets:

    Points3D = Points2D * Floats    // extend with a float

Extending record-structured sets:

    Customers = People * Addresses * PhoneNumbers

A product set of two tuple-structured sets is a tuple-structured set.
A product set of two record-structured sets is a record-structured set.


## Sum types (Sum sets)

Two types (sets) can be added

    Ints + Strings

The resulting set will contain all items from `Ints` and all items from `Strings`. As there are no overlap between these, this es essentially the same as a union set `Ints | Strings`.

However, you will often want to create sums of *tagged* sets:

    Shapes = 
        Circle of {. Center:Points2D, Radius:Floats .} 
        + Square of {. TopLeft:Floats, SideLength:Floats .}



## Intersection types, union types, complement types

Sets/types can be combined using operators `|`, `||`, `|||`, `&`, `&&`, `&&&`. These are variants of unions and intersections.

Sets/types can be *complemented*. The *not operator* `!` can be applied to a set to create the *complement*.

    NotInts : !Ints

This is the set of any value that *is not an integer*.

