# Enumerations

Tagged values are constructed using `(|` ... `|)`. Inside the constructor an expression list enumerated alternatives (at least one)

    (| Black |)

    (| Red, Blue, Green |)

The latter is a value wich is a *choice* between `Red`, `Blue` and `Green`.

All of the following are equivalent

    { (|Black|), (|Red,Blue,Green|) }

    { (| Black,Red,Blue,Green |) }

A short form

    {| Black,Red,Blue,Green |}


The tagged value creates


The identifiers of tagged values can be optionally followed by a predicate

    Ranks = {| Ace, King, Queen, Jack, Number:{2...10} |}

    r:Rank

    r.Ace       // the `Ace` value
    r.Number    // A function from `2...10` to a value tagged with `Number`

    value = r |> Ranks./ {
        Ace -> 14
        King -> 13
        Queen -> 12
        Jack -> 11
        val Number n -> n
    }

    r = RanksNumber 5


