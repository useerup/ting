module Exp

open System

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
///     ReferenceLeft/Right, BuiltIn, PushScope, PushMemberScope), and
///   - identifiers in the Unbound binding role.
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
///   BuiltIn          Never written in source; synthesized as the operator/function head
///                    for every operator desugaring (`_+_`, `_=>_`, `!_`, `fn_`, ...).
///                    Lowered by the Binder to a root-scope Referential identifier.
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
///                    emits Unbound; the Binder rewrites to Declarative/Referential.
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
    | Declare of Exp
    | Reference of Exp
    | DeclareLeft of Exp * Exp * Exp
    | DeclareRight of Exp * Exp * Exp
    | ReferenceLeft of Exp * Exp * Exp
    | ReferenceRight of Exp * Exp * Exp
    | BuiltIn of string
    | PushScope of Exp * Exp
    | PushMemberScope of Exp * Exp
    // Shared nodes (parser output, preserved by the Binder).
    | Publish of Scope * AccessModifier * Exp
    | Apply of Exp * Exp
    | Tuple of Exp list
    | Record of Scope * Exp
    | Identifier of BindingRole
    | Literal of Literal
    | Set of Exp list
    | List of Exp list
    | StringLiteral of InterpolatedParts
    | Projection of Scope * Exp * Exp
    | MemberProjection of Scope * Exp * Exp
    | This of BindingRole
    | Void
    // Analyzed-only nodes (produced by the Binder and later passes).
    | Local of Scope * Exp
    | ScopeExp of Scope * Exp
    | Unknown
    | Throw of Exp
    | Catch of Exp * Exp
    | IsDefined of Exp
    | Equals of Exp * Exp
    | ValueEquals of Exp * Exp

and InterpolatedPart =
    | TextPart of string
    | HolePart of Exp

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
    | DirectPropertyHost of Exp
    | MemberPropertyHost of Exp

and NameEntry = {
    NameParts: InterpolatedParts
    Expression: Exp
    Host: NameHost option
}

// Name table for a scope: declaration id -> analyzed name entry.
and Names = System.Collections.Generic.Dictionary<Guid, NameEntry>

// Scope hierarchy.
and [<AbstractClass>] Scope(id: Guid, names: Names) =
    member _.Id = id
    member _.Names = names
    member this.defineName (id: Guid, name: string, exp: Exp) : unit =
        this.Names.Add(id, { NameParts = [ TextPart name ]; Expression = exp; Host = None })
    member this.defineName (id: Guid, name: InterpolatedParts, exp: Exp) : unit =
        this.Names.Add(id, { NameParts = name; Expression = exp; Host = None })
    member this.defineName (id: Guid, name: InterpolatedParts, exp: Exp, host: NameHost) : unit =
        this.Names.Add(id, { NameParts = name; Expression = exp; Host = Some host })

and [<AbstractClass>] ChildScope(id: Guid) =
    inherit Scope(id, new Names())

and LocalScope(id: Guid) =
    inherit Scope(id, new Names())

and ExpressionScope(id: Guid, expression: Exp) =
    inherit ChildScope(id)
    member _.Expression = expression

and ExpressionMemberScope(id: Guid, expression: Exp) =
    inherit ChildScope(id)
    member _.Expression = expression

// Declarative scopes (names being introduced) and referential scopes
// (candidates for name lookup), nearest first.
type ScopeStack = Scope list * Scope list

let scopeId (s: Scope) = s.Id
