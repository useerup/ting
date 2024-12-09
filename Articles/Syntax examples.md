    // A tuple instance
    Point = (1,42)

    // A record instance
    Person1 = (. Name="Zaphod", Age=42 .)

    // Tuple type 
    Points = float*float
    Points = { (float _, float _) }

    // Record type
    People = Name@:string * Age@:int && {
        let x \\
    }
        Name.Length <= 30
        Age >= 0
    
    People = {. 
        Name:string
        Age:int 
        \\\ 
        Name.Length <= 30
        Age >= 0
     .}

    


    Location = { (string description,,CompassDirections^4 (north, south, east, west)) }

    GameState = { Locations map, }