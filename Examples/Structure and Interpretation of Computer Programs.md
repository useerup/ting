See https://web.mit.edu/6.001/6.037/sicp.pdf


// The following models geometric shapes in a hierarchy. Unlike traditional nominal or structured typing, Tings blend of

    Point = (TODO)

    Shape = class {. Area:float, Circumference:float .}

    Polygon = Shape * {. Corners = ..., Sides = ... .}

    Quadrilateral = Polygon && {?
        public [A,B,C,D] = Corners
        public [SideA,SideB,SideC,SideD] = Sides
    ?}

    Trapezoid = Quadrilateral && {? SideA.IsParallelTo SideC ?}

    Kite = Quadrilateral && {? SideA.Length == SideB.Length ?}

    Parallelogram = Trapezoid && {? SideB.IsParallelTo SideD ?}

    Rhombus = Parallelogram & Kite
 
    Rectangle = Parallelogram && {? Corners.All .IsRightAngle ?}

    Square = Rectangle & Rhombus




    Triangle = Polygon && {?
        public [A,B,C] = Corners
        public [SideA,SideB,SideC] = Sides
    ?}

    RightTriangle = Triangle && {? 
        C.IsRightAngleBetween(A,B)
    ?}

    IsoscelesTriangle = Triangle && {?
        SideA.Length == SideB.Length
    ?}

    IsoscelesRightTriangle = IsoscelesTriangle & RightTriangle

    EquilateralTriangle = IsoscelesTriangle && {? SideC.Length == SideA.Length ?}



