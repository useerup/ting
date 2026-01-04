
    let
        (left,right) = file
            .ReadLines("path")
            .Map(./Parse( (a,b) => int.Parse a >> .SkipSpace >> int.Parse b ) )
            .Unzip()
    in
        Lists.Zip( left.OrderBy(._), right.OrderBy(._) ) 
        |> ./Map((l,r)=>Math.Abs(l-r)) 
        |> ./Sum()




    let
        (left,right) = file
            .ReadLines("path")
            .Map(./Parse( (a,b) => int.Parse a >> .SkipSpace >> int.Parse b ) )
            .Unzip()
    in
        left.Map(l => l * right.Where(==l).Count())

