module Analyzer

open Exp
open CoreExp

let rec markDeclarations (exp:Exp) (scopes:ScopeStack) : CoreExp =
    match exp with
    | Exp.Local(s, e)                                   -> CoreExp.Local(s, markDeclarations e scopes)
    | Exp.Declarative(s, e)                             -> markDeclarations e (addDeclarative scopes s)
    | Exp.Referential(s, e)                             -> markDeclarations e (addReferential scopes s)
    | Exp.Apply(f, a)                                   -> CoreExp.Apply(markDeclarations f scopes, markDeclarations a scopes)
    | Exp.Reference(e)                                  -> markDeclarations e (makeReferential scopes)
    | Exp.Publish(s, access, e)                         -> let s = newScope() in CoreExp.Publish(s, access, markDeclarations e (addDeclarative scopes s))
    | Exp.Tuple(items)                                  -> CoreExp.Tuple(List.map (fun e -> markDeclarations e scopes) items)
    | Exp.Record(s,fields)                              -> CoreExp.Record(s, List.map (fun e -> markDeclarations e (addDeclarative scopes s)) fields)
    | Exp.Identifier(parts)                             -> CoreExp.Identifier(bindingRole scopes, List.map (fun p -> markDeclarationsPart p scopes) parts)
    | Exp.BuiltIn(name)                                 -> CoreExp.Identifier(BindingRole.Referential [RootScope], [TextPart name])
    | Exp.Literal(lit)                                  -> CoreExp.Literal(lit)
    | Exp.Set(items)                                    -> CoreExp.Set(List.map (fun e -> markDeclarations e scopes) items)
    | Exp.List(items)                                   -> CoreExp.List(List.map (fun e -> markDeclarations e scopes) items)
    | Exp.InterpolatedString(parts)                     -> CoreExp.InterpolatedString(List.map (fun p -> markDeclarationsPart p scopes) parts)
    | Exp.Projection(s,e,m)                             -> CoreExp.Projection(s, markDeclarations e scopes, markDeclarations m scopes)
    | Exp.MemberProjection(s,e, m)                      -> CoreExp.MemberProjection(s, markDeclarations e scopes, markDeclarations m scopes)
    | Exp.This                                          -> CoreExp.This(bindingRole scopes)
    | Exp.Void                                          -> CoreExp.Void

and markDeclarationsPart (part:InterpolatedPart) (scopes:ScopeStack) : CoreInterpolatedPart =
    match part with
    | InterpolatedPart.TextPart text  -> CoreInterpolatedPart.TextPart text
    | InterpolatedPart.HolePart exp   -> CoreInterpolatedPart.HolePart(markDeclarations exp scopes)




