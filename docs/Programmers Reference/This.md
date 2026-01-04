## `this` Current scope

`this` can be used to disambiguate references.

### Declaring and binding `this`

Using `this` in a declarative position declares the scope(s) in which it is declared.

Usually you will use this to simplify a function:

    DateTime this => $"{Year}-{Month}-{Day} {Hours}:{Minutes}"

This especially useful when extending a set members with new properties:

    public Persons..FullName = this => FirstName + " " + LastName