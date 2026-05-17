# Ting Programming Language

Ting is a logic programming language. It unifies logic constraint programming with object-oriented- and functional programming. Constraint programming means that you define the bounds (constraints) of the problem and let the compiler with its associated libraries figure out a solution. An example of a simple program is one that prints out the solutions to a quadratic equation:

    import Math.Polynomials
    float x \ 2*x^2 + 10*x - 48 == 0

When run, this program will output 

    3
    -8

Ting - like most other logic programming languages - embraces nondeterminism: The value of an expression may not be a single value, it may multiple values at the same time. 

But constraints programming is not just for solving quadratic equations or indeed methematical equations. The layout engine of your browser is a typical constraint problem: How to flow text into a box is a problem of fitting as many words as possible per line without overflowing the width.

But how does Ting know how to solve a quadratic equation? As you may suspect from the line `import Math.Polynomials`, it doesn't. The Ting compiler knows about nondeterminism, bindings of variables and "truth values", which means the values `true` and `false`. Solving a quadratic equation is a feature implemented by a library. The Ting compiler knows how to *search* for solutions. Somwhere in the `Math.Polynomials` there is an *axiom* like this:

    (bound float a, bound float b, bound float c, free float x) \ a*x^2 + b*x + c == 0  axiom
        let 
            d = b^2-4*a*c
        in 
            x =
                d<0 then 
                    undefined
                else d==0 then
                    -b/2/a
                else (-b +/- Math.Sqrt d)/2/a





But there is more going on in the above example

Let's up the ante a bit a and create a program which does not solve the equation at compile time buth rather compiled to a program which solves quadratic equations:

    import Math.Polynomials
    [float a, float b, float c] => float x \ a*x^2 + b*x + c == 0

This time the pogram is a function that maps a list of 3 `float` values to a quadratic equation using those 3 `float`s as coefficients.


## Nondeterminism

*Nondeterminism* is the ability for a program expression to have multiple valid outcomes. Instead of computing one fixed result, Ting can represent and explore several possible values that satisfy the program's constraints. It does this by default by employing backtracking, going back to the last point where another, but by employing sets and functions you as a programmer can tightly control this behavior.

Perhaps the most common nondeterministic expression is that of a declaration using a set, like e.g. `var int n`. This declares `n` as some member of the set `int`. This is fundamentally nondeterministic: Any of `int`s members will fit the bill. Howevever, in most programs other constraints will limit the possible the value of `n` to the point where only a single value is possible (i.e. deterministic). 

The *choice operator* `;` **explicitly** introduces nondeterminism. This operator introduces a nondeterministic binary choice. The expression `0;1` can assume the value `0` *or* the value `1`.

The *range operators* like `...` also causes nondeterminism to arise. The expression `0...9` can assume any one of the values `0` through `9`. 

Nondeterministic expressions are often used inside set constructors, where the combination effectively becomes an *intensional set definition*. 

    Values = { 0...9 }

    Even = { int n \ n % 2 == 0 }



Functions can be nondeterministic. 

Using nondeterministic functions to define sets can be a really powerful. This is the set of Fibonacci numbers:

    { let f = (a,b) => a ; f(b,a+b) in f(0,1) }

The generating function is a nondeterministic, recursive function.


## Constraint programming

The following is a small program which calculates a value by solving an equation:

    float x \ x - 10 == 0

It reads as *a float value `x` *such that*  `x - 10 == 0`*. The compiler will solve `x` for that equation. To do this it draws on the arithmetic library though which it can discover that `x - 10 == 0` where `x` is not bound can be solved by rewriting into `x == 0 + 10`. 

This may seem limited in scope. After all, how often do we really need to solve equations programatically? But let's look at another example. Consider this function:

    Char = char c => [c] + rest => rest

This is a fundamental *parser function*: It accepts a character and returns a function which accepts a string starting with that character and returns the remaining string.

If we apply this function to a character like `Char 'A'`, we get back a function which is essentially `'A' + rest => rest`. This is a function which accepts a string beginning with the character `A` and returns the string excluding that leading `A`. 

However, if we apply `Char` to a *free variable* we get back a function which will parse *any* character and bind that character to the argument.

Buildiing on that we can also do:

    Digit = {'0'...'9'} >> Char
    Letter = Unicode.Letters >> Char

