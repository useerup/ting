# Pets

(inspired by https://www.reddit.com/r/ProgrammingLanguages/comments/yo3937/dry_a_simple_dynamicallytyped_programming/)

    Pet = class record { MakeSound : Unit-->bool }

    Ducks = Pet extend { MakeSound = () => WriteLn "Quack" }

    Duck = Ducks ()

    Denji = new Pet extend { MakeSound = () => WriteLn "Woof" }

