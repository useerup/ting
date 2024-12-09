# Pets

(inspired by https://www.reddit.com/r/ProgrammingLanguages/comments/yo3937/dry_a_simple_dynamicallytyped_programming/)

    Pet = class {[MakeSound <= Unit+bool]}

    Ducks = Pet extend {\\ MakeSound = () \ WriteLn "Quack" }

    Duck = new Ducks

    Denji = new Pet with MakeSound = () \ WriteLn "Woof"

