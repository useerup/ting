# State management in Ting



    Customers = {.
        AddOrder = Order o => this with Orders .Add o
    .}

    do (c=customer) in
        c.=AddOrder o1
        c.=AddOrder o2
        c.=AddOrder o3

    c : Customers mut=(.Name="Zaphod Beeblebrox",Orders=... .)
    



## The `do` operator

The do operator delimits states. The left operand is a *transitional* expression. The right operand is a post-transitional expression.

    
    l := + 1 do l

