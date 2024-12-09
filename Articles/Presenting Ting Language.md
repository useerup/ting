### Algebraic types

    Tuple = Strings * Strings
    Predicates = Doubles ** Booleans

    UnaryDoubleFunctions = Doubles ** Doubles
    BinaryDoubleFunctions = Doubles * Doubles ** Doubles

    Point2Ds = class Doubles~X * Doubles~Y
    Triangles = class (Point2Ds.Combine 3)~Corners


### Refinement types

    Ints = Int32s

    NegativeInts = Ints.Where(<0)

    NonNegativeInts = Ints & !NegativeInts

    StringsStartingWithA = Strings.Where( ./ StartsWith "A" )

`NegativeInts` is defined as the set of elements of `Ints` that compares to less than 0.

Most binary operators, such as `<`, can be used in prefix positions as well as infix. In that case the operator is a partial evaluation of the binary operator. Thus, `<0` is a partial evaluation of `<` and returns a function which accepts an `Ints` member and returns true if that number is less than 0.

`!NegativeInts` is the complementary set of `NegativeInts`. This set contains any element that *is not* a member of `NegativeInts`. However, this includes all object that are also not `Ints`. To obtain the set of `Ints` members that are not member of `NegativeInts` we *intersect* `Ints` with `!NegativeInts` to obtain the set `NonNegativeInts`.




### Dependent types

Lists are inherently dependent types

    Lists.Of = Sets s => [] || [s x ,, Lists.Of s]
    Lists..Concat = left  =>  []  =>  left  ||  left => [head,,tail] => (left.Append head).Concat tail





    myInts : Lists.Of int32


    



