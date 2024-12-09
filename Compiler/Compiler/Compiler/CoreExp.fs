module CoreExp

open System.Collections.Generic
open System


type PairDirection = | To | From


type IdentifierSet = System.Collections.Generic.HashSet<string>


type Scope = 
    | RootScope
    | LetScope of IdentifierSet * Scope
    | RecordScope of IdentifierSet * Scope
    | LambdaScope of IdentifierSet * Scope
    | SetScope of Guid * Scope
    | SetItemScope of Guid * Scope


type Identifier =
    | Name of string
    | Scoped of Scope * string


type LiteralValue = 
    | IntLiteral of int
    | StringLiteral of string


type SetId = Guid option


type SyntaxExp =
    | Identifier of Identifier
    | Let of SyntaxExp
    | In of SyntaxExp * SyntaxExp
    | Reference of SyntaxExp
    | List of SyntaxExp list
    | Tuple of SyntaxExp list
    | Record of SyntaxExp list
    | Pair of SyntaxExp * SyntaxExp * PairDirection
    | Lambda of SyntaxExp * SyntaxExp * PairDirection
    | Set of SyntaxExp list
    | Class of SyntaxExp
    | Apply of SyntaxExp * SyntaxExp
    | Property of SyntaxExp * Identifier
    | MemberProperty of SyntaxExp * Identifier
    | Literal of LiteralValue
    | This of Guid option
    | These of Guid option
    | Discard
    | Eq of SyntaxExp * SyntaxExp
    | And of SyntaxExp * SyntaxExp
    | Or of SyntaxExp * SyntaxExp
    | Not of SyntaxExp


let pop (decl:IdentifierSet list) : IdentifierSet list =
    match decl with
    | [] -> []
    | _::rest -> rest


let rec findThese (scope:Scope) =
    match scope with 
    | LetScope (_,scope) -> findThese scope
    | RecordScope (_,scope) -> findThese scope
    | LambdaScope (_,scope) -> findThese scope
    | SetScope (id,_) -> id
    | SetItemScope (_,scope) -> findThese scope
    | RootScope -> invalidOp "No set in scope"


let rec findThis (scope:Scope) =
    match scope with 
    | LetScope (_,scope) -> findThis scope
    | RecordScope (_,scope) -> findThis scope
    | LambdaScope (_,scope) -> findThis scope
    | SetItemScope (id,_) -> id
    | SetScope (_,scope) -> findThis scope
    | RootScope -> invalidOp "No set item in scope"


let scopeIdentifier (scope:Scope) (decl:IdentifierSet list) (name:string) : Identifier =
    if (decl=[]) 
    then Scoped(scope, name)
    else let _ = decl.Head.Add name in Scoped(scope, name)


let rec scopeIdentifiers (scope:Scope) (decl:IdentifierSet list) (exp:SyntaxExp) : SyntaxExp =
    match exp with
    | Identifier(Name name) -> Identifier( scopeIdentifier scope decl name )
    | Identifier _ -> invalidOp "Unexpected"
    | Let exp -> let ids = new IdentifierSet() in let scope=LetScope(ids,scope) in scopeIdentifiers scope (ids::decl) exp
    | In (left,right) -> In( scopeIdentifiers scope decl left, scopeIdentifiers scope (pop decl) right)
    | Reference exp -> scopeIdentifiers scope [] exp
    | List exps -> List( exps |> List.map (scopeIdentifiers scope decl) )
    | Tuple exps -> Tuple( exps |> List.map (scopeIdentifiers scope decl) )
    | Record(exps) -> let ids = new IdentifierSet() in let scope=RecordScope(ids,scope) in Record( exps |> List.map (scopeIdentifiers scope (ids::decl) ))
    | Pair(exp1,exp2,dir) -> Pair( scopeIdentifiers scope decl exp1, scopeIdentifiers scope (pop decl) exp2, dir )
    | Lambda(exp1,exp2,dir) -> let ids=new IdentifierSet() in let scope=LambdaScope(ids,scope) in Lambda( scopeIdentifiers scope (ids::decl) exp1, scopeIdentifiers scope decl exp2, dir)
    | Set exps -> let scope=SetScope(Guid.NewGuid(),scope) in Set( exps |> List.map (let scope=SetItemScope(Guid.NewGuid(), scope) in scopeIdentifiers scope decl))
    | Class exp -> Class( scopeIdentifiers scope decl exp )
    | Apply(fn,arg) -> Apply( scopeIdentifiers scope (pop decl) fn, scopeIdentifiers scope decl arg )
    | Property(exp, Name name) -> Property( scopeIdentifiers scope (pop decl) exp, scopeIdentifier scope decl name )
    | Property(exp, _) -> invalidOp "Unexpected"
    | MemberProperty(exp,Name name) -> MemberProperty( scopeIdentifiers scope (pop decl) exp, scopeIdentifier scope decl name )
    | MemberProperty(exp, _) -> invalidOp "Unexpected"
    | Literal value -> Literal value
    | This _ -> This (Some (findThis scope))
    | These _ -> These (Some (findThese scope))
    | Discard -> Discard
    | Eq(left,right)  -> Eq(scopeIdentifiers scope decl left, scopeIdentifiers scope (pop decl) right)
    | And(left,right) -> And(scopeIdentifiers scope decl left, scopeIdentifiers scope decl right)
    | Or(left,right) -> Or(scopeIdentifiers scope decl left, scopeIdentifiers scope decl right)
    | Not(exp) -> Not(scopeIdentifiers scope decl exp)


