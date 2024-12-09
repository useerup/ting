# Records

The square brackets `[` and `]` are used to construct a record *instance*.

    Arthur = [ Name="Arthur Dent", Age=42 ]

The brackets enclose an expression list. Each expression in the list must be a `bool` expression which must evaluate to true. The expressions in a record constructor are all *declarative* (see *declarative and referential  expressions*). The identifiers that is declared by the expressions are the fields of the record. 

You may wonder what *type* `Arthur` above is. In Ting, types are *sets*. So the question is: To which set(s) does `Arthur` belong? The answer is that `Arthur` belongs to any set for which `Arthur`s value satisfies the set criterion.

Here are some sets that all contain `Arthur`:

    People = { [Name:string] }

    PeopleWithAge = { [Name:string, Age:int] }

    Passengers = { Arthur }

    PeopleStartingWithLetterA = People ?? ./(Name.StartsWith "A")


`Arthur` is a member of `People` because the value of `Arthur` satisfies all of the criteria expressed by the definition of `People`: 
    - must be record-structured
    - must have a field named `Name`
    - the value of `Name` must be a member of `string`

`Arthur` is a member of `PeopleWithAge` because the value of `Arthur` satisfies all of the criteria expressed by the definition of `People`: 
    - must be record-structured
    - must have a field named `Name`
    - the value of `Name` must be a member of `string`
    - must have a field named `Age`
    - the value of `Age` must be a member of `int`

`Arthur` is a member of `Passengers` because the value of Arthur is specifically mentioned as a value of the set.

`Arthur` is a member of `PeopleStartingWithLetterA` because `Arthur` is a member of `People` *and* that `Arthur`s `Name` does indeed start with the letter A.


# Lists and Tuples



    (int^_)         // List of integers of any length

    List.Of int     // List of integers of any length

    (1,2,3)

    (1,2,3,,_)

    (,a,) 


# Sets 

Sets are *inclusive*: They contain all possible matching elements


# Classes

A *class* is *a set* with one additional criterion for membership: Any member *must* be created through the class itself. A class is created through the `class` operator. The `class` operator is a prefix operator which accepts the *candidate set* of the class as its operand.

    Passengers = class { [Name:string, Age:int] }

    

# Non-deterministic expressions

An expression which can take on multiple values is said to be *non-deterministic*. 

The archetypical non-deterministic expression is the *discard* `_`. The discard can take any value; it can be an integer, a set, a class.

Other expressions may be non-deterministic while being bounded

    int _       // some integer

    float x ?>=0 ?<1    // some float between 0 (included) and 1 (excluded)

Sets can be constructed from non-deterministic expressions. In that case the set contains all possible values of the expression.

    { int x?x>0 }   // the set of positive `int`s

    { 0 -> 1, 1 -> 1, int x?>1 -> these(x-1) + these(x-2) }  // Fibonacci function

