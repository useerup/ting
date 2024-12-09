
    Quadrilateral = Shape extend {.
        (Line^4) Sides
        (Angles^4) Angles
    .}

    Rectangle = Rectangle && { _\\
        Lines = with(a,b) in (a,b,a,b)
        Angles = (90,90,90,90)
    }

