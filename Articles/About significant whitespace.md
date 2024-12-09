

    { n <== n%3=0 & n%5=0 -> "FizzBuzz", n \\ n%3=0 -> "Fizz", n\\n%5=0 -> "Buzz", int n -> n.ToString }


    { 
        n \\ n%3=0 & n%5=0 -> "FizzBuzz",
        n \\ n%3=0 -> "Fizz",
        n \\ n%5=0 -> "Buzz",
        int n -> n.ToString
    }



    { "Alice", "Bob", "Charlie" }

    { 
        "Alice",
        "Bob",
        "Charlie"
    }


    Customers
        .Orders
        .Select
        .Consolidate

    
    Some =
        Others
        + Otherwise
        + Randoms
        + MyOwn

    Some =
        Others +
        Otherwise +
        Randoms +
        MyOwn


So, a binary operator will take precedence over same-indent line break. I can do that by eagerly eating whitespace whenever I match a binary operator:

    `+` = BinaryOperator("+",Add)

    BinaryOperator = (string sym, f) \ { MultiLineSpace _ >> Symbol sym >> MultiLineSpace _  <-  f }



    Sum = + >>> (1,2,3)



    