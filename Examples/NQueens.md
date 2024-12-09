# N-Queens problem

    int n => with
            Pos = { ( 1...n, 1...n ) }
            IsAttacked = (Pos (r,c), Queens qs) => exists q:qs && (q.Row==r | q.Col==c | (q.Row-p.Row,q.Col-p.Col)==let(n,n);let(n,-n))
            PlaceQueens = { (0,{Pos}b) -> b, 0 |>  }
        in

