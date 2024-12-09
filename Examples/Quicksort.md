# Quicksort


## Non-in place Quicksort

    { (,,) -> (,,), (p,,m) -> this(m??<=p) + (p,,) + this(m??>p) }


Explanation:
1.  We know how to sort a very simple (empty) list: `() -> ()`.
2.  To sort any other list:
    1.  Take the head element - which we will call the *pivot* element - and seperate it from the tail: `(p,,m)`.
    2.  Divide the tail of the list into two lists: 
        1.  One with items less than or equal to the pivot element: `m??<=p`.
        2.  The other with the items that are greater than the pivot element: `m??>p`. 
    3.  Sort the two lists recursively. 
    4.  The result is the following 3 lists concatenated:
        1.  the sorted list of the items less than or equal to the pivot element: `self(m??<=p)`.
        2.  a list with the pivot element itself: `(p,,)`.
        3.  the sorted list of the items greater than the pivot element: `self(m??>p)`.


## In place quicksort


    a $ \.Qsort
    

    Arrays..SplitAt 

    Arrays..Qsort = ar \ {
        ar?.IsEmpty -> ar
        ar -> 
            let (left,right) = ar.Partition ar
            in left.Qsort ++ right.Qsort
    }

    Arrays..Partition = ar 
        let while (ar.Index l < ar.Index h) 

        let
            l = 0 ; while ( x \ x < hi && ar x <= p ) (+ 1)
            h = ar.Count-1 ; while ( x \ x > l && ar x >= p ) (- 1)
        in (l < h) 
            then This( ar.SwapAt(l,h), p, l + 1, h - 1 )
            else ar.ReplaceAt(hi,ar l).ReplaceAt(l,p).SplitAt p
    }

    // Each array has Swap method which returns an array with two items swapped
    Arrays..SwapAt = a => a.Index^2 (i,j) \ a.ReplaceAt(i, a j).ReplaceAt(j, a i)

    // Each array has a ReplaceAt method
    // which returns an array with one item replaced by a value
    Arrays..ReplaceAt = a \ ( a.Index i, a.Candidates item ) \ ...

    ar $ \SwapAt(x,y)


    
    (while) = let T:Type in 
        Set.Of(T+bool) cond  \  Set.Of (T+T) act  \  { T val -> cond val --> This(act val) --- val }
    


    Lists? Sum = Lists.Of float list  \  list.FoldLeft(+,0)

    