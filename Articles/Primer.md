
A Ting program is an expression that is evaluated

A Ting system is an expression which must evaluate to true for each state




Sets are constructed through a *set constructor*. A set constructor is a list of expressions between `{` and `}`. 

    Numbers = { 1, 2, 3, 4, 5, 6 }
    Names = { "Ford Prefect", "Arthur Dent", "Tricia McMillan", "Zaphod Beeblebrox" }

If any of the expressions are *nondeterministic* then the set will contain all of the possible values of the nondeterministic expression.

    Evens = { int _ * 2 }

A set is implicitly also a function. If the set contains only non-relation values, then the function is the set's *identity function*.


Since `Strings` is a set, the expression `Strings "flollop"` will return the string "flollop". This may seem silly, but consider the expression `Strings s`. That expression will return the string value of `s`, *but the expression is only defined when `s` is a string*. In other words, `s` is bound to be a string.

This is what we exploit to bind variables to a set of possible types. Consider a definition

    Wines = { (Strings name, Ints age) \\ age>0 && name!="" }



Functions are sets of mapping




    Persons = class { (Strings _, Date _) }

    Wines = class { (Strings _, Date _) }

