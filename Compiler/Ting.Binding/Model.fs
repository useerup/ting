/// The result of binding names from the program text alone (plan phase 2).
module Ting.Binding.Model

open Ting.Syntax.Source

type FrameId = int

/// Builtin declarations have negative ids.
type DeclId = int

type Mode =
    | Declarative
    | Referential
    /// Declarative in its own operand but made referential by an enclosing operator or a callee position.
    | Forced

type FrameKind =
    | Root
    | Let
    /// `public`, `private`, `protected`, `internal`.
    | Publish of string
    /// Introduced by a declarative operator: `=>`, `\`, `in`, `where`, `all`, `contract`.
    | Local of string
    /// `record`, `records` and the named section of a product: declarations become fields.
    | Fields of string
    /// `tuple`, `tuples`.
    | TupleScope of string
    /// `./` and `../`: the names it provides are known only after semantic analysis.
    | ExpressionScope of string

type Frame =
    { Id: FrameId
      Kind: FrameKind
      At: Position
      Parent: FrameId option }

type DeclKind =
    | Builtin
    | LocalName
    | Published
    | Field
    | ThisBinding
    /// Declared through property access: `host.name` or, for members, `host..name`.
    | Hosted of isMember: bool * host: Span

type Decl =
    { Id: DeclId
      Name: string
      Kind: DeclKind
      Frames: FrameId list
      Span: Span }

/// Why an occurrence cannot be classified from the program text alone.
type DeferReason =
    /// The walk reached a `./` or `../` scope whose names depend on its operand.
    | ScopeContents of FrameId
    /// The name may be a property of the `this` bound in this frame.
    | ImplicitThis of FrameId
    | PropertyName
    | InterpolatedName
    /// Waiting on frames that deferred occurrences may still declare into.
    | Blocked

type Binding =
    | Declares of DeclId
    | References of DeclId
    /// `record(value)`: declares the field and references `value` outside the record.
    | FieldShorthand of field: DeclId * value: Binding
    | Anonymous
    | Deferred of DeferReason
    | Unresolved

type Occurrence =
    { Name: string
      Span: Span
      Binding: Binding }

type Diagnostic = { Message: string; Span: Span }

type Bound =
    { Frames: Frame list
      Decls: Decl list
      Occurrences: Occurrence list
      Diagnostics: Diagnostic list }
