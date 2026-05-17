### Projection operators

#### The `.` property projection operator

The operator is a binary operator. On the left it accepts an expression which has *properties*. On the right it accepts an expression that declares identifiers.

In referential context the RHS defines which properties are selected from the LHS.

    customer.Orders
    
    customer.(Orders)

    customer.(Orders<=AllOrders)

##### `.` in declarative position

In declarative position the RHS *declares* properties on the LHS value.

    public customer.CompletedOrdersTotal = customer.Orders.Where(.Completed).Total

    public customer.Orders.CompletedTotal = customer.Orders.Where(.Completed).Total

    


##### `.` as prefix operator

As with all binary operators you can use the `.` operator in *prefix position*.

The following expression is a function which accepts an object which has an `Amount` property and returns the value of that property.

    .Amount


The following  expression is a function which accepts an object which has a property `Amount` which again must be a member of the `decimal` set. It returns the value of the `Amount` property, which is guaranteed to be a member of the `decimal` set.

    .(decimal Amount)

The ability to easily create a function to a property is handy when for instance you want to calculate the sum over a property of a list or set:

    ordersTotal = customer.Orders.Sum(.Total)

Note how `.Total` is used instead of `x=>x.Total` or similar.


The following expression is a function which accepts an object which has a property `Orders` which again is a list of `AllOrders` elements. The function returns the sum of the `Amount` property over all of the `Orders` elements.

    x?.(Orders:AllOrders.Lists) => x.Orders.Sum(.Amount)

Here the `.` operator is used in prefix position twice:
1. `.(Orders:AllOrders.Lists)`: This is the RHS of `?`. It is a function that restricts the LHS of `?` to objects that has an `Orders` property which must be a list of `AllOrders` elements.
2. `.Amount`: This is a function which selects the `Amount` property from the argument.

### The `..` element property projection operator

The `..` operator selects a property from the *elements/items* of a set or sequence. It returns a *function* from an element/item of the set/sequence to the property of that element/item.

Given a set `Customers` in which each element has a property `FirstName` these two expressions are equivalent:
1. `Customers c => c.FirstName`
2. `Customers..FirstName`

##### `..` in declarative context

In declarative context the `..` operator *declares* a property on each element/item of the LHS argument.

The following example declares a `FullName` on each element of the `Customers` set where the FullName is calculated as `FirstName`, `MiddleName` and `LastName` concatenated with `" "` as separator, i.e. *string joined*.

    public Customers..FullName = c => string.Join(" ",[c.FirstName,c.MiddleName,c.LastName])

With the above declaration, any object known to be a member of `Customers` now has a `FullName` property:

    let c:Customers in c.FullName


##### `..` as prefix operator

As with all binary operators you can use the `..` operator in *prefix position*.

The following expression is a function which accepts a set or sequence whose elements have a `FullName` property and returns a projection of that property from each element.

The following expression is a function which accepts a set or sequence whose elements have a property `FullName` which is a member of the `string` set. It returns a projection of the `FullName` property from each element.

    ..(string..UpperCase FullName) >> .UpperCase

