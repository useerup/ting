
    Either = (Sets L,Sets R) => {| Left:L, Right:R |}


    f = Either(a,b) x => 
        x |> { 
            ~ .Left l -> (Some l, None)
            ~ .Right r -> (None,Some l) 
        }


    Option = private Sets A => {| Some:A, None |}

    g = Option int x => 
        x |> {
            ~ Some value -> x
            None -> 0
        }

