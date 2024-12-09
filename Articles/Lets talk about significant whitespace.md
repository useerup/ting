


Traditionally we delimit *lists of items* with some form of delimiter between the items. That's sensible. It works well for short lists of stuff like parameters of a function, elements of tuples and short array/list literals. 

Note here, I am not just talking about stuff like the list data structure or other data structures. When we program we create **lots* of lists of stuff: Imperative languages have lists statements, Haskell has "do-lists", functional languages has all sort of lists. Virtually all programming languages has some form of lists of definitions. Sometimes the programming language will not demand that we explicitly *delimit* the items because the grammar has been defined to be unambigous about it. In JavaScript you can even omit the statement delimiter `;`

When we write longer lists in our programs, we tend to break the lists up so that each line will hold an item. Cobol and Fortran were "line oriented" probably owing to punch cards being the primary way to input programs at the time those languages were initially defined. BASIC was designed with line-orientation to allow for editing using a very simple editor.

With the advent of CRT terminals and text editors, the "Algols" and other *context free grammar* languages blossomed. Line-orientation was sooo much punch-cardy. For a long time, any language who wanted to compete *had* to use explicit terminals to delimit blocks, statements, expressions, items etc.

Then came the counter-revolution. The space *between* the terminals of the grammar (the "white space") became fashionable again. However, this was not a revert back to the *line-orientation*. This was mostly a reaction to the fact that programmers did in fact illustrate the block structures with indentation *anyway*. It was a bold move, mainly popularized by Python.



For short lists it is ok to keep the items on the same line.
For longer lists we break the list across multiple lines. When we do that, we *often* place each item on it's own separate line


When delimited lists grow longer, we like to break them up over multiple lines. Each line will then  with one item on each line. Each line *except for the last* will then typically end with the list delimiter.








A *same indent* line break is replaced by an implicit item delimiter, *unless* one of the following is true:
* the line *begins* with a binary operator
* the previous line *ends* with a binary operator or a left-bracketing symbol

A *more indented* line is equivalent to a paranthesized expression. An invisible parenthesis is put around all of the indented 

    matress
        .flip
        .flop
        + 42

    value
        + 42
        + 65
        - 76



A *set* is inclusive. Any object for which the set proposition is defined and holds is a member of the set.

A *class* is exclusive. A class is *based* on a set (the *candidate set* of the class), but only objects constructed through the class are inhabitants of the class.



    FooPoint = {.
        int x
        int y
        string tag
        \\
        x < y
        y !: [200...2000]
        tag="hack" --> x > 100 --- x > 1000 & x < 2000
    .}


    Feathered = class Records 
    Organisms = class Records

    // Animals is a sub-class of Organisms
    Animals = extend Organisms

    Birds = extend Animals & Feathered
    Plants = extend Organisms
    Trees = extend Plants 



    Environment do 
        a := 42
        a := 49

        

    (:=) = (a,b) \ Assign(a,b)

    

    Console.Out do 
        WriteLine "Flollop"
        WriteLine "zark"

    Console.Out := Console.Out
        .WriteLine("Flollop")
        .WriteLine("zark")
    
