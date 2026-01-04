### The `.` property selector

A property selector expression takes the form `whole.part` where 
- `whole` is some expression which has properties
- `part` is a *declarative* expression (often just an identifier)

Note how - unlike many other object-oriented languages - the `part` need not be an identifier but may be an *expression* with identifiers.

The operator can be used in both referential and declarative context. 

The right operand is always declarative. In referential context the operator is used to *reference* properties of the left operand. In declarative context the operator is used to *declare* properties on the left operand.
- In referential context the identifiers declared by the right operand references properties of the left operand.
- In declarative context the identifiers declared by the right operand are declared as properties to the left operand.

When used in prefix position - like any other binary operator - it evaluates to a function which accepts the left operand, essentially `whole => whole.part`.

A expression such as `.Name` is essentially a function which assumes that the argument is some structured value with a `Name` and returns the value of that property. `.(string Name)` is a similar function, but which is only defined for structured values which has a `Name` property the value of which is a member of the `string` set.


### The `..` member property selector


The `some..prop` expression returns a function that maps from a member of the set `some` to the property of `prop` of that member.

The left operand must be a set. The right operand is always declarative. An identifier declared by the expression is a reference to a propertu of an element of the set. 

In referential context all of the members of the left operand set **must** define properties compatible with the identifier(s) declared by the right operand.

In declarative context all of the members of the left operand set are extended with properties corresponding to the identifiers declared by the right operand.

#### Examples

Given a set `persons` the following proposition will declare a new `Age` property on every member of `persons` 

    public persons..Age = this => DateTime.Today.FullYearSubtract BirthDate

      

