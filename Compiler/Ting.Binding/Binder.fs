/// Binding from the program text: the scope walk of the specification's Declarations and Binding section.
module Ting.Binding.Binder

open System.Collections.Generic
open Ting.Syntax.Source
open Ting.Syntax.Syntax
open Ting.Syntax.Operators
open Ting.Binding.Model

let builtins =
    [ "bool"; "char"; "string"; "byte"; "sbyte"; "short"; "ushort"; "int"; "uint"; "long"; "ulong"
      "float"; "double"; "decimal"; "unit"; "Sets"; "Funcs"; "bound"; "free" ]

type private PendingKind =
    | Plain
    | Shorthand of field: DeclId * fieldFrame: FrameId
    | Property of declares: bool * isMember: bool * receiver: Span
    | Interpolated

type private Pending =
    { Name: string
      Span: Span
      Stack: (FrameId * Mode) list
      Kind: PendingKind
      mutable Result: Binding option }

let private referential (stack: (FrameId * Mode) list) =
    stack |> List.map (fun (f, m) -> f, (if m = Declarative then Forced else m))

/// A deferred occurrence may still declare into the frames it passes once analysis decides it.
let private mayDeclareLater (binding: Binding) =
    match binding with
    | Deferred(ScopeContents _ | ImplicitThis _ | Blocked) -> true
    | _ -> false

