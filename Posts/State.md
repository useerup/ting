# No side-effects

Ting functions are not allowed to have side-effects. The decision to make functions pure was a consequence of Ting being a logic programming language, where the compiler may satisfy (or generate code to satisfy) a given proposition *any way it can*. 

In its simplest form, a Ting program is an expression which calculates a value. The value of the expression is the result of the program execution. The goal of the compiler is to generate a program which will compute the value of the program expression at runtime.

Thus, this is a simple program:

    "Hello, World!"

The value of this expression is the string "Hello, World!". This is an utterly uninteresting program, and even the compiler  will (when I have a compiler) just reduce it to emit the string on stdout.

A *slightly* more interesting program is this:

    { let x^2-x-2=0 in x }

This is the set of values of `x` such that the quadratic equation is satisfied. The compiler does not know how to solve this by itself, and the above "program" should not compile.

The compiler *does* know about *libraries*, and Ting libraries may offer *rewrite rules* which can rewrite terms. In this case a (as of now, hypothetical) `QuadraticEquations` library could be used:

    using QuadraticEquations in
    { let x^2-x-2=0 in x }

This should compile. However, at *compile time* this will *still* reduce to a program which - when run - will emit the (constant) set `{-1,2}`. In other words, the program will not solve the quadratic equation at runtime - it will reduce to simply print a set literal. I did say *slightly* more interesting.

This reduction is roughly equivalent to *constant folding and merging* known from other programming languages, albeit folding and merging through external libraries. 

So how do we write *interesting* programs with Ting? *Interesting* as in programs which you can interact with and which can maintain state, and yes - which can have side effects. 

I could have gone with a monad solution here, but I didn't, partly because my idea for state management was born before I learned about monads, and partly because I still cannot claim that I understand monads.

# Constraints

The solution I chose for Ting is born from a design goal I had from the start of the project: I wanted to use logic programming to allow the programmers to *specify* rather than *program*, and only require them to *program* (as in algorithms) when necessary. This is often referred to as *declarative programming* and *constraint programming*.

As such, I envisioned that the programmer describes a system by setting up a set of rules. 

Consider a (contrived) example of an order-management system:

- Customers places orders.
- Each order will consist of a number of order lines.
- The order total is the sum of the order lines minus discounts plus shipping costs. 
- An order line consists of an item reference, quantity, and possibly a unit-price.

The code could look something like this:
    
    System (.

        Customers = {. 
            identity CustomerNo:string
            Name:string
            Orders=../Orders.Where(o->o.Customer==this)
        .} mandated {} 
        
        Orders = {.
            identity OrderNo : string
            Lines = {. Item:Items, Quantity:decimal, LineTotal=Item.UnitPrice*Quantity .}
            LinesTotal = Lines.SelectMany(.LineTotal).Sum
            DiscountPercent : decimal
            Discount = LinesTotal * DiscountPercent / 100
            OrderTotal =  LinesTotal - Discount
        .} mandated {}

        Items = {.
            identity ItemNo : string
            Name : string
            UnitPrice : decimal
            QuantityInWarehouse : decimal
            let lines = Orders.SelectMany(.Lines).Where(.(Item=this)) in
            OrderedQuantity = lines.SelectMany(.Quantity).Sum
        .} mandated {}

        \\

        Auto override || {
            Customers $ +: Console.Input
            Orders $ +: Console.Input
            Items $ +: Console.Input
        }

    .)

A couple of points:

* `(.` ... `.)` is a *record literal* ^[I chose this notation because it resembles a tuple `(` ... `)` but where fields are identified with `.` dot-notation instead of by position.].
* `{.` ... `.}` is a *set* of records. This notation is just a shorthand for `{` `(.` ... `.)` `}`. The notation `{` ... `}` itself is a *set literal* ^[The set literal `{` ... `}` expands the listed elements, so when the expression is ambigous - i.e. not constrained to a single value - the set contains *all* of the possible values. A set literal *unwinds* ambiguities.].
* The `$` operator is the *oppropose* operator. The name is a contraction of *oppose* and *propose*. It opposes a value and proposes a new value.
* The `+:` operator 


A set is a type in Ting. For example, `{. A:int .}` defines the set (i.e. *type*) of *records* which has a *field* called `A`, the value of which belongs to the set of integers.

Sets can be nested inside other sets. For example, `Customers` has a field called `Orders` which is itself a set (subset of the outermost `Orders` set). Likewise, `Orders` has a field `Lines` which is also a set.

Some of the fields of the members of these sets are *constrained*. For example, `OrderTotal` must equal the sum if the order lines minus discount. However, in general the record expression is *ambigous*: Many values exists which would satisfy the record constraints. This is the *freedom* within the constraints.

If we were to bind each field to a value, we would be able to determine if the constraints are satisfied, and thus if those bindings are valid. 

A single instance of an ambigous expression, where each ambigous field has been assigned a concrete value with fully bound fields, and which satisfies all of the constraints, is referred to as a *valuation*. 

The the simplest such valuation is one with no `Customers`, no `Orders` and no `Items`. Going on from there we can imagine another valuation which has a single customer, then two customers, then an order and so forth. In this case there is obviously an infinite number of valuations. 

But we have (at least) two problems: 1) How to "initialize" such an order management system with an initial valuation - with empty sets or otherwise - and 2) how to "add" set members such as items, customers and orders, i.e. how to *mutate* the state, while (preferably) retaining the pure nature of the expression? And how can we do that while still claiming that the program is *pure*?


# Things and interregnums

If you look at the order management system above, it should be clear that it describes the constraints of a well-formed order management system *at rest*. 

We now add *transitional rules*. A transitional rule is a rule which can transition from one valuation to another, i.e. from one well-formed state to another. A transitional rule is a function which accepts a system expression and returns a modified system expression.

    AddOrder = order -> this with( Orders = Orders +: order ) 

In Danish and Norwegain, the word "Ting" has a dual meaning. Nowadays *Ting* most often refers to the abstract concept of *things*, i.e. objects or even ideas (as in *it is a good thing*). However, *Ting* is also the name for the periodic Viking assemblies where *free men* would gather, make laws, settle disputes, and pass judgments. The word survives to this day in *Stortinget* (the Norwegian parliament, literally *the grand ting* ) and *Folketinget* (the Danish parliament, literally *the peoples ting*). 

In between the Ting gatherings were interregnum periods during which you were expected to live by the decisions of the Ting. People didn't live so that they could gather at Tings, although I am sure that there was also a social aspect as well, and that Mjöd was involved. No, they gathered at the Tings so that they could take care of necessary business and then go back and live their lives, until the next Ting. Tings were where the rules were made and changed, but what the participants really wanted was to go back and live their "stable" Viking life.