This demonstrates *set promotion* and *function composition*. *Set promotion* means that any set can be automatically promoted to a function. When promoted, a set becomes its own identity function. The set `{'0'...'9'}` is promoted to a function because it is used with the `>>` operator.

*Function composition* (also referred to as *pipelining*) composes a new function by using the result from one directly as the argument for another. In above `Digit` and `Letter` functions, the composition has the effect of *restricting* what arguments can be passed on to the `Char` function, thus creating parser functions which will parse digits and letters, respectively.

We can combine those to e.g. create a parser function which will parse a letter and 2 digits: 

    [x,d1,d2] => Letter x >> Digit d1 >> Digit d2

In Ting, strings are formally lists of characters, so this function "accepts" a string with 3 characters and returns a parser for it, or more precisely it *establishes a relation between a string and a parser for that string*. 


## Sets are types

    Fibonacci = {   }


In Ting there is no formal types; not because Ting does not have types but rather because in Ting the types are referred to as *sets*. It is important to understand, that in Ting a *set* is **not** a data structure. Like in math, a set in Ting is an abstract collection of elements. Thus they can ideas such as "all cars that are painted red".

Some sets are like structural types: They include as elements all values that meet a certain criteria. An example of this would be the set of all even `int`s:

    { int x \ x % 2 == 0 }

This is effectively a *refinement type*. The compiler will not *materialize* the set (enumerate all the members) unless necessary. If you only use the set to test for asking whether a given object is a member, only the membership test is required.

Like in math you can also define a set simply by listing its elements:

    { 2, 4, 6, 8 }

In Ting *sets* are the types. In the above example `float` is the set of all 32-bit IEEE-754 floating point values. `float` is one of the built-in standard sets. Other sets are `double`, `char`, `string`, `int`, `bool`. 

Thus a set in Ting is not a data structure. 

## Structured data types

Ting features tuples and records as ways to create structures

The `records` keyword defines a set of records:

    records 

## Functions

Functions are values.

A number of operators will accept functions and operands. Given that `f` and `g` are functions the following are examples of operators:
- `f || g`: returns a function which evaluates to `f x` when `f` is defined for `x` and `g x` in other cases. `g x` is only evaluated when `f x` is not defined.
- `f | g`: returns a function which evaluates to `f x`, `g x` or `(f x),,(g x)` depending on whether `f`, `g` or both is defined for `x`, respectively.
- `f && g`:
- `f & g`:
- `f >> g`: Same as `x => g(f x)`
- `f << g`: Same as `x => f(g x)`.


An important concept in Ting is that of *definedness*. Some expressions may not be defined

    partial float x => 1 / x

This will

    float x \ 4*x^2 - 10*x - 50 == 0

This reads as "a float value `x` *such that* `4*x^2 - 10*x - 50 == 0` is satisfied". Because `x` is the value of the program, the compiler will attempt to solve the quadratic equation. In fact, given that this code has no side-effects, it will calculate the solution at compile time, erase the quadratic equation and simply generate code to output the solutions.



This is a program which will accept the coefficients from the command line:
    
    (float a, float b, float c) => partial nondeterm float x \ a*x^2 + b*x + c == 0

In this case the compiler will generate

The `partial` keyword is an instruction to the compiler that the function is *partial*. A partial function may not be defined for all values in its domain. In this case the domain is a tuple of 3 `float`s. As you probably know, a quadratic equation may have 0, 1 or 2 solutions.

    





During compilation, the compiler will look for libraries that can help solve the algebraic problems.

To help describe the problem Ting has an advanced type system which features both nominal and structural types, records and tuples, refinement types, dependent types, session types and more.

    Factorial = fn{ 0 -> 1, 1 -> 1, int n?>1 ~> n*Factorial(n-1) }


Although Ting has curly braces, it is not a "curly brace language". The curly braces of Ting is more related to set notation from mathematics than block structures from another programming language.

In Ting, types are referred to as *sets*. These are sets in the mathematical sense rather than the set datastructures you may know from other languages. The "type" of a function parameter is the set of values it can assume.

Types and functions are values themselves and can be passed to functions and operators and stored in variables. In fact, types in Ting are referred to as *sets*. 








Ting is a statically typed, compiled language. As a programmer you describe the problem and let the compiler deduce the solution. 

    Span..Quicksort = ...


## Operators

