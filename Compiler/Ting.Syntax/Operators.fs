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
    [ [ "="; "?=" ], LeftAssoc, all RightReferential
      [ "./"; "../" ], LeftAssoc, all LeftReferential
      [ "|"; "||"; "|||" ], LeftAssoc, all Neutral
      [ "&"; "&&"; "&&&" ], LeftAssoc, all Neutral
      [ ":"; "::"; ":::" ], NonAssoc, all RightReferential
      [ "<"; "<="; ">="; ">"; "=="; "!="; "!:" ], NonAssoc, all RightReferential
      [ "..."; "...<"; "<..."; "<...<" ], NonAssoc, all Neutral
      [ ";"; ";;" ], LeftAssoc, all Neutral
      [ "^^" ], RightAssoc, all Neutral
      [ "**" ], LeftAssoc, all Neutral
      [ "+"; "-" ], LeftAssoc, all Neutral
      [ "*"; "/"; "%"; "/%" ], LeftAssoc, all Neutral
      [ "^" ], LeftAssoc, all Neutral
      [ "."; ".." ], LeftAssoc, all PropertyAccess ]

let infixTable: Map<string, InfixInfo> =
    rows
    |> List.mapi (fun index (ops, assoc, binding) ->
        ops |> List.map (fun op -> op, { Text = op; Row = index + 1; Assoc = assoc; Binding = binding op }))
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
    | "!" -> leftPower infixTable[":"]
    | _ -> invalidArg (nameof text) $"'{text}' is not a prefix operator"
