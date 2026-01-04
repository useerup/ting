# Scopes 

In Ting, there is no separate syntax for declarations: Declaration of identifiers happens within the expressions in which they occur. Identifiers in expressions may either *declare* the identifier or *reference* a declaration of that identifier in an enclosing scope. 

The expression `count : int` may *declare* `count` or it may *reference* it. It depends on the context is which the expression appears.

- When the context is *referential*, then both `count` and `int` are *references* to those identifiers, which must be declared elsewhere in a surrounding scope.

- When the context is *declarative*, then `count` is declared by the expression. The scope in which it ois declared depends on the context.

In both cases, the semantics of the expression is the same: It is a boolean expression which returns `true` if `count` is a member of `int`, `false` otherwise.

## Scope operators

In general, when an expression is in a declarative scope, then all sub-expressions are in the same declarative scope. When the expression in declarative scope is just an identifier, that identifier is declared within that scope. Thus, identifiers can be declared by arbitrarily deep expressions, as long as the declarative scope not broken.

As an example, an expression such as `(a * b + c * d) / d` in a declarative scope declares all of `a`, `b`, `c` and `d` in that scope.

However, there are a few exceptions to this general rule.

- **Relational operators are *right-referential***. Operators `=`, `<`, `<=`, `==`, `!=`, `>=`, `>`, `:`, `::`, `:::` continue a declarative scope *on the left* but are *referential* on the right. When an expression such as `s : string` is in a declarative scope, only `s` is declared in that scope; the identifier `string` is a *reference* to a declaration in an enclosing scope.

- **The property operators `.` and `..` are *left-referential***. You can declare a new property on an existing object using these operators.

- **Function application expressions are *left-referential***. A function application in declarative scope continues the declarative scope through the *argument* while the *function* is referential. When an expression containing a function application such as `f x = 42` is in a declarative scope, only `x` is declared in that scope, while `f` is a reference, i.e., `f` must be declared somewhere else.

- **The `in` operator has *nested-right scope***. The scope of the right operand is a *sub-scope* of the left operand. If the left operand declares local identifiers, the same identifiers are available to be *referenced* in the right operand. The `in` operator does not by itself start a declarative scope, but it can be combined with `let`, which *does* start a new declarative scope, in which the righ operand is then evaluated. Example: `let x=42 in x/7`.

- **The lambda arrow `->`, ordered pair `-->` and condition `\` operators are *left declarative***. These operators starts a new local scope which is declared by the left operand. The right operand is evaluated in this local scope. In the expression `int x -> x * 2`, `int x` is declarative in the local scope. Per the function application rule, `x` is thus declared. The expression `int d\(d%2==0)` is a non-deterministic value of even integers, i.e. it may assume any even integer value.

- **Operators, literals and keywords are not declarative**. If any of these appears as declarative expression, they do not declare anything in the scope.

**Tuples**
Tuples follow the general rule. In declarative scope, all of the expressions in the expression list which defines the tuple are declarative expressions of that scope. Thus, `let (x,y) = coordinate` effectively *deconstructs* the tuple `coordinates`.

**Records**
A record instance constructor `(.`...`.)` and record set constructor `{.`...`.}` accept a list of expressions which are all declarative. The identifiers they declare are the record fields.

    (. Name="Zaphod", Age=42 .)     // record with fields `Name` and `Age`

    {. Name:string, Age:int .}      // set of records

This illustrates the combination of declarative record expressions and relational operator expressions. All expressions within `(.`...`.)` and `{.`...`.}` are *declarative* and identifiers being declared by these expressions are the *fields* of the records. The relational operators `=` and `:` continue the declarative scope only on the left. Thus only `Name` and `Age` become fields.

