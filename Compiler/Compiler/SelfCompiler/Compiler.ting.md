
`Char` is a parser function which parses the front character of a string

    Char = c -> string[c,,rest] -> rest

Symbols:

    Symbols = CharSymbols + WordSymbols

    CharSymbols = { 
        "+", "-", "*", "/", "%",
        ":", "::", ":::", "!:", 
        "<", "<=", "==", "!=", ">=", ">",
        "=>", "->", "%>", "-->"
        ";", ";;", "#", "##",
        "(", ")", "(.", ".)", "(|", "|)", "(.", ".)", 
        "{", "}", "{.", ".}", "{|", "|}", "{>", "<}", 
    }
