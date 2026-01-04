Ting is a logic programming language with no working compiler (yet). In this post I will describe the type system. 

I would love to get some feedback/criticism on the following topics:

- Readability of the code
- Choice of syntax, specifically the multi-character delimiters

The type system is rather advanced. It features 
- Structural and nominal types
- Union types, intersection types 
- Sum types / discriminated unions
- Product types 
- Refinement types and depedent types

Types and functions are first class citizens, which means that they are values which can be used in corresponding arithmetic and logical functions, passed as parameters etc.

---
# Sets

In Ting, the types are called *sets*. Crucially, a Ting set is **not** a data structure. It is more closely related to the sets from math.

An example of a simple set (type) defined by listing elements:

    SomeNumbers = { 1, 2, 3 }

Now consider:

    n : SomeNumbers

Here, `n` can only assume one of the values `1`, `2`, or `3`.

The definition of `SomeNumbers` is an example of an *extensional set definition*: It lists each element of the set, i.e., each possible value of the type.

A somewhat related example is this:

    EvenNumbers = { int x \ x % 2 == 0 }

Here, the expression `int x \ x % 2 == 0` is *non-deterministic*. It doesn't have a single, fixed value like `1`; rather, it can assume a number of different values. In Ting, a set construction *unwinds* such non-determinism and constructs a set (type) containing all of those values.

This *intensional set definition* is really only a special form of the above *extensional set definition*: Each element in the list of members can be non-deterministic.

The *range* operator `...` accepts two operands and returns a non-deterministic value constrained to the range with both operands inclusive. Excluding operators also exists. Like any other non-deterministic value, this can be used in a set definition:

    Digits = { '0'...'9' }

    ScreenXCoordinates = { 0 ...< 1920 }
    ScreenYCoordinated = { 0 ...< 1080 }


**Built-in sets**

A number of sets are built-in. Among those are:
- `string`: The set of all Unicode strings
- `char` the set of all Unicode characters
- `bool` the set of values `{ false, true }`
- `int`: The set of all 32-bit integers
- `float`: The set of all 32-bit IEEE-754 floating point numbers.
- `double`: The set of all 64-bit IEEE-754 floating point numbers.
- `decimal`: The set of all 128-bit decimal numbers.

---

# Tuples

This is a tuple *instance*:

    MyBox = (10, 20, 15)  // Width, Height, Depth

This is a *set* of tuples:

    Dimensions = { (float _, float _, float _) }

Or:

    Dimensions = float*float*float      // Multiplying sets creates tuples

Or simply (by *set arithmetic*):

    Dimensions = float^3        // Same as float*float*float


---

# Records

This is a record instance:

    President = (. Name="Zaphod Beeblebrox III", Age=42 .)

The delimiters `(.` ... `.)` construct a record instance. In this case, the fields `Name` and `Age` are both *non-deterministic*. Thus, creating a set of such a non-deterministic record creates a set of all possible such records.

The rationale behind the choice of combined symbols `(.` and `.)` is that the period should help associate the syntax with records, in which `.` is used to access properties/fields. If you dislike this, then hold on to your marbles when you read about discriminated unions and function constructors below 😱.

A record does not have to be created explicitly as part of any set. The expression list between `(.` and `.)` is a list of propositions which must all be `true` for the record instance to exist. A valid record is one in which the identifiers are bound to values which satisfy all of the propositions. In this case it is pretty straightforward to make these propositions true: Just bind the field `Name` and `Age` to the corresonding values.

However, even a record instance can be *non-deterministic*, like for instance:

    (. Name:string, Age:int .)

This record can assume the value `(. Name="Zaphod Beeblebrox III", Age=42 .)` or `(. Name="Ford Prefect", Age=41 .)` or infinitely many other values.

By following the aforementioned set constructor, this constructs a set of records:

    Persons = { (. Name:string, Age:int .) }

Syntactically (sugar), Ting allows this to be shortened:

    Persons = {. Name:string, Age:int .}

I.e., I also allow the `.` modifier to be used on a combined set symbol. In that case, it is not possible to list multiple elements, as each expression is now a definition of a record field.

---

# Classes and nominal types

By default, sets are *inclusive*: they contain all values that satisfy the set condition. In that sense, such sets represent *structural types*: A member of a given set does not have to be constructed explicitly as a member or inhabitant; if it fits the criteria, it is a member.

So what about *nominal types*?

The answer in Ting is *classes*. Unlike many other languages, a *class* in Ting does not presume anything about structure, representation, reference, or allocation/deallocation semantics.

A Ting class is constructed from a *candidate set*. This candidate set can be a set of simple values (like `int`, `float`, or `string`), or a set of structured values like sets of tuples or sets of records.

The `class` keyword constructs a unique class from the candidate set:

    Latitude = class { float -90...90 }

    Longitude = class { float -180...180 }

To be a member of a class, a value must be constructed as a member of that class *explicitly*:

    lat = Latitude 40.71427
    lon = Longitude -74.00597

All members of a class are still members of the *candidate set*. This means that it is possible to perform arithmetic on `Latitudes` and `Longitudes`. However, unless the functions/operators performing the arithmetic have been overloaded to support returning `Latitudes` and `Longitudes`, they will return members of the *candidate set* and will need to be converted back to class members.

    north = Latitude( lat + 10 )

