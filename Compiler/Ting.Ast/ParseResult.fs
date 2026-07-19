module ParseResult

open Exp

/// Parser-neutral outcome of parsing Ting source into the analyzed AST.
///
/// The compiler depends on this stable contract rather than on any specific
/// parser implementation (or its result type). That keeps the current FParsec
/// parser swappable in the future — e.g. for a self-hosted parser written in
/// Ting once the compiler can dogfood itself.
///
/// On success the carried Exp is already scope-annotated: the Binder has run
/// as part of parsing, so declarative identifiers carry declaration ids
/// (recorded in their scopes) and referential identifiers carry their ordered
/// candidate scope-id lists.
type ParseResult =
    | ParseSucceeded of Exp
    | ParseFailed of string
