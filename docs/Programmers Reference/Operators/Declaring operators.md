### Declaring operators

#### The `let` operator

The `let` operator is a prefix operator. It introduces a new local, declaretive scope that spans its operand. It has the same value as its operand expression. 

Let is often used in combination with the `in` operator, but doesn't have to be.

`let a = 2 in a*2` is an expression which evaluates to `4`.


#### `public`

    public Fibonacci = { int arg => int res mode bound arg | bound res }

#### `protected`

#### `internal`

#### `private`


#### `flow`

The `flow` operator describes the possible bindings and flows a function supports.

If an exported function does not have an explicitly set mode proposition, it is assumed to be *regular flow*, which means that the the only flow is bound argument and unbound (free) result

The `OrderedSets..ElementAt` is 

    OrderedSets..ElementAt : this => int index => set element flow bound(set,index) | bound(set,element) \ || >>>
            bound(index) then set.ElementAt set index
            bound(element) then index = set.IndexOf(element)
                                

        
            

