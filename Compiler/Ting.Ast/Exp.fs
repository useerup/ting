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

/// How the positional section of a product literal joins the product.
///
/// Written `@` and `@@` respectively, mirroring the two product set operators:
/// `*` fuses one level deep, `**` does not.
type Fusion =
    /// `@` — the positional section is fused one level deep: its top-level
    /// fields become the positional fields of the product.
    | Fusing
    /// `@@` — the positional section stays a single field, nested whole.
    | NonFusing

// Binding mode of a scope frame for a specific expression region.
type ScopeMode =
    | DeclarativeScope
    | ReferentialScope

/// The abstract syntax tree.
///
/// Every case carries its source Range as its **first** field. There is no
/// wrapper node: an expression and the span it was parsed from are one value.
/// Read the span back with the `Range` member, which is defined over every case,
/// and build a span covering several operands with `Exp.spanning` /
/// `Exp.spanningAll`. Expressions with no source text of their own (builtins,
/// placeholders, some desugaring artifacts) carry `Range.synthetic`.
///
/// Note: do not print a tree with %A. It has no depth limit problem any more,
/// but the ranges drown the structure. Use ExpDiagnostics.renderExpression.
///
/// The parser produces this tree with:
///   - binding-intent marker cases (Reference, DeclareLeft/Right,
///     ReferenceLeft/Right, PushScope, PushMemberScope), and
///   - identifiers in the Unbound binding role, except for the operator heads it
///     synthesizes: the parser knows every operator it can parse, so it emits
///     those already Bound(Referenced) against its own operator scope.
///
/// The Binder (run as part of parsing) rewrites the tree in place-of-type:
///   - marker cases are eliminated (lowered to Local/Apply/scope pushes),
///   - every Unbound identifier becomes Bound(Declared) when its scope stack
///     starts declaratively, or Pending (carrying the ordered moded frame
///     stack, nearest frame first) when classification needs scopes that are
///     populated later.
///
/// The Resolver classifies Pending occurrences to Bound once the scopes they
/// depend on are populated.
///
/// Source patterns per case (the leading Range is omitted throughout):
///
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
///   Product          Parenthesized product literal with a named section,
///                    `(p1, p2 @ n1 = v1, n2 = v2)`; fields are (positional
///                    elements, fusion, named scope, named elements). The
///                    positional elements are ambient, exactly as in a Tuple; the
///                    named elements are declarative and are harvested into the
///                    scope like a Record's. A Tuple is the degenerate case with
///                    no named section.
///   Record           Prefix `record E`. The parser allocates the record's scope; the
///                    Binder fills it with the record's declarations.
///   Identifier       An identifier occurrence carrying its BindingRole. The parser
///                    emits Unbound for identifiers written in source, and the
///                    already-Bound head of each operator desugaring
///                    (`_+_`, `_=>_`, `!_`, `fn_`, ...). The Binder rewrites
///                    Unbound to Bound/Pending and leaves the rest alone.
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
///   Primary          Prefix `primary E` — erases attributes from E. Deep,
///                    total, deterministic, idempotent; preserves class
///                    membership. Not lowered to a function application: the
///                    solver has to be able to invert it, since `primary a =
///                    primary b` must be usable to constrain `a`.
///   Local            A declaration region with its scope. Produced by the parser for
///                    prefix `let E` (the parser allocates the scope; the Binder fills
///                    it), and by the Binder from DeclareLeft/Right.
///   Candidate        Prefix `candidate E` — erases class membership from E,
///                    yielding the candidate value the class was built over.
///                    Preserves attributes.
///
/// Analyzed-only cases (never produced by the parser):
///
///   ScopeExp         An expression annotated with an explicit scope.
///   Unknown          Placeholder expression (e.g. names declared without a value yet).
///   Throw/Catch/IsDefined/Equals
///                    Semantic forms produced by later analysis passes.
///                    There is deliberately no value-equality case: `==`
///                    normalizes to Equals(Primary a, Primary b), and every other
///                    attribute-blind relation wraps its operands in Primary the
///                    same way. Only `=` leaves its operands unwrapped.
type Exp =
    // Surface binding-intent markers (parser output, eliminated by the Binder).
    | Reference of Range * Exp
    | DeclareLeft of Range * Exp * Exp * Exp
    | DeclareRight of Range * Exp * Exp * Exp
    | ReferenceLeft of Range * Exp * Exp * Exp
    | ReferenceRight of Range * Exp * Exp * Exp
    | PushScope of Range * Exp * Exp
    | PushMemberScope of Range * Exp * Exp
    // Shared forms (parser output, preserved by the Binder).
    | Publish of Range * Scope * AccessModifier * Exp
    | Apply of Range * Exp * Exp
    | Tuple of Range * Exp list
    | Product of Range * Exp list * Fusion * Scope * Exp list
    | Record of Range * Scope * Exp
    | Identifier of Range * BindingRole
    | Literal of Range * Literal
    | Set of Range * Exp list
    | List of Range * Exp list
    | StringLiteral of Range * InterpolatedParts
    | Projection of Range * Scope * Exp * Exp
    | MemberProjection of Range * Scope * Exp * Exp
    | This of Range * BindingRole
    | Void of Range
    | Primary of Range * Exp
    | Candidate of Range * Exp
    | Local of Range * Scope * Exp
    // Analyzed-only forms (produced by the Binder and later passes).
    | ScopeExp of Range * Scope * Exp
    | Unknown of Range
    | Throw of Range * Exp
    | Catch of Range * Exp * Exp
    | IsDefined of Range * Exp
    | Equals of Range * Exp * Exp

    /// The source span this expression was parsed from.
    member this.Range =
        match this with
        | Reference (r, _)
        | DeclareLeft (r, _, _, _)
        | DeclareRight (r, _, _, _)
        | ReferenceLeft (r, _, _, _)
        | ReferenceRight (r, _, _, _)
        | PushScope (r, _, _)
        | PushMemberScope (r, _, _)
        | Publish (r, _, _, _)
        | Apply (r, _, _)
        | Tuple (r, _)
        | Product (r, _, _, _, _)
        | Record (r, _, _)
        | Identifier (r, _)
        | Literal (r, _)
        | Set (r, _)
        | List (r, _)
        | StringLiteral (r, _)
        | Projection (r, _, _, _)
        | MemberProjection (r, _, _, _)
        | This (r, _)
        | Void r
        | Primary (r, _)
        | Candidate (r, _)
        | Local (r, _, _)
        | ScopeExp (r, _, _)
        | Unknown r
        | Throw (r, _)
        | Catch (r, _, _)
        | IsDefined (r, _)
        | Equals (r, _, _) -> r

