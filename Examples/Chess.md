# Ting chess

The format of this file is an example of *literate programming* where markdown is combined with Ting code.

When a file name ends in `.ting.md` the compiler will only consider the indented code blocks. Therefore you can read this as annotated code as it is supposed to be.


### Chess board

We start by modeling the chess board which consists 64 squares created by a grid of *ranks* (rows) and *files* (columns). The ranks are numbered from 1 to 8 and the files are designated from *a* to *h*. They form a grid of *squares*:

        Rank = class { '1' ... '8' }
        File = class { 'a' ... 'h' }
        Square = File * Rank

By defining ranks and files as *classes*, we ensure that they are not mistaken for the characters that are used to *identify* them. We *could have* defined `File` as just `File = { 'a' ... 'h' }`, but then it would be a *subset* (*refinement type*, if you will) of the `char` set. Defining `Rank` and `File` as classes captures the idea, that within this chess domain, a *file* is something on a chess board that is *identified* by a character, but it is not *itself* a character.


### Board arithmetic

For convenience, we overload the `+` operator so that we can use it to add an integer to a rank or to a file to obtain a rank or a file relative to it. We do the same for squares; only for squares we will add a tuple of integers instead of a single integer. Given a rank `r` we can then write `r + 1` to get next rank. In Ting all operators has an underlying function. We overload an operator by overriding this underlying function. The function underlying the binary `+` operator has the name `_+_`.

        `_+_` override f => Rank.Add || File.Add || Square.Add || f

We override a virtual object like `_+_` through the `override` operator. This operator overrides (re-binds) the identifiers declared by the left hand operator. It expects a function as it right hand operator. This function must accept the *current* value (here represented by `f`) and returns the *new* value to which the identifiers are bound. As we want to *extend* the `+` operator we still include the current function (`f`) but we put it last in the chain of guarded function-unions to give preference to the new adding functions. 

We now define the actual adding functions referenced by the overriding expression:
        
        public 
            Rank.Add = (Rank.ElementAt r, int dr)  =>  Rank.ElementAt(r + dr)
            File.Add = (File.ElementAt f, int df)  =>  File.ElementAt(f + df)
            Square.Add = (Squares (f,r), (int df, int dr))  =>  (f + df, r + dr)



Note the use of `ElementAt` to bind the index in the argument definitions. All lists and ordered sets (`Rank` and `File` are ordered sets) has an `ElementAt` method. This method accepts a zero-based index into the list/set and returns the element in that position.

Because `Rank.ElementAt r` *returns* a `Rank` member and it is used to define part of the tuple parameter, the actual parameter will be unified with that `Rank` element. The local parameter `r` is not bound to the element; rather it is bound to *the index* of the element. This is an example of using a function in "reverse".


### Players

The players are identified by a discriminated union:

        Player = enums White, Black  

For convenience we also define a *member method* for getting the opponent of a given player. This allows us to write `p.Opponent` when `p` is a member of the `Player` set:
        
        Player..Opponent = Player ./ fn{ Black->White, White->Black }

Declaring `Player..Opponent` like this declares `Opponent` as a property on all *members* of `Player`. Accessed this way through `..` the property must be bound to a function which accepts the `Player` member as an argument. The `./` operator is used to "open" the scope of `Player` so that we can write `Black` instead of `Player.Black`.


### Game state

        GameState = records
            Board : Square --> Player*PieceType
            Castling : Player --> enums KingSide, QueenSide
            EnPassant : File option


### Steps

