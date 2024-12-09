

The following is an error, as the second `42` expression is completely shadowed by the first. 

    { 42, 42 }    


    { }


The `{` `}` set constructor eliminates nondeterminisn of the function point to generate a number of function points. 

    { int x \\ x > 0 }

The expression `int _ ? >0` is non-deterministic. It cannot be calculated to any one specific integer, indeed it is just *some* integer that is greater than 0. Used inside the set constructor `{` and `}`, the nondeterminism is eliminated by having the set contain 

Any set member (deterministic or generated) shadows following member expressions within the same set constructor. The following is a subset of integers

    Div3Or5 = { int x ? %% 3, int x ? %% 5 }

Some integers are divisible by both 3 and 5. Because the ...


So the definition is the same as writing 

    Div3Or5 = { int x ? %% 3, int x ? %% 5 ?! %% 3 }

Functions are sets of function points. How does this shadowing work when a function is constructed from function points? The answer to that lies in the *identity* of set members.

The identity of an integer member is the integer values itself. Hence, when used in a set constructor, an integer value will shadow the same value from any subsequent member expression.

The identity of a `->` function point is the identity of the left operand. Hence, within a set constructor a function point will shadow following function shadow points with the same argument. That is why the function `Div3Or5` above is deterministic.


The identity of a `<-` function point is the identity of the left (result) value. This means that a function created by `<-` (right-to-left) relations is potentially nondeterministic.

The following holds for any two sets X and Y:

    ex (X x, Y y) in { ~x <- y }  =  { ~x -> y } </>


The following is also a function, but the function points

    FibFirst = { 0 -> 0, 1 -> 1}



Customers..Orders;ex c\o