# Ting chess

The format of this file is an example of *literate programming* where markdown is combined with Ting code.

When a file name ends in `.ting.md` the compiler will only consider the indented code blocks. Therefore you can read this as annotated code as it is supposed to be.


### Chess board

We start by modeling the chess board which consists 64 squares created by a grid of *ranks* (rows) and *files* (columns). The ranks are numbered from 1 to 8 and the files are designated from *a* to *h*. The form a grid of *squares*. We model the rank and files as *classes*, and the squares as *tuples* of those:

        Rank = class { '1' ... '8' }
        File = class { 'a' ... 'h' }
        Square = File * Rank

By defining ranks and files as *classes*, we ensure that they are not mistaken for the characters that are used to *identify* them. We *could have* defined `File` as just `File = { 'a' ... 'h' }`, but then it would be a *subset* (*refinement type*, if you will) of the `char` set. Defining `Rank` and `File` as classes captures the concept, that within this chess domain, a *file* is something on a chess board that is *identified* by a character, but it is not *itself* a character.


### Board arithmetic

For convenience, we overload the `+` operator so that we can use it to add an integer to a rank or to a file to obtain a rank or a file relative to it. We do the same for squares; only for squares we will add a tuple of integers instead of a single integer:

        `_+_` override || >>>
            (Rank.ElementAt r, int dr)  =>  Rank.ElementAt( r + dr )
            (File.ElementAt f, int df)  =>  File.ElementAt( f + df )
            (Squares (f,r), (int df, int dr))  =>  (f + df, r + dr )

Note the use of `ElementAt` to bind the index in the argument definitions. All lists and ordered sets (`Rank` and `File` are ordered sets) has an `ElementAt` method. This method accepts a zero-based index into the list/set and returns the element in that position.

Because `Rank.ElementAt r` *returns* a `Rank` member and it is used to define part of the tuple parameter, the actual parameter will be unified with that `Rank` element. The local parameter `r` is not bound to the element; rather it is bound to *the index* of the element. This is an example of using a function in "reverse".


### Players

The players are identified by a discriminated union:

        Player = enum { White, Black } 

For convenience we also define a *member method* for calculating the opponent of a given player. This allows us to write `p.Opponent` when `p` is a member of the `Player` set:

        Player..Opponent = fn{ Black->White, White->Black }

Declaring `Player..Opponent` like this declares `Opponent` as a property on all *members* of `Player`. Accessed this way through `..` the property must be bound to a function which accepts the `Player` member as an argument. The argument of that function again relies on the fact that we can declare the argument as `Player.ElementAt i` to let the function operate on the *index* of the member within `Player`. The `Player` set has only 2 members at index `0` and index `1`, so `Player.ElementAt(1-i)` will return the member with the *other index*, i.e. the other player.


### Game state

        GameState = record {  
            Board : Square --> Player * PieceType
            Castling : Player --> record{ KingSide, QueenSide }
            EnPassant : Maybe Square
        }


        EnPassant |> fn{
            Some sq ->
            None void ->
        }

### Steps

We define a *step* as a single unit move in some direction. 

        Steps = int*int

        Steps.(
            Rook = { (-1;1,0), (0,-1;1) }
            Bishop = { (-1;1,-1;1) }
            Royal = Rook | Bishop
            Knight = { (-2;2, -1;1), (-1;1, -2;2) }
        )

We leave out the pawn moves as they are more complicated:
- the direction a pawn may move depends on the player (color)
- a pawn may move 1 or 2 steps initially and only one step thereafter
- a pawn may not strike in the direction it moves
- a special condition arises when a pawn moves 2 steps *en passant* an opposing pawn

We will implement these moves when we implement the individual piece types.


### Moves

        Move = (GameState game) => 
            let piece : sit.Pieces ?? .(Player=sit.PlayerInTurn) in


        VectorMove = GameState state => 
            Dir dir => 
                let this ~ Square sq =>
                    let sq=sq+dir in state.Board(sq) |> fn{
                        Free -> Move(sq) ; this(sq)
                        Piece(_,Player.Opponent) -> Strike(sq)
                    }

        VectorMove =  => sq+dir |>
            let this ~ state.Board  fn { 
            sq => sq.Free then Move(sq);this(sq+dir) 
            piece # record Player==player --> undefined
            piece # record Player==player.Opponent --> Strike(piece)
        } 


### Pieces

        State state

        King = Piece && record {
            Move = SingleMove AnyDirection
            Strike = SingleMove AnyDirection
        }

        Pawn = Piece && record {
            Move = Player player -> { let dir = if player==White then 1 else -1 in  }
            Strike = PawnStrike
        }

        PawnStrike = with player:Player in 
            (State state, position) ? state.Board position is Pawn player  -> ...


A *situation* is a state in the game characterized by

        PiecePlacements = Square=>Player*PieceType
        Castlings = enum{ KingSide, QueenSide }

        BoardPosition = record {
            Placements: PiecePlacements
        }

        {
            Situations ( sit + {square->(color,piece)} ) => piece.Moves(sit,square,color)
        }


        var
            Piece = { record(Moves : ..., Strikes : ...) }
            
            King : Piece
            Queen : Piece
            Rook : Piece
            Bishop : {Piece} {
                Moves = (Square s,Board b) => { (DiagonalMoves m,s) |> ((m,s) => s+m;this(m,s+m)) }
            }
            Knight : Piece
            Pawn : Piece
            
            Board = Square --> Player * PieceType
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




