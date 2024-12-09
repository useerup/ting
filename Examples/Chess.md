# Ting chess

*Ting* is the language I am designing. Ting is a logic programming language which unifies functional and object-oriented programming. As such, it sports refinement types, dependent types, subtyping, abstractions (overridable properties/methods). As a logic programming language, functions are more like relations that they are imperative code. In the same way as Prolog, a the compiler will often be able to figure out the inverse of a function when it knows the function result but have to figure out which arguments corresponds to the result.

This is the beginning of an example of the game of chess implemented in Ting. I chose this rather challenging use case because 

1) Chess is a fascinating
2) I am writing the compiler partly in the Ting language itself, and I want to avoid falling into the trap of just creating a language that is good for writing compilers and not much else.

It is **not yet** a running example because 1) the example is **not complete** and (worse) 2)  **no compiler for the language exists yet**. I apologize, but the compiler is a challenging work in progress. When completed, this example will act as one of the tests for the compiler.

So, for now this is entirely conceptual, so you will need to excercise you abstract thinking skills.

The format of this file is an example of *literate programming* where markdown is combined with Ting code.

When a file name ends in `.ting.md` the compiler will only consider the indented code blocks. Therefore you can read this as annotated code as it is supposed to be.


### Chess board

We start by modeling the chess board which consists 64 squares created by a grid of *ranks* (rows) and *files* (columns). The ranks are numbered from 1 to 8 and the files are designated from *a* to *h*. The form a grid of *squares*. We model the rank and files as *classes*, and the squares as *tuples* of those:

    let 
        Rank = class { '1' ... '8' }
        File = class { 'a' ... 'h' }
        Square = File * Rank


By defining ranks and files as *classes*, we ensure that they are not mistaken for the characters that are used to *identify* them. This captures the concept, that within the chess domain, a *file* is something on a chess board that is *identified* by a character, but it is not *itself* a character.

For convenience, we overload the `+` operator so that we can use it to add an integer to a rank or to a file to obtain a rank or a file relative to it. We do the same for squares; only for squares should be able to add a tuple of integers instead of a single integer:

    `_+_` override || >>>
        (Rank.ElementAt r, int dr)  =>  Rank.ElementAt( r + dr )
        (File.ElementAt f, int df)  =>  File.ElementAt( f + df )
        (Squares (f,r), (int df, int dr))  =>  Square( f + df, r + dr )

Note the elegant use of `ElementAt` to bind the index in the argument definitions.
    
### Players

The players are identified by a discriminated union. For convenience we also define a *member method* for calculating the *opponent* of a given player. This allows us to write `p.Opponent` when `p` is a member of the `Player` set:

    let 
        Player = {| White, Black |} 
        Player..Opponent = Player ./ {^ Black -> White, White -> Black ^}


### Pieces

    let
        Piece = {.
            Move = ...
        .} 
        King : Piece
        Queen : Piece
        Rook : Piece
        Bishop : Piece
        Knight : Piece
        Pawn : Piece
        

        Board = Square ^^ Player * PieceType
        Situation = ...

    let
        f = (Situation s) => { s.Board } let(player,piece) = (board sq)


### Moves

Some pieces can perform *vector moves*. A vector move is a move that follow an axis across multiple squares until the bounds of the board or until a square is occupied. This is the way the queen, rooks and bishops move. A vector move can be said to be comprised of an initial vector applied multiple times. Thus, we describe these

We describe these vectors in terms of the *vector steps* they are comprised of. 

    let 
        VectorSteps = {|
            Diagonal = { (-1,-1), (-1,1), (1,-1), (1,1) }
            Orthogonal  = { (0,-1), (0,1), (-1,0), (1,0) }
            Star = Diagonal | Orthogonal
        |}

    Board (pos,(~player,piece)) => 