Here, `+` works because `lat` is a member of `Latitude`, where all members are also `float` members. `+` is defined for `float*float` and will return a `float`.

Classes are themselves sets. New inclusive sets or classes can be constructed based on classes:

    // A tuple of longitude and latitude is a coordinate
    Coordinates = Latitudes*Longitudes     

---

# Discriminated unions / disjoint unions

A simple discriminated union is:

    Colors = {| Red, Green, Blue |}

`Colors` is a set of 3 symbolic values, denoted by `Colors.Red`, `Colors.Green` and `Colors.Blue`.

Values can be associated with the symbolic values:

    Shapes = {| 
        Circle of float             // radius
        Rectangle of float*float    // 2 sides
        Triangle of float^3         // 3 sides
    |}


---

# Functions

Functions are first class citizens in Ting. Every function is itself a value which can be stored, passed etc.

The simplest way to create a function in Ting is through the *lambda arrow*:

    Double = float x -> x * 2

The `Double` identifier is bound to the function `float x -> x * 2`.

Because a function is a value, it is akin to a tuple or record *instances*. In other words, a function is an instance, which - like simple values, record and tuple instances - can be a member of a number of a number of sets.

This describes the set of all binary functions on `float`:

    BinaryFloatFunctions = float*float=>float

The `=>` operator accepts a left hand *domain* and a right hand *codomain* and returns the set of all functions from the domain to the codomain. In this case the domain is `float*float` and the codomain is `float`.

`BinaryFloatFunctions` is thus a *set* (type) of all functions which accepts a tuple of two `float`s and returns a `float`. The above `Double` function belongs to this set.

Underlying each function is a set of *ordered pairs*. This set of ordered pairs can be accessed through the `.AsOrderedPairs` property of any function. 

An ordered pair is very much like a tuple, but it has slightly different *identity*: The identity of a tuple is that of the combined identity of all the components. The identity of an ordered pair is the identity of the domain component, disregarding the codomain component.

The syntax for explicitly creating an ordered pair is

    origin --> target

A function can be created by specifying the ordered pairs explicitly in a set-like notation. 

    Factorial = {>  0 --> 1, int n?>0 --> n * this(n-1)  <}

The delimiters `{>` ... `<}` constructs a function from a set of ordered pairs.

    Fibonacci = {>  0-->1, 1-->1, int n?>1-->this(n-2)+this(n-1)  <}

Or formatted on multiple lines:

    Fibonacci = 
    {>  
        0 --> 1
        1 --> 1
        int n?>1 --> this(n-2)+this(n-1)  
    <}

Alternative ways to write the `Fibonacci` function:

    Fibonacci = 
        (0 -> 1) ||
        (1 -> 1) || 
        (int n?>1 -> Fibonacci(n-2)+Fibonacci(n-1))

or

    Fibonacci = (int n?>=0 -> n==0||n==1 then 1 else Fibonacci(n-2)+Fibonacci(n-1))


The rationale behind choosing the multi-character delimiters `{>` and `<}` is that the `>` and `<` "modifiers" should lead the user to think of functions, for which `>` is an essential character for constructing lambda arrows.

**Definedness, total and partial functions**

The tight type system allows Ting to be really explicit about the values for which a function is defined. The domain of `/` for floats, is `(float??!=0,float)`.

For intra-module functions the compiler will - if possible - infer the domains of function itself. However, in a number of cases the compiler will not be able to infer the domain, but may be able to *check* a user-supplied domain.

Consider this function:

    f = float x -> 1 / (1-x)

In this case the compiler may be able to infer that `(1-x)` may produce a value `0` for which `/` is not defined. Depending on how this function is used, the compiler may be able to check that it is never invoked with the value `1`, and hence that the program is safe.

However, if the compiler *is not* able to infer backwards, the compiler will throw a compiler error. The user should be able to overcome such a compiler error by specifying 

    f = float x?!=1 -> 1 / (1-x)

A function which returns a result for every value in its domain is a *total function*.

A function which may not return a result for a given argument is a *partial function*. 

Consider for instance a function which given a filename returns the contents of the file. If the file does not exist at runtime, the function is undefined for the given filename. The compiler has no way of knowing this at compile time. Thus, such a function is marked as *partial* because while it is defined for all filenames, only a subset of those will actually return file contents.


**Composing functions**

The operators >> and << combines two function into one by chaining them. Essentially `f >> g` is the same as `x -> g(f(x))` and `f << g` is the same as `x -> f(g(x))`

But there are other ways to combine functions.

`||` works on functions. `f || g` returns a function which given an argument `x` returns `f x` if and only if `f` is defined for `x`, otherwise it returns `g x`.

Consider a function `File.ReadAllText` which given a filename returns all text from the file. This function is *partial*. Invoking a partial function without handling it may lead to errors. 

However we can combine with a function which simply returns an empty string:

    File.ReadAllTextOrEmpty = File.ReadAllText || (string _ -> "")

This function is not partial: It will always return a string. When the file with the name does not exist, it simply returns an empty string.

Likewise `f && g` returns a function which is only defined for a given `x` if both `f` and `g` are defined for `x`.


# Refinement types

To refine the `int` type using the filter operator `??`, we can define a subset of integers that satisfy a specific condition. Here's an example:

    PositiveIntegers = int??>0
    EvenIntegers = int??(x->x%2==0)

Similarly:

    StartingWithA = strings??./StartsWith "A"

