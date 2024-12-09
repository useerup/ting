// https://www.reddit.com/r/ProgrammingLanguages/comments/1cjtptw/clone_wars/

`EvenNumber` and `OddNumber` are *refinements* of `int`:

    EvenNumber = { int x \ x%2==0 }
    OddNumber = { int x \ x%2==1 }

An `OddNumber` plus an `OddNumber` should be an `EvenNumber`. An `EvenNumber` plus an `EvenNumber` should be an `EvenNumber`. This is not overloading; rather it is asserted so that the type system can use it:

    (a:OddNumber, b:OddNumber, c=a+b) axiom c:EvenNumber
    (a:EvenNumber, b:EvenNumber, c=a+b) axiom c:EvenNumber


`ShoeSize` is a

    ShoeSize = class int



Lengths

    Meters unit m = class float
    where
        unit [cm]
        Inches unit [in]

        Convert override || 


    2.54[cm] = 1[in]
    1[m] = 100[cm]
    
    let x:float in m = (x*100) cm

Area

    SquareMeters = m^2
    SquareCentimeters = cm^2
