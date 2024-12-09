# The Ting programming language

*Ting* is a modern programming language which unifies functional programming, object-oriented programming and logic programming into a simple, elegant and powerful programming language.

## Motivating example

    Suits = {| Hearts, Clubs, Spades, Diamonds |}
    Ranks = [2...10] | {| Jack, Queen, King, Ace |}
    Cards = Suits*Ranks
    
    Deck = Shuffle (mandate Cards)


    Shuffle = {
        (picked,{}) -> picked
        (picked,rest) -> picked $ .Add
    }

    with rnd = Random.New
    List.Draw = rnd.

    





## Sets and classes

Sets are fundamental to Ting. 

The standard sets:

|          |     |
| -------- | --- |
| `bool`   |     |
| `char`   |     |
| `string` |     |
| `int`    |     |
| `short`  |     |
| `long`   |     |
| `byte`   |     |
| `sbyte`  |     |
| `ushort` |     |
| `uint`   |     |
| `ulong`  |     |
| `Guid`       |     |
| `DateTime`       |     |
| `DateTimeOffset`       |     |
| `DateOnly`       |     |
| `TimeOnly`       |     |
| `float`       |     |
| `double`       |     |
| `Half`       |     |
| ``       |     |
| ``       |     |
| ``       |     |
| ``       |     |


- `float`
- `double`
- `half`
- `decimal`

- `string`
- `string`

- `System.DateTime`
- `System.DateTimeOffset`
- `System.DateOnly`
- `System.TimeOnly`
- `System.TimeSpan`


