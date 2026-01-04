# Ting type system


## Sets

*Sets* are the types of Ting.

Sets are *inclusive*: A set contains all values that satisfy the set condition. 


## Simple types


## Tuples

Tuples are position-structured values. Each component of a tuple is identified by its position within the tuple.

    ("Zaphod", 42, true)


While the parenthesis are not part of the tuple construction syntax itself, it is often nesscary to enclose the tuple expression list within parenthesis to disambiguate from the sorrounding expression.

The *identity* of a tuple is that of the combined identity of its components.


## Records

Records are name-structured values

    rec( Name="Zaphod", Age=42 )

The identity of a record is the combined identity of its fields.



## Records and tuples

Records and tuples can be combined. An expression list can be broken up into a tuple and a record part using the `\\` operator:

    ( int a, float b \\ Name:string, Age:int )

The identity of such a combined tuple and record is the combined identity of the tuple and the record component.


## Classes

A *class* in Ting is not bound to be name-structured like in many other languages. Rather, classes in Ting are the *nominal* types. 

Ting classes are constructed from a *candidate set*. 

    OrangesCount = class int
    ApplesCount = class int

    myApples = ApplesCount 5
    myOranges = OrangesCount 2

    myFruits = myApples + myOranges