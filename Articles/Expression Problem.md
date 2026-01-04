# How Ting solves the Expression Problem

    export 
    
        Expr = class rec {
            Eval:int abstract
        }

        Add = Expr extend rec {
            Left : Expr
            Right : Expr
            Eval override left.Eval + right.Eval
        }

        Const = int i => Expr extend rec {
            Value : int
            Eval override Value
        }

    ---

    export 

        Neg = Expr extend rec {
            Arg : Expr
            Eval override -Value
        }

    ---

    export 

        Expr..Format : string virtual | >>>
            Add x => $"( {x.Left.Format} + {x.Right.Format} )"
            Const x => x.ToString()
            Neg x => $"-{x.Format}"

    ---

    export 

        Mul = Expr extend {
            Left : Expr
            Right : Expr
            Eval override Left.Eval * Right.Eval
            Format override $"( {Left.Format} * {Right.Format} )"
        }

    