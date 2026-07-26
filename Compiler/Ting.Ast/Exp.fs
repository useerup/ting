module Exp

open System
open Range

type Literal =
    | Int32 of int
    | Boolean of bool
    | Float32 of float
    | Float64 of double
    | Decimal of decimal
    | String of string

type AccessModifier =
    | Public
    | Private
    | Internal
    | Protected

/// The abstract syntax tree.
///
/// The parser produces this tree with:
///   - binding-intent marker nodes (Declare, Reference, DeclareLeft/Right,
///     ReferenceLeft/Right, PushScope, PushMemberScope), and
///   - identifiers in the Unbound binding role, except for the operator heads it
///     synthesizes: the parser knows every operator it can parse, so it emits
///     those already Referential against its own operator scope.
///
/// The Binder (run as part of parsing) rewrites the tree in place-of-type:
///   - marker nodes are eliminated (lowered to Local/Apply/scope pushes),
///   - every Unbound identifier becomes Declarative (with a fresh declaration
///     id recorded in its declarative scopes) or Referential (with the ordered
///     candidate scope-id list, nearest enclosing scope first).
///
/// The Resolver later upgrades Referential occurrences to Resolved.
///
/// Source patterns per case:
///
///   Declare          Prefix `let E` — opens a declaration/constraint region over `E`.
///                    Eliminated by the Binder into `Local`.
///   Reference        Not a keyword: wraps the callee of a juxtaposition application
///                    (`f x` => `Apply(Reference f, x)`) to flag it as a use-site.
///                    Eliminated by the Binder.
///   DeclareLeft      Infix operator whose LEFT operand is declarative; fields are
///                    (operator head, left, right). Produced by `x => body` (lambda
///                    parameter) and `elem \ gen` (set-comprehension binder).
///   DeclareRight     Infix operator whose RIGHT operand is declarative. Produced by
///                    `body where decls`.
///   ReferenceLeft    Infix operator with a plain-reference LEFT operand. Defined for
///                    symmetry but currently NOT produced by the parser.
///   ReferenceRight   Infix operator whose RIGHT operand is a plain reference while the
///                    left keeps the ambient binding context. Produced by `=`, `in`,
///                    `then`, `->`, `catch`, `?`, `??`, `:`, `::`, `:::`, `<`, `<=`,
///                    `>=`, `>`, `==`, `!=`, `!:` — e.g. `x = 5`, `decls in body`.
///   PushScope        Infix `./` — `scope ./ body`. Eliminated by the Binder (pushes an
///                    ExpressionScope over `body`).
///   PushMemberScope  Infix `../` — `scope ../ body`. Eliminated by the Binder (pushes
///                    an ExpressionMemberScope over `body`).
///   Publish          Prefix access modifier on a declaration: `public E`, etc. The
///                    parser allocates the declaration scope; the Binder fills it.
///   Apply            Function/operator application (`f x` and desugared operators).
///   Tuple            Parenthesized comma-separated list `(a, b, ...)`.
///   Record           Prefix `record E`. The parser allocates the record's scope; the
///                    Binder fills it with the record's declarations.
///   Identifier       An identifier occurrence carrying its BindingRole. The parser
///                    emits Unbound for identifiers written in source, and the
///                    already-Referential head of each operator desugaring
///                    (`_+_`, `_=>_`, `!_`, `fn_`, ...). The Binder rewrites
///                    Unbound to Declarative/Referential and leaves the rest alone.
///   Literal          A literal term; see the Literal type.
///   Set              Brace comma-separated list `{a, b, ...}`; also `records E`,
///                    `tuples E`.
///   List             Bracket comma-separated list `[a, b, ...]`.
///   StringLiteral    Interpolated string `$"text{expr}text"` (a plain `"..."` is a
///                    single-Literal string instead).
///   Projection       Infix `.` — `receiver.member`; the parser allocates its scope.
///   MemberProjection Infix `..` — `receiver..member`; the parser allocates its scope.
///   This             The `this` keyword, carrying its BindingRole.
///   Void             The `void` keyword.
///
/// Analyzed-only cases (never produced by the parser):
///
///   Local            A declaration region with its scope, produced from Declare,
///                    DeclareLeft/Right by the Binder.
///   ScopeExp         An expression annotated with an explicit scope.
///   Unknown          Placeholder expression (e.g. names declared without a value yet).
///   Throw/Catch/IsDefined/Equals/ValueEquals
///                    Semantic forms produced by later analysis passes.
type Exp =
    // Surface binding-intent markers (parser output, eliminated by the Binder).
    | Declare of Node
    | Reference of Node
    | DeclareLeft of Node * Node * Node
    | DeclareRight of Node * Node * Node
    | ReferenceLeft of Node * Node * Node
    | ReferenceRight of Node * Node * Node
    | PushScope of Node * Node
    | PushMemberScope of Node * Node
    // Shared nodes (parser output, preserved by the Binder).
    | Publish of Scope * AccessModifier * Node
    | Apply of Node * Node
    | Tuple of Node list
    | Record of Scope * Node
    | Identifier of BindingRole
    | Literal of Literal
    | Set of Node list
    | List of Node list
    | StringLiteral of InterpolatedParts
    | Projection of Scope * Node * Node
    | MemberProjection of Scope * Node * Node
    | This of BindingRole
    | Void
    // Analyzed-only nodes (produced by the Binder and later passes).
    | Local of Scope * Node
    | ScopeExp of Scope * Node
    | Unknown
    | Throw of Node
    | Catch of Node * Node
    | IsDefined of Node
    | Equals of Node * Node
    | ValueEquals of Node * Node

