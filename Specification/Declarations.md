
Expressions of the declarative set constructor `{` ... `}` are declarative.

    { exp1, exp2, exp3 }

Each expression in the expression list is in a scope of its own. Identifiers declared by exp1 are *local* to exp1 and cannot be referenced from exp2 or exp3. 


## Nested declarative set constructors

If a set constructor itself appears in a declarative context

If this expression appears in declarative context

    { exp1, exp2, exp3 }

then identifiers declared by exp1, exp2 and exp3 will be declared in the outer scope *and* in each respective inner scope.

Example

    {
        { int a, string s} // set of all `int`s and all `string`s

        // in this scope `a` is a nondeterministic member of `int`
        // and `s` is a nondeterministic member of `string`
    }


## Private identifiers



## Protected identifiers



## Public identifiers