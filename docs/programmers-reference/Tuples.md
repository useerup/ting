## Tuples


### Tuples with optional components

    (public int a, public int b, public int c option=42)




### `tuple` and `tuples` operators

The `tuple` operator *explicitly* defines the operand expression as a tuple. 

The `tuple` operator also binds `this` to the actual tuple.

Under normal circumstances a parenthesized expression is automatically promoted to tuple. In certain instances there can be ambiguity as to whether an expression is a conjunction or a tuple of values. In those rare cases `tuple` can be used to disambiguate.

The `tuples` operator creates a *set* of tuple rather than a single tuple instance.

The following expressions are equivalent:

    { tuple( int _, float _, string _ ) }

    tuples( int _, float _, string _ ) 

    tuples 
        int _
        float _
        string _


