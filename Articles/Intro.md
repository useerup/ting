# Introducing the Ting language

## Classes

*Classes* are the nominal types of Ting. A class is created based on a candidate set:

    Apples = struct int
    Oranges = struct int
    

    oranges = Oranges 42
    apples = Apples 21

    // error
    applesAndOranges = oranges + apples 

    // error
    oranges = Oranges (oranges.Value * 2)



## Units of measure

In Ting, *units of measure* is a library feature. A unit of measure is a pair of a *unit* and a *numeric value*.

Units of measures can be added, multiplied, divided, subtracted, raised to the power (including squared and square root) etc.
    
    UnitOfMeasure.Add = 
        u:UnitOfMeasure in 
            (u.Unit left, u.Unit right) => u( left.Value + right.Value )

    UnitOfMeasure.MultiplyValues = 
        uLeft:UnitOfMeasure & uRight:UnitOfMeasure in 
            (uLeft left, uRight right) => (uLeft*uRight) (left.Value * right.Value)

    UnitOfMeasure.MultiplyUnits = 
        (UnitOfMeasure left, UnitOfMeasure right) => UnitOfMeasure.Product(left,right)




  
    // Time
    Seconds = class UnitOfMeasure 
    Minutes = Seconds * 60
    Hours = Minutes * 60
    Days = Hours * 24
  
    // Distance
    Meters = class UnitOfMeasure
    Centimeters = Meters / 100
    Kilometers = Meters*1000

    // US Distance
    Inches = Centimeters * 2.54
    Feet = Inches * 12
    Yards = Feet * 3
    Miles = Yards * 1760

    // Speed
    KmH = Kilometers/Hours
    ms = Meters/Seconds
    MPH = Miles/Hours
