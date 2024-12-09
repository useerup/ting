# Test examples


    42

    let x = 42 in x


    {
        Zark = 42
    }




    {

        Foo = "Foo"

        Zark = {

        } extend {

        }


        Bar = "Bar"
    }


    Tree = enum { Leaf, Node of Tree*Tree }

    let t = Tree (Node(42,Leaf 0,Lead 1))