and InterpolatedPart =
    | TextPart of string
    | HolePart of Exp

and InterpolatedParts = InterpolatedPart list

// How an occurrence's binding walk ended: it either contributed a declaration
// or resolved to an existing one. Distinct from ScopeMode, which describes a
// frame during the walk rather than the occurrence's outcome.
and BindingKind =
    | Declared
    | Referenced

// Unbound is the parser's role for every identifier occurrence. The Binder
// rewrites it: Bound is the terminal outcome (kind + declaration id); Pending
// carries the ordered moded frame stack (nearest first) for occurrences whose
// walk needs scopes that are only populated by later analysis. The Resolver
// classifies Pending to Bound.
and BindingRole =
    | Unbound of InterpolatedParts
    | Pending of ScopeFrame list * InterpolatedParts
    | Bound of BindingKind * Guid * InterpolatedParts

// A scope together with its binding mode at a particular point in the
// expression. The stack of these frames is what name binding walks.
and ScopeFrame = {
    Scope: Scope
    Mode: ScopeMode
}

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

// Ordered scope stack, nearest first. Each frame keeps its own mode so nested
// operators can mix declarative and referential behavior without losing frame
// order.
type ScopeStack = ScopeFrame list

let scopeId (s: Scope) = s.Id

/// The span covering two expressions. Used when a desugaring synthesizes a
/// parent (e.g. the Tuple in an infix operator lowering) that has no token of
/// its own but covers its operands.
let spanning (a: Exp) (b: Exp) : Range = Range.span a.Range b.Range

/// The span covering all of the given expressions; synthetic when the list is empty.
let spanningAll (exps: Exp list) : Range =
    exps |> List.map (fun e -> e.Range) |> Range.spanAll

/// The standard placeholder for a name declared without a value yet.
let unknown = Unknown Range.synthetic

/// Re-range an expression, keeping its shape. Needed where a desugaring builds a
/// tree bottom-up and only then learns the span of the construct it came from.
let withRange (range: Range) (exp: Exp) : Exp =
    match exp with
    | Reference (_, e) -> Reference(range, e)
    | DeclareLeft (_, h, a, b) -> DeclareLeft(range, h, a, b)
    | DeclareRight (_, h, a, b) -> DeclareRight(range, h, a, b)
    | ReferenceLeft (_, h, a, b) -> ReferenceLeft(range, h, a, b)
    | ReferenceRight (_, h, a, b) -> ReferenceRight(range, h, a, b)
    | PushScope (_, a, b) -> PushScope(range, a, b)
    | PushMemberScope (_, a, b) -> PushMemberScope(range, a, b)
    | Publish (_, s, access, e) -> Publish(range, s, access, e)
    | Apply (_, f, a) -> Apply(range, f, a)
    | Tuple (_, xs) -> Tuple(range, xs)
    | Product (_, positional, fusion, s, named) -> Product(range, positional, fusion, s, named)
    | Record (_, s, e) -> Record(range, s, e)
    | Identifier (_, role) -> Identifier(range, role)
    | Literal (_, lit) -> Literal(range, lit)
    | Set (_, xs) -> Set(range, xs)
    | List (_, xs) -> List(range, xs)
    | StringLiteral (_, parts) -> StringLiteral(range, parts)
    | Projection (_, s, e, m) -> Projection(range, s, e, m)
    | MemberProjection (_, s, e, m) -> MemberProjection(range, s, e, m)
    | This (_, role) -> This(range, role)
    | Void _ -> Void range
    | Primary (_, e) -> Primary(range, e)
    | Candidate (_, e) -> Candidate(range, e)
    | Local (_, s, e) -> Local(range, s, e)
    | ScopeExp (_, s, e) -> ScopeExp(range, s, e)
    | Unknown _ -> Unknown range
    | Throw (_, e) -> Throw(range, e)
    | Catch (_, a, b) -> Catch(range, a, b)
    | IsDefined (_, e) -> IsDefined(range, e)
    | Equals (_, a, b) -> Equals(range, a, b)
