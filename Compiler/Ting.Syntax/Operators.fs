module Ting.Syntax.Operators

type Associativity =
    | LeftAssoc
    | RightAssoc
    | NonAssoc

/// How an operator treats the scope frames covering its operands (spec: Declarative and referential operators).
type BindingClass =
    | Neutral
    | RightReferential
    | LeftReferential
    | LeftDeclarative
    | RightDeclarative
    /// `.` and `..`: referential receiver, property interpreted against it.
    | PropertyAccess

type InfixInfo =
    { Text: string
      /// 1-based row of the spec's binary infix table; higher binds tighter.
      Row: int
      Assoc: Associativity
      Binding: BindingClass }

/// The rows of the specification's binary infix table, loosest first.
/// Associativity is provisional (see specification issue SYN-01).
let private rows: (string list * Associativity * (string -> BindingClass)) list =
    let all b = fun (_: string) -> b
    [ [ "where" ], RightAssoc, all RightDeclarative
      [ "in" ], RightAssoc, all LeftDeclarative
      [ "="; "?=" ], NonAssoc, all RightReferential
      [ "contract"; "asks"; "throws" ], LeftAssoc, (fun op -> if op = "contract" then LeftDeclarative else RightReferential)
      [ "->" ], LeftAssoc, all RightReferential
      [ ">>"; "<<"; "|>"; "<|" ], LeftAssoc, all Neutral
      [ "=>" ], RightAssoc, all LeftDeclarative
      [ "then"; "else" ], RightAssoc, (fun op -> if op = "then" then RightReferential else Neutral)
      [ "./"; "../" ], LeftAssoc, all LeftReferential
      [ "catch"; "answer" ], LeftAssoc, all RightReferential
      [ "\\" ], LeftAssoc, all LeftDeclarative
      [ "|"; "||"; "|||" ], LeftAssoc, all Neutral
      [ "&"; "&&"; "&&&" ], LeftAssoc, all Neutral
      [ "?"; "??" ], LeftAssoc, all RightReferential
      [ ":"; "::"; ":::" ], NonAssoc, all RightReferential
      [ "<"; "<="; ">="; ">"; "=="; "!="; "!:" ], NonAssoc, all RightReferential
      [ ";"; ";;" ], LeftAssoc, all Neutral
      [ "..."; "...<"; "<..."; "<...<" ], NonAssoc, all Neutral
      [ "^^" ], RightAssoc, all Neutral
      [ "**" ], LeftAssoc, all Neutral
      [ "+"; "-" ], LeftAssoc, all Neutral
      [ "*"; "/"; "%"; "/%" ], LeftAssoc, all Neutral
      [ "^" ], RightAssoc, all Neutral
      [ "."; ".." ], LeftAssoc, all PropertyAccess ]

/// Operators whose associativity differs from their row; mixing them with the row's other operators is an error.
let private assocOverrides = Map [ "<|", RightAssoc ]

let infixTable: Map<string, InfixInfo> =
    rows
    |> List.mapi (fun index (ops, assoc, binding) ->
        ops
        |> List.map (fun op ->
            let assoc = assocOverrides |> Map.tryFind op |> Option.defaultValue assoc
            op, { Text = op; Row = index + 1; Assoc = assoc; Binding = binding op }))
    |> List.concat
    |> Map.ofList

let tryInfix (text: string) = Map.tryFind text infixTable

let rowOf (text: string) = infixTable[text].Row

/// Binding power on the left of an infix operator. Row r occupies powers 2r and 2r+1.
let leftPower (info: InfixInfo) = 2 * info.Row

/// Minimum binding power of the right operand.
let rightPower (info: InfixInfo) =
    match info.Assoc with
    | RightAssoc -> 2 * info.Row
    | LeftAssoc
    | NonAssoc -> 2 * info.Row + 1

/// Minimum binding power of the operand of a prefix keyword or `!` (provisional, see SYN-01).
let prefixOperandPower (text: string) =
    match text with
    | "let" | "public" | "private" | "protected" | "internal" -> 0
    | "total" | "partial" | "determ" | "nondeterm" | "throw" | "ask" -> leftPower infixTable["|"]
    | "!" -> leftPower infixTable[":"]
    | "fn" | "class" | "record" | "records" | "tuple" | "tuples" | "primary" | "candidate" -> leftPower infixTable["."]
    | _ -> invalidArg (nameof text) $"'{text}' is not a prefix operator"

let isPrefixKeyword (word: string) =
    match word with
    | "let" | "public" | "private" | "protected" | "internal"
    | "total" | "partial" | "determ" | "nondeterm" | "throw" | "ask"
    | "fn" | "class" | "record" | "records" | "tuple" | "tuples" | "primary" | "candidate" -> true
    | _ -> false
