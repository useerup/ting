(see https://docs.scala-lang.org/overviews/scala-book/enumerations-pizza-class.html)


    Topping = enum {
        Cheese
        Pepperoni
        Sausage
        Mushrooms
        Onions
    }

    CrustSize = enum {
        Small
        Medium
        Large
    }

    CrustType = enum {
        Regular
        Thin
        Thick
    }

    Pizza = class {.
        CrustSize crustSize
        CrustType crustType
        Lists.Of Toppings toppings
        public AddTopping = toppings.Add
    .}

    
    let p = new Pizza()
        .AddTopping Cheese
        .AddTopping Pepperoni
    in 
        do Console.WriteLine p





    (:=) = lhs \ rhs \ Environment env \ env.Assign lhs rhs