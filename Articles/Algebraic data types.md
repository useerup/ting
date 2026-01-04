

Reference: https://flix.dev/about/


## Algebraic types


    let 
        // An algebraic data type for shapes.
        Shapes = class rec { Circle:float, Square:float, Rectangle:float*float }

        // Computes the area of the given shape.
        Shapes..Area = fn {
            Circle r -> Math.Pi*r^2
            Square w -> w^2
            Rectangle (w,h) -> w*h
        }

    in
        // Computes the area of a 2 by 4.
        Rectangle(2, 4).Area @ Console.WriteLn;

## Parametric polymorphic types

    let

        // An algebraic data type for binary trees where each leaf carries a value of type T.
        Trees = Sets T => enum {
            Leaf of T ? 
                x => x.ToString override = $"Leaf {x}"
            Node of Trees.Of T * Trees.Of T
        }

        // A ToString instance for Tree.
        Sets all T=>(Trees T)..ToString override = {
            Leaf a -> $"Leaf({a})"
            Node (l,r) -> $"Node({l},{r})"
        }

        // A higher-order function that transforms a tree with
        // elements of type T to a tree with elements of type U.
        Trees..Map = let (Sets T, Sets U) in Tree T t \ ~{T+U} f \ t @ {
            Leaf x -> Leaf (f(x))
            Node (l, r) -> Node(This l, This r)
        }

        // Returns a simple tree with two leafs.
        tree = Trees int Node(Leaf 1, Leaf2)

    in

        // Squares all elements in the simple tree.
        tree.Map (x\x*x) @ Console.WriteLn

