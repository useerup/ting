

Sets for structural typing

Types for nominal typing



    // We represent people as tuples of names and ages
    People = Strings*Ints

    // Wines have a name and a year
    Wines = Strings*Ints


    RateWine = Wine wine \ ...

But `Wine` is just an alias for `Strings*Ints` - which is structurally equivalent to People. 

We want nominal typing.

    // Rectangles have 4 sides
    Rectangle = { Quadrilateral  }

    // Squares are rectangles where all the sides have the same length
    Square = { Rectangle (s,s) }

    
    