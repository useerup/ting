Being a logic language, Ting uses logic expressions to define the *model* of the state. 

    Customers = records
        identity CustomerNo : string
        Name : string

    Products = records 
        identity ProductNo : string
        Name : string
        Description : string
        UnitPrice : decimal

    Orders = records
        identity OrderNo : string
        Customer : Customers
        Items : records
            Product : Products
            Quantity : int
            ItemTotal = Product.UnitPrice * Quantity
        OrderTotal = Items.Sum(.ItemTotal)

This declares 3 sets: `Customers`, `Products`, `Orders`. These are all sets of *records*. *Sets* are the *types*; not data structures. Together these 3 sets 


    Customers.Update c with(Name="hh", Address="")



    Customers.Update c (with Name="Zaphod")


    
    Customers.Update = (Customers old, Customers.Candidates new) => 
        Customers oppropose .Replace(old, new)

    Customers -= c += c., c => c?.CustomerNo=="1000" with Name="Zaphod Beeblebrox" 
 
    Customers := c => c with mut Name \

        Name := ~




A Ting *system* will ensure that its state satis

A program in Ting is just an expression. The above code is in reality one big record expression, the fields of which are `CustomerNos`, `ProductNos`,... `Customers`, `Product` and `Orders`.

This describes the (beginning of) the state of an order fullfillment system. There is a number of rules here. For instance:
- CustomerNos = class string??.(Length:{1...10}) is a rule that declares `CustomerNos` as a class of strings with lengths between 1 and 10.
- `Customers = records ` says that `Customers` is a set of records, each record must satisfy the rules on the subsequent indented lines.
- `Customer : Customers` says that the `Customer` property of the `Orders` set must be a member of the `Customers` set.

During the lifetime of a system written in this language, the mandate values may be

I here use the term *system* to describe a program execution that stretches over time and

What is a *mandate value*? It is a value which must have a *proponent* to exist. Somebody must "vote" for the value before it can be "seated" (has a mandate). But as in every democratic system, we also may encounter *opponents*. Indeed opponents are crucial for

An object/value in the system may be a proponent or an opponent of any number of members of mandate sets. To exist it must be allowed to propose or oppose certain values. 

    Customers update .=(Name="Zaphod"))

To edit a customer in the mandate set `Customers` we need to *oppose* the "old" value (making sure that it is not around any more) and *propose* the "new" value (ensuring it is added back with a new value). A message about editing a customer may be handled by doing exactly this.

Events are finite sets that (in theory) contains all of the events that has happened to the system during its lifetime.

Note how `Product : Products` in the