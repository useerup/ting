module ParseResult

open Exp

/// Parser-neutral outcome of parsing Ting source into the raw AST.
///
/// The compiler depends on this stable contract rather than on any specific
/// parser implementation (or its result type). That keeps the current FParsec
/// parser swappable in the future — e.g. for a self-hosted parser written in
/// Ting once the compiler can dogfood itself.
type ParseResult =
    | ParseSucceeded of Exp
    | ParseFailed of string
