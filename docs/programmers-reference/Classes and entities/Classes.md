## Classes

A regular set is *inclusive*: It has as elements **any object/value** that satisfies the set condition. Contrary to this, a class is *exclusive*: It contains **only those elements** that have been explicitly created as instances of the class.

A class is based on a *candidate set*. All members of a class must be members of the class' candidate set, but not all members of the candidate set are members of the class. A class is itself a set, namely the set of all of the members of the class. 

> Note: Unlike many object-oriented languages a *class* does not have to be name-structured. You can create a class based on any set, including sets of simple values, such as `int`, sets of tuples, sets of records or sets of lists. 

### The `class` operator

The `class` prefix operator is used to define new classes. It accepts a function as its operand. This function is the *class constructor*. The *codomain* of the constructor function is *candidate set* of the class.

    Meters = class float m => m

This expression binds `Meters` to be a class of `float`s.

However, any set used where a function is expected (such as the operand of `class`) is automatically promoted to its own identity function. So you will frequently see class definition simply based on a set. The above example could be written simply as

    Meters = class float

Another class definition could be 

    Amounts = class float

`Meters` and `Amounts` are two distinct classes. You cannot pass a member of `Amounts` where a member of `Meters` is expected and vice versa, even though they are both also subsets of `float`.





### Creating instances

Members of a class are created using the class' constructor

    zaphod = new Person("Zaphod", "Beeblebrox") 


### Extending classes with subclasses

The `extend` operator extends a class with a subclass. The left operand is the *base*, the right operand is the *extension*. The base **must** be a class while the right operand **must** be a set. The resulting class is based on the base class left-intersected with the extension set, but where the resulting class is considered a *subclass* of the base class.

    Person = class { @(Name:string)  }

    Employee = Person extend record(EmployeeNumber:string, EmploymentStart:DateTime) => base(EmployeeNumber),,


### Abstract classes

    Actor = class records
        DisplayName:string

    PersonActor = Actor extend f => arg => f arg @ record
        FirstName:string
        LastName:string
        DisplayName override = $"{FirstName} {LastName}" 
        
    CorporateActor = Actor extend @@ records
        CorporateName:string
        DisplayName override = CorporateName




### The set of all classes (`Classes`)

The `Classes` built-in set contains all classes. Like the set of all sets `Sets`, the set of all `Classes` is not enumerable, i.e. you can only use it to test for membership.



### Examples

This declares `Amount` as a class based on the *decimal* set:

    Amount = class decimal

Given this declaration the value `12.34` is **not** a member of `Amount`, but `(new Amount 12.34)` **is**.

This declares a `Currencies` class:

    Currencies = class string??.(Length==3)
    
    Dollar = new Currencies "USD"
    Euro = new Currencies "EUR"

This declares a `CurrencyAmounts` class:
    
    CurrencyAmounts = class (decimal a, Currencies c) => rec { Amount=a, Currency=c } 

