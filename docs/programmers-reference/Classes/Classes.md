## Classes

A class is based on a *candidate set*. A class contains elements from the candidate set that have been returned by the *class constructor*. Thus, members of a class **must** be explicitly created as members of that class.

A regular set is *inclusive* in that it has as elements any value that satisfies the set condition. A class is *exclusive* in that it contains only those elements that have been explicitly created through its constructor.

> Note: Unlike many object-oriented languages a *class* does not have to be name-structured. You can create a class based on any set, including built-in sets like `int`. 

The `class` prefix operator creates a new class based on the *candidate set*. 


### Creating instances

members of a class are created using the class' constructor

### Constructors

The `new` operator accepts a class as its argument and returns the constructor function for the class. If `Amount` is declared like this:

    Amount = class decimal

Then this will create a member of the `Amount` class:

    new Amount 12.34


### Extending classes with subclasses

The `extend` operator extends a class with a subclass. The left operand is the *base*, the right operand is the *extension*. The base **must** be a class while the right operand **must** be a set. The resulting class is based on the base class left-intersected with the extension set, but where the resulting class is considered a *subclass* of the base class.

    Persons = class rec { Name:string }

    Employee = Persons extend rec { EmployeeNumber:string, EmploymentStart:DateTime }


### Constructors


### Abstract classes

    Actor = abstract class rec { DisplayName:string abstract }

    PersonActor = Actor extend { 
        FirstName:string
        LastName:string
        DisplayName override = $"{FirstName} {LastName}" 
    }

    CorporateActor = Actor extend { 
        CorporateName:string
        DisplayName override = CorporateName
    }



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

