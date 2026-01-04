I am (like most people here, I suppose) biased, as I have already considered this a lot for the language that I am designing.

TL;DR: Preference for type first for when position is more important than the name, like in tuples and formal parameters. Preference for name first when name is more important than position, like fields of a record.

In my language I support *both* "type first" and "name first". I consider the right answer to be what is most important in the situation:

- For a tuple component, the position is more important than the name. Indeed, there need not be a name. To the extent that the tuple component does have a - potentially local - name, it is not as important as the position.

- For a record field, the name more important than its position.

- For a local variable, the name more important than its position.

- Function arguments are more complicated, and ultimately a judgement call. Ignoring more advanced stuff such as named arguments, the position is arguably more important to the *caller*, while the name is more important at the definition site.

I have generalized declaration syntax to the extent that it has been erased: In my language there is no special syntax for declaring stuff. An identifier - whether it is a local variable, a function, a type (*set* in my language) or a parameter - is declared *within the expression* where it occurs.

`int x` is an *expression* in my language. While `int` is a set (type) in my language, any set can also act as its own identity function. So in the expression `int x`, `int` is the identity function of `int`, and the value of the expression is the value of `x`. 

`y:int` is *also* an expression in my language. It is a *proposition*, ie. it returns a boolean value. The `:` (*is member of*) operator accepts a set of the right side.

Whether an identifier is being declared or simply referenced within an expression, depends on the context in which the expression appears. If it appears within a *declarative scope*, then it is considered a *declaration* of that identifier within that scope. If it appears within a *referential scope* that contains the same identifier, then it is considered a *reference* to that identifier in that scope.

Some operators starts a new local scope. The lambda arrow `=>` starts a new local scope. The LHS of `=>` is *declarative* and the RHS is *referential*. This means that in `x => x * 2`, The LHS `x` is declared while the RHS `x` within `x * 2` is a reference to that x.

Some operators converts declarative scopes into referential.
- In a function application such as `f x`, the function is always referential. Any declarative scope is passed on to the argument expression. This is why in the expression `int x => x * 2`, only `x` is declared on the LHS, while `int` is a reference.
- Relational operators such as `=` (*equals*), `:` (*is member of*), `<`, `<=`, `!=`, `=>`, `>` and `==` (*equivalence*) always evaluates the RHS as referential, while the LHS continues the expression scope, referential or declarative.

Records are constructed using the `rec` operator. The operand expression is declarative and must be a proposition, i.e. must evaluate to a `true`. The identifiers declared by the operand expression are the fields of the record.

    rec Name:string                 // record with a single `Name` field
    rec (Name:string, Age:int)      // record with two fields

When an identifier occurs in a declarative scope, it is declared within that scope, establishing a binding. When an identifier occurs within a referential scope, it is a reference to the binding of that identifier in that scope.



When this expression is used to *declare*, it declares x and constrains it (the type) to be a member of the set/type `int`. The *value* of `int x` is the value of `x`.


    x=42 in x*2

    int x => x * 2

    let (x,y,z) : float^3