/// An expression together with the source span it was parsed from.
///
/// Every recursive position in Exp goes through Node, so a range is available at
/// every point in the tree. Nodes with no source text of their own (builtins,
/// placeholders, some desugaring artifacts) carry Range.synthetic.
///
/// Note: do not print a Node tree with %A. Each Node hop costs print depth, so
/// %A elides nested structure as `Apply (..., ...)`. Use ExpDiagnostics.renderExpression,
/// which is written for this tree and has no depth limit.
and Node = {
    Exp: Exp
    Range: Range
}

and InterpolatedPart =
    | TextPart of string
    | HolePart of Node

and InterpolatedParts = InterpolatedPart list

// Unbound is the parser's role for every identifier occurrence. The Binder
// rewrites it: Declarative carries declaration identity; Referential carries the
// ordered candidate scope ids + name pattern. Resolved carries the declaration
// id a referential occurrence was bound to during name resolution.
and BindingRole =
    | Unbound of InterpolatedParts
    | Declarative of Guid * InterpolatedParts
    | Referential of Guid list * InterpolatedParts
    | Resolved of Guid * InterpolatedParts

and NameHost =
    | DirectPropertyHost of Node
    | MemberPropertyHost of Node

and NameEntry = {
    NameParts: InterpolatedParts
    Expression: Node
    Host: NameHost option
}

// Name table for a scope: declaration id -> analyzed name entry.
and Names = System.Collections.Generic.Dictionary<Guid, NameEntry>

// Scope hierarchy.
and [<AbstractClass>] Scope(id: Guid, names: Names) =
    member _.Id = id
    member _.Names = names
    member this.defineName (id: Guid, name: string, exp: Node) : unit =
        this.Names.Add(id, { NameParts = [ TextPart name ]; Expression = exp; Host = None })
    member this.defineName (id: Guid, name: InterpolatedParts, exp: Node) : unit =
        this.Names.Add(id, { NameParts = name; Expression = exp; Host = None })
    member this.defineName (id: Guid, name: InterpolatedParts, exp: Node, host: NameHost) : unit =
        this.Names.Add(id, { NameParts = name; Expression = exp; Host = Some host })
    // Convenience overloads for callers that define names with no source text of
    // their own (builtins, placeholder declarations); the entry gets a synthetic range.
    member this.defineName (id: Guid, name: string, exp: Exp) : unit =
        this.defineName(id, name, { Exp = exp; Range = Range.synthetic })
    member this.defineName (id: Guid, name: InterpolatedParts, exp: Exp) : unit =
        this.defineName(id, name, { Exp = exp; Range = Range.synthetic })

and [<AbstractClass>] ChildScope(id: Guid) =
    inherit Scope(id, new Names())

and LocalScope(id: Guid) =
    inherit Scope(id, new Names())

and ExpressionScope(id: Guid, expression: Node) =
    inherit ChildScope(id)
    member _.Expression = expression

and ExpressionMemberScope(id: Guid, expression: Node) =
    inherit ChildScope(id)
    member _.Expression = expression

// Declarative scopes (names being introduced) and referential scopes
// (candidates for name lookup), nearest first.
type ScopeStack = Scope list * Scope list

let scopeId (s: Scope) = s.Id

/// Match a node's expression while ignoring its range.
///
/// Intended for *nested* pattern positions, where writing out the record pattern
/// would drown the shape being matched:
///
///     match node.Exp with
///     | Apply (E (Identifier role), arg) -> ...
///
/// Prefer plain `match node.Exp with` at the top level of a match: that keeps
/// F#'s exhaustiveness checking working, which is what makes adding a new Exp
/// case a compile error rather than a silent fallthrough.
let inline (|E|) (node: Node) = node.Exp

/// Match a node's expression and bind its range as well.
///
///     match node.Exp with
///     | Apply (ER (Identifier role, range), arg) -> ...
let inline (|ER|) (node: Node) = (node.Exp, node.Range)

[<RequireQualifiedAccess>]
module Node =
    /// A node for an expression that came from a known source span.
    let at (range: Range) (exp: Exp) = { Exp = exp; Range = range }

    /// A node with no source text of its own.
    let synthetic (exp: Exp) = { Exp = exp; Range = Range.synthetic }

    /// Replace a node's expression, keeping its range. The workhorse of any
    /// rewriting pass: a lowered node reports the same source span as the node
    /// it replaced.
    let like (original: Node) (exp: Exp) = { Exp = exp; Range = original.Range }

    /// A node whose range covers both of the given nodes. Used when a desugaring
    /// synthesizes a parent (e.g. the Tuple in an infix operator lowering) that
    /// has no single token of its own but spans its operands.
    let spanning (a: Node) (b: Node) (exp: Exp) =
        { Exp = exp; Range = Range.span a.Range b.Range }

    /// A node covering all of the given nodes; synthetic when the list is empty.
    let spanningAll (nodes: Node list) (exp: Exp) =
        { Exp = exp; Range = nodes |> List.map (fun n -> n.Range) |> Range.spanAll }

    /// The standard placeholder for a name declared without a value yet.
    let unknown = synthetic Unknown

    let map (f: Exp -> Exp) (node: Node) = { node with Exp = f node.Exp }