We define a *step* as a single unit move in some direction. 

        Steps = int*int

        Steps.(
            Rook = Situation sit => Square sq =>        { (-1;1,0), (0,-1;1) }
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
            piece : sit.Pieces ?? .(Player=sit.PlayerInTurn) in


        VectorMove = GameState state => 
            Dir dir => 
                this alias Square sq =>
                    let sq=sq+dir in state.Board(sq) |> fn {
                        Free -> Move(sq) ; this sq
                        Piece(_,Player.Opponent) -> Strike sq
                    }

        VectorMove =  => sq + dir |>
            this is state.Board  fn { 
                sq => sq.Free then Move(sq);this(sq+dir) 
                piece # record Player==player --> undefined
                piece # record Player==player.Opponent --> Strike(piece)
            } 


### Pieces

        State state

        King = Piece extend && records
            Move = SingleMove AnyDirection
            Strike = SingleMove AnyDirection

        Pawn = Piece && records
            Move = Player player -> { let dir = player==White then 1 else -1 }
            Strike = PawnStrike

        PawnStrike = with player : Player in 
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





----


You picked a fun one—NNUE is basically a nicely-behaved typed linear pipeline over a huge but very regular index set. Let’s make that explicit.



1. Core index sets as types

We start by turning the chess domain into finite types:

~~~latek
Pieces: [ \text{Piece} ::= {\text{Pawn}, \text{Knight}, \text{Bishop}, \text{Rook}, \text{Queen}, \text{King}} ]

Colors: [ \text{Color} ::= {\text{White}, \text{Black}} ]

Squares: [ \text{Square} ::= {0,1,\dots,63} ]

You can refine this further if you like (e.g. File, Rank with Square ≅ File × Rank), but for NNUE the flat 0–63 is fine.

2. Feature space as a dependent family

Conceptually, NNUE uses piece–square features for both sides, with some perspective trick (e.g. “side to move” perspective). A simplified but structurally faithful type view:

Raw feature index type: [ \text{FeatIdx} ::= \text{Color} \times \text{Piece} \times \text{Square} ]

Feature vector type: [ \text{FeatureVec} ;:=; \text{FeatIdx} \to \mathbb{R} ]

So a feature vector is a total function from all possible (color, piece, square) triples to a real value (usually 0 or some fixed activation when that piece is on that square).

To reflect actual positions rather than arbitrary vectors, you can refine:

Position: [ \text{Position} := {; p : \text{Square} \to \text{Option}(\text{Color} \times \text{Piece}) ;\mid; \text{legal}(p) ;} ]

Feature extraction as a dependent function: [ \mathsf{features} : \Pi (s : \text{Position}),; \text{FeatureVec} ]

So features is a dependent function: for each legal position s, it returns a feature vector consistent with that position.

If you want to encode the “side-to-move perspective” explicitly, you can refine:

Side to move: [ \text{STM} ::= {\text{WhiteToMove}, \text{BlackToMove}} ]

State: [ \text{State} := { (p : \text{Position},; stm : \text{STM}) } ]

Perspective-aware features: [ \mathsf{features} : \Pi (st : \text{State}),; \text{FeatureVec} ]

3. Layers as typed linear maps

Now we model the NNUE layers as typed linear maps between finite-dimensional vector spaces.

Let’s name the layer spaces:

Input space: [ V_0 := \text{FeatureVec} \cong \mathbb{R}^{N_0} ] where (N_0 \approx 98{,}304).

Hidden layer 1: [ V_1 := {0,\dots,255} \to \mathbb{R} \cong \mathbb{R}^{256} ]

Hidden layer 2: [ V_2 := {0,\dots,31} \to \mathbb{R} \cong \mathbb{R}^{32} ]

Output space: [ V_3 := \mathbb{R} ]

Each layer is then a function of type:

Layer 1 (affine + nonlinearity): [ \mathsf{L}_1 : V_0 \to V_1 ]

Layer 2: [ \mathsf{L}_2 : V_1 \to V_2 ]

Output layer: [ \mathsf{L}_3 : V_2 \to V_3 ]

The full network is:

[ \mathsf{NNUE} : \text{State} \to V_3 ] [ \mathsf{NNUE}(st) := \mathsf{L}_3(\mathsf{L}_2(\mathsf{L}_1(\mathsf{features}(st)))) ]

So type-theoretically, it’s a composition:

[ \mathsf{NNUE} = \mathsf{L}_3 \circ \mathsf{L}_2 \circ \mathsf{L}_1 \circ \mathsf{features} ]

with each arrow having a precise type.

4. Refinement types for legality and consistency

You can make the types more precise by refining them with invariants:

Legal positions only: [ \text{Position}_\text{legal} := { p : \text{Position} \mid \text{legal}(p) } ]

Feature correctness: [ \text{FeatureVec}_p := { f : \text{FeatureVec} \mid \text{consistent}(p, f) } ]

Then:

[ \mathsf{features} : \Pi (p : \text{Position}_\text{legal}),; \text{FeatureVec}_p ]

So the type guarantees that the feature vector is not arbitrary—it’s a refinement indexed by the position.

Similarly, you can refine the network to say “this is an evaluation in centipawns”:

[ \text{Eval} := { v : \mathbb{R} \mid \text{interpreted_as_centipawns}(v) } ] [ \mathsf{NNUE} : \text{State} \to \text{Eval} ]

5. Incremental updates as a dependent delta

The NNUE trick is that when a move is played, only a small subset of features changes. Type-wise, you can model this as a delta on the feature vector:

Move type: [ \text{Move} := { m \mid \text{legal_move}(m) } ]

State transition: [ \mathsf{applyMove} : \Pi (st : \text{State}),; \text{Move}(st) \to \text{State} ]

Feature delta: [ \mathsf{deltaFeatures} : \Pi (st : \text{State}),; \Pi (m : \text{Move}(st)),; \text{FeatureVec} \to \text{FeatureVec} ]

with a refinement:

[ \mathsf{deltaFeatures}(st, m, f) = \mathsf{features}(\mathsf{applyMove}(st, m)) ] and [ \text{supp}(\mathsf{deltaFeatures}(st, m, f) - f) \subseteq \text{small_set}(st, m) ]

So the type says: given a previous feature vector f consistent with st, and a legal move m, you get a new feature vector consistent with the new state, and the difference is supported only on a small index set (the changed piece–square features). That’s exactly the NNUE incremental update mechanism, but expressed as a refinement on the delta function.

You can push this further and make the hidden layer state explicit:

Cached hidden state: [ \text{HiddenState}_1 := V_1 ]

Incremental update: [ \mathsf{updateH1} : \Pi (st : \text{State}),; \Pi (m : \text{Move}(st)),; \text{HiddenState}_1 \to \text{HiddenState}_1 ]

with a refinement that updateH1 corresponds to applying L1 only to the changed input features.

6. Summary as a type-theoretic skeleton

Putting it all together:

Domain: [ \text{State} := \text{Position}_\text{legal} \times \text{STM} ]

Feature extraction: [ \mathsf{features} : \Pi (st : \text{State}),; \text{FeatureVec}_{st} ]

Layer spaces: [ V_0 := \text{FeatureVec},\quad V_1 := \mathbb{R}^{256},\quad V_2 := \mathbb{R}^{32},\quad V_3 := \text{Eval} ]

Layers: [ \mathsf{L}_1 : V_0 \to V_1,\quad \mathsf{L}_2 : V_1 \to V_2,\quad \mathsf{L}_3 : V_2 \to V_3 ]

Network: [ \mathsf{NNUE} : \text{State} \to \text{Eval} ] [ \mathsf{NNUE}(st) := \mathsf{L}_3(\mathsf{L}_2(\mathsf{L}_1(\mathsf{features}(st)))) ]

If you’d like, next step could be: we make the weight matrices and activations explicit as records with dependent fields (e.g. weights : FeatIdx → H1Idx → ℝ), and then talk about erasure—what parts of this structure are relevant at “specification level” vs “runtime-optimized bitboard/AVX implementation”.
~~~