type private Binder() =
    let frames = ResizeArray<Frame>()
    let names = Dictionary<FrameId, Dictionary<string, DeclId>>()
    let decls = Dictionary<DeclId, Decl>()
    let alias = Dictionary<DeclId, DeclId>()
    let pendings = ResizeArray<Pending>()
    /// Unclassified occurrences that may still declare a name into a frame.
    let openCount = Dictionary<struct (FrameId * string), int>()
    let mutable nextDecl = 1

    let isOpen (f: FrameId) (name: string) =
        match openCount.TryGetValue(struct (f, name)) with
        | true, n -> n > 0
        | _ -> false

    let adjustOpen (p: Pending) (delta: int) =
        for f, mode in p.Stack do
            if mode = Declarative then
                let key = struct (f, p.Name)
                openCount[key] <- (match openCount.TryGetValue key with | true, n -> n | _ -> 0) + delta

    let rec canon (id: DeclId) =
        match alias.TryGetValue id with
        | true, target -> canon target
        | _ -> id

    let kindOf (f: FrameId) = frames[f].Kind

    let newFrame (kind: FrameKind) (at: Position) (stack: (FrameId * Mode) list) =
        let id = frames.Count
        let parent = match stack with (p, _) :: _ -> Some p | [] -> None
        frames.Add { Id = id; Kind = kind; At = at; Parent = parent }
        names[id] <- Dictionary()
        id

    let lookup (f: FrameId) (name: string) =
        match names[f].TryGetValue name with
        | true, id -> Some(canon id)
        | _ -> None

    let newDecl (name: string) (kind: DeclKind) (frameIds: FrameId list) (span: Span) =
        let id = nextDecl
        nextDecl <- nextDecl + 1
        decls[id] <- { Id = id; Name = name; Kind = kind; Frames = frameIds; Span = span }
        id

    let addToFrames (id: DeclId) (name: string) (frameIds: FrameId list) =
        for f in frameIds do
            names[f][name] <- id
        let d = decls[id]
        decls[id] <- { d with Frames = d.Frames @ frameIds |> List.distinct }

    /// The frame that receives a declaration: only the innermost declarative frame of the walk.
    let segment (stack: (FrameId * Mode) list) =
        match stack with
        | (f, Declarative) :: _ -> [ f ]
        | _ -> []

    let declare (name: string) (span: Span) (frameIds: FrameId list) =
        let head = List.head frameIds
        if name = "this" then
            match lookup head name with
            | Some id -> id
            | None ->
                let id = newDecl name ThisBinding [ head ] span
                names[head][name] <- id
                id
        else
            match frameIds |> List.choose (fun f -> lookup f name) |> List.distinct |> List.sort with
            | [] ->
                let kind =
                    match kindOf head with
                    | Fields _ -> Field
                    | Publish _ -> Published
                    | _ -> LocalName
                let id = newDecl name kind [] span
                addToFrames id name frameIds
                id
            | keep :: others ->
                for other in others do
                    alias[other] <- keep
                    addToFrames keep name decls[other].Frames
                addToFrames keep name frameIds
                keep

    let declareThis (f: FrameId) (span: Span) =
        names[f]["this"] <- newDecl "this" ThisBinding [ f ] span

    do
        let root = newFrame Root { Offset = 0; Line = 1; Column = 1 } []
        builtins
        |> List.iteri (fun i name ->
            let id = -(i + 1)
            decls[id] <- { Id = id; Name = name; Kind = Builtin; Frames = [ root ]; Span = { Start = frames[root].At; End = frames[root].At } }
            names[root][name] <- id)

    member _.Root = 0

    member _.Add(name: string, span: Span, stack: (FrameId * Mode) list, kind: PendingKind) =
        let p = { Name = name; Span = span; Stack = stack; Kind = kind; Result = None }
        pendings.Add p
        if kind = Plain && name <> "_" then adjustOpen p 1

    // ---- Collection: frames and occurrences from the syntax tree ----

    member this.Walk(expr: Expr, stack: (FrameId * Mode) list) : unit =
        match expr with
        | Identifier(span, name) -> this.Add(name, span, stack, Plain)
        | InterpolatedIdentifier(span, parts) ->
            let name = parts |> List.map (function Text s -> s | Hole _ -> "{}") |> String.concat ""
            this.Add(name, span, stack, Interpolated)
            this.Parts(parts, referential stack)
        | This span -> this.Add("this", span, stack, Plain)
        | Void _
        | Bool _
        | Number _ -> ()
        | String(_, parts) -> this.Parts(parts, stack)
        | Group(_, inner) -> this.Walk(inner, stack)
        | Tuple(_, items)
        | Set(_, items)
        | Multiset(_, items)
        | List(_, items) ->
            for item in items do
                this.Walk(item, stack)
        | Product(span, positional, _, named) ->
            for item in positional do
                this.Walk(item, stack)
            let f = newFrame (Fields "product") span.Start stack
            this.FieldItems(named, f, stack)
        | Apply(_, callee, argument) ->
            this.Walk(callee, referential stack)
            this.Walk(argument, stack)
        | Prefix(_, op, operand) -> this.Prefix(op, operand, stack)
        | Infix(_, op, left, right) -> this.Infix(op, left, right, stack)
        | Section(_, op, operand) -> this.Section(op, operand, stack)

    member this.Parts(parts: StringPart list, stack: (FrameId * Mode) list) =
        for part in parts do
            match part with
            | Hole e -> this.Walk(e, stack)
            | Text _ -> ()

    /// Elements of a record or a product's named section. A bare name both declares a field and references the name.
    member this.FieldItems(items: Expr list, f: FrameId, stack: (FrameId * Mode) list) =
        let inner = (f, Declarative) :: stack
        for item in items do
            match item with
            | Identifier(span, name) when name <> "_" ->
                let field = declare name span [ f ]
                this.Add(name, span, inner, Shorthand(field, f))
            | _ -> this.Walk(item, inner)

    member this.Prefix(op: Operator, operand: Expr, stack: (FrameId * Mode) list) =
        let at = op.Span.Start
        match op.Text with
        | "let" ->
            let f = newFrame Let at stack
            this.Walk(operand, (f, Declarative) :: stack)
        | "public"
        | "private"
        | "protected"
        | "internal" ->
            let f = newFrame (Publish op.Text) at stack
            this.Walk(operand, (f, Declarative) :: stack)
        | "record"
        | "records" ->
            let f = newFrame (Fields op.Text) at stack
            declareThis f op.Span
            let items =
                match operand with
                | Tuple(_, xs) -> xs
                | Group(_, x) -> [ x ]
                | x -> [ x ]
            this.FieldItems(items, f, stack)
        | "tuple" ->
            let f = newFrame (TupleScope op.Text) at stack
            declareThis f op.Span
            this.Walk(operand, (f, Referential) :: stack)
        | "tuples" ->
            let f = newFrame (TupleScope op.Text) at stack
            declareThis f op.Span
            this.Walk(operand, (f, Declarative) :: stack)
        | _ -> this.Walk(operand, stack)

    member this.Infix(op: Operator, left: Expr, right: Expr, stack: (FrameId * Mode) list) =
        let at = op.Span.Start
        match infixTable[op.Text].Binding with
        | Neutral ->
            this.Walk(left, stack)
            this.Walk(right, stack)
        | RightReferential ->
            this.Walk(left, stack)
            this.Walk(right, referential stack)
        | LeftReferential ->
            this.Walk(left, referential stack)
            let f = newFrame (ExpressionScope op.Text) at stack
            this.Walk(right, (f, Referential) :: stack)
        | LeftDeclarative ->
            let f = newFrame (Local op.Text) at stack
            this.Walk(left, (f, Declarative) :: stack)
            this.Walk(right, (f, Referential) :: stack)
        | RightDeclarative ->
            let f = newFrame (Local op.Text) at stack
            this.Walk(left, (f, Referential) :: stack)
            this.Walk(right, (f, Declarative) :: stack)
        | PropertyAccess ->
            this.Walk(left, referential stack)
            match right with
            | Identifier(span, name) ->
                let declares = match stack with (_, Declarative) :: _ -> true | _ -> false
                this.Add(name, span, stack, Property(declares, op.Text = "..", left.Span))
            | _ -> this.Walk(right, referential stack)

    /// A binary operator in prefix position: the operand is treated as the operator's right operand.
    member this.Section(op: Operator, operand: Expr, stack: (FrameId * Mode) list) =
        let at = op.Span.Start
        match infixTable[op.Text].Binding with
        | Neutral -> this.Walk(operand, stack)
        | RightReferential -> this.Walk(operand, referential stack)
        | LeftReferential ->
            let f = newFrame (ExpressionScope op.Text) at stack
            this.Walk(operand, (f, Referential) :: stack)
        | LeftDeclarative ->
            let f = newFrame (Local op.Text) at stack
            this.Walk(operand, (f, Referential) :: stack)
        | RightDeclarative ->
            let f = newFrame (Local op.Text) at stack
            this.Walk(operand, (f, Declarative) :: stack)
        | PropertyAccess ->
            match operand with
            | Identifier(span, name) -> this.Add(name, span, stack, Property(false, op.Text = "..", op.Span))
            | _ -> this.Walk(operand, referential stack)

    // ---- Classification ----

    /// The binding walk. None means a frame on the way may still receive declarations.
    member _.Resolve(name: string, span: Span, stack: (FrameId * Mode) list) : Binding option =
        let rec go (stack: (FrameId * Mode) list) =
            match stack with
            | [] -> Some Unresolved
            | (_, Declarative) :: _ -> Some(Declares(declare name span (segment stack)))
            | (f, mode) :: rest ->
                match kindOf f with
                | ExpressionScope _ -> Some(Deferred(ScopeContents f))
                | Fields _
                | TupleScope _ ->
                    match lookup f name with
                    | Some id when name = "this" -> Some(References id)
                    | _ -> go rest
                | _ ->
                    match lookup f name with
                    | Some id -> Some(References id)
                    | None when isOpen f name -> None
                    | None when mode = Referential && name <> "this" && (lookup f "this").IsSome ->
                        Some(Deferred(ImplicitThis f))
                    | None -> go rest
        go stack

    member this.Classify(p: Pending) : Binding option =
        match p.Kind with
        | Interpolated -> Some(Deferred InterpolatedName)
        | Property(false, _, _) -> Some(Deferred PropertyName)
        | Property(true, isMember, receiver) ->
            Some(Declares(newDecl p.Name (Hosted(isMember, receiver)) (segment p.Stack) p.Span))
        | Plain when p.Name = "_" -> Some Anonymous
        | Plain -> this.Resolve(p.Name, p.Span, p.Stack)
        | Shorthand(field, fieldFrame) ->
            let outer = p.Stack |> List.skipWhile (fun (f, _) -> f <> fieldFrame) |> List.tail |> referential
            this.Resolve(p.Name, p.Span, outer) |> Option.map (fun b -> FieldShorthand(field, b))

    member this.Solve() =
        let mutable progress = true
        while progress do
            progress <- false
            for p in pendings do
                if p.Result.IsNone then
                    match this.Classify p with
                    | Some result ->
                        p.Result <- Some result
                        progress <- true
                        if p.Kind = Plain && p.Name <> "_" && not (mayDeclareLater result) then adjustOpen p -1
                    | None -> ()
        for p in pendings do
            if p.Result.IsNone then p.Result <- Some(Deferred Blocked)

    // ---- Results ----

    member _.Redeclarations() : Diagnostic list =
        let rec ancestors (f: FrameId) =
            match frames[f].Parent with
            | Some p -> p :: ancestors p
            | None -> []
        [ for d in decls.Values do
              if d.Kind = LocalName && canon d.Id = d.Id then
                  let own = set d.Frames
                  let clash =
                      d.Frames
                      |> List.collect ancestors
                      |> List.filter (fun a -> not (own.Contains a))
                      |> List.exists (fun a ->
                          match kindOf a, lookup a d.Name with
                          | (Let | Local _ | TupleScope _), Some other -> other <> d.Id && decls[other].Kind = LocalName
                          | _ -> false)
                  if clash then
                      { Message = $"'{d.Name}' is already declared in an enclosing scope"; Span = d.Span } ]

    member this.Result() : Bound =
        let userDecls =
            decls.Values
            |> Seq.filter (fun d -> d.Id > 0 && canon d.Id = d.Id)
            |> Seq.sortBy (fun d -> d.Span.Start.Offset, d.Id)
            |> List.ofSeq
        // Number declarations in source order.
        let number = userDecls |> List.mapi (fun i d -> d.Id, i + 1) |> dict
        let renumber (id: DeclId) =
            let c = canon id
            if c < 0 then c else number[c]
        let rec canonBinding (b: Binding) =
            match b with
            | Declares id -> Declares(renumber id)
            | References id -> References(renumber id)
            | FieldShorthand(f, v) -> FieldShorthand(renumber f, canonBinding v)
            | other -> other
        let occurrences =
            pendings
            |> Seq.map (fun p -> { Name = p.Name; Span = p.Span; Binding = canonBinding p.Result.Value })
            |> Seq.sortBy (fun o -> o.Span.Start.Offset)
            |> List.ofSeq
        let unresolved =
            occurrences
            |> List.choose (fun o ->
                match o.Binding with
                | Unresolved
                | FieldShorthand(_, Unresolved) -> Some { Message = $"unresolved name '{o.Name}'"; Span = o.Span }
                | _ -> None)
        { Frames = List.ofSeq frames
          Decls = userDecls |> List.map (fun d -> { d with Id = number[d.Id] })
          Occurrences = occurrences
          Diagnostics = (this.Redeclarations() @ unresolved) |> List.sortBy (fun d -> d.Span.Start.Offset) }

/// Binds every identifier occurrence in a compilation unit.
let bind (expr: Expr) : Bound =
    let binder = Binder()
    binder.Walk(expr, [ binder.Root, Referential ])
    binder.Solve()
    binder.Result()
