
# See https://github.com/koka-lang/koka/blob/master/test/bench/haskell/rbtree.hs


    import Color = enum { Red, Black } 

    import Tree = enum { Leaf, Node of Color * Tree * int * bool * Tree }





    

    


A record *instance* is constructed using `(.` ... `.)`

A set of record can be created from one or more record instances:

    Records = { (. Name="Zaphod", Age=42 .), (. Name="Arthur", Age=45 .) }

    Passengers = { (. Name:string, Age:int??>0 .) }

    Passengers = {. Name:string, Age:int??>0 .}


    ManifestPerson = {| Passenger, Captain, FlightAttendant |}



    A single record (name-structured tuple):

        (. Name="Zaphpod Beeblebrox", Address="Betelgeuse IV", Salary:100000m .)

    A set of records:

        { (. Name:string, Address:string, Salary:decimal .) }

        {. Name:string, Address:string, Salary:decimal .}



    [  ]