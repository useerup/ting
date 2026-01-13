## Records


### `record` and `records`

The `record` prefix operator accepts a single operand. The operand is *declarative* and the record will have as fields all of the identifiers that are declared by the operand.

    person = record Name:string        // a record with a single field

    person = record(Name:string, Age:int)

    person = record(Name:string)

    persons = { record(Name:string, Age:int) }


The `records` prefix operator is a shortcut for creating *a set of records*

The following 2 expressions are equvalent, and both produces the same set

    records( Name:string, Age:int )

    { record( Name:string, Age:int ) }

You can of course combine with the indentation rule that a number of indented lines form a parenthesized expression or expression list:

    Persons = records
        Name:string
        Age:int

