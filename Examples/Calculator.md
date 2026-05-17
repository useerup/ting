## Example: A small calculator


    Parser = string --> string
    Rule = Any --> Parser

    OneOrMore = Parser rule => 
        [h]+t => rule h >> ZeroOrMore rule t
    
    ZeroOrMore = Parser rule => || >>>
        [h]+t => rule h >> ZeroOrMore rule t
        Parser string

    Spaces = ZeroOrMore ' '

    Char = char c => [c]+rest => rest

    Digit = { '0'...'9' } >> Char

    Symbol = string sym => Spaces >> string(sym+rest) => rest

    Number = int.Parse d => Spaces >> OneOrMore Digit d

    LeftAssoc = Operator op => next => let this ~ || >>>
        f <| (a,b) => this a >> op f >> next b
        next

    RightAssoc = Operator op => next => let this ~ || >>> 
        f <| (a,b) => next a >> op f >> this b
        next

    Exp =
        LeftAssoc Add|Sub
            LeftAssoc Mul|Div
                Number

    Add = (+) => Symbol "+"
    Mul = (*) => Symbol "*"
    Div = (/) => Symbol "/"
    Sub = (-) => Symbol "-"

    