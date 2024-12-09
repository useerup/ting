
Haskell:

    data List a = Nil | Cons a (List a)

    data Tree a = Leaf | Bin a (Tree a) (Tree a)

Ting:


    List = { _ :+ This _, () }
    List.Of = T \ { List _ ? all :T }

    Tree = enum { Leaf, Node of these*these  }
    Tree.Of = set T \ enum { Leaf of T, Node of these*these}


