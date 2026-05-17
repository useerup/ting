### The `.` property selector

A property selector expression takes the form `whole.part` where 
- `whole` is some expression which has properties
- `part` is a *declarative* expression (often just an identifier)

Note how - unlike many other object-oriented languages - the `part` need not be an identifier but may be an *expression* with identifiers.

The operator can be used in both referential and declarative context. 

The right `part` operand is always declarative. The identifiers that it declares are properties of the `whole` left operand.

In referential context the operator is used to *reference* the properties of the left operand.

In declarative context the operator is used to *declare* the properties on the left operand.

When used in prefix position - like any other binary operator - it evaluates to a function which accepts the left operand, essentially `whole => whole.part`.

A expression such as `.Name` is thus a function which assumes that the argument is some structured value with a `Name` property. It returns the value of that property.

`.(string Name)` is a similar function, but which is only defined for structured values which has a `Name` property the value of which is a member of the `string` set.

`.(string Name, int Age)` is a function which is only defined for structured values which has a `Name` and an `Age` property. The value of `Name` mustbe a member of `string`, and the value of `Age` must be a member of `int`. The result is a *tuple* of those two values.

`.record(Name, Age)`

`.$"{string Name} (age {int Age})"`. When invoked with a record `record(Name="Tom Balivar", Age=32)` this function will return the string `Tom Balivar (age 32)`



### The `..` member property selector


The `some..prop` expression returns a function that maps from a member of the set `some` to the property of `prop` of that member.

The left operand must be a set. The right operand is always declarative. An identifier declared by the expression is a reference to a propertu of an element of the set. 

In referential context all of the members of the left operand set **must** define properties compatible with the identifier(s) declared by the right operand.

In declarative context all of the members of the left operand set are extended with properties corresponding to the identifiers declared by the right operand.

#### Examples

Given a set `persons` the following proposition will declare a new `Age` property on every member of `persons` 

    public persons..Age = this => DateTime.Today.FullYearSubtract BirthDate

      

