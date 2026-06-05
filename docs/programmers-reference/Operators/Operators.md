## Operators

Binary operators can be categorized as follows:

| pattern           | in declarative context                                                                 | in referential context                                                                                             |
| ----------------- | -------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------ |
| left-declarative  | Creates a local scope spanning the operator expression. Both operands are declarative. | Creates a local scope spanning the operator expression. Left operand is declarative; right operand is referential. |
| right-declarative | Creates a local scope spanning the operator expression. Both operands are declarative. | Creates a local scope spanning the operator expression. Right operand is declarative; left operand is referential. |
| left-referential  | Left operand is referential; right operand is declarative.                             | Both operands are referential.                                                                                     |
| right-referential | Left operand is declarative; right operand is referential.                             | Both operands are referential.                                                                                     |
| non-declarative   | Both operands are declarative.                                                         | Both operands are referential.                                                                                     |

The publishing operators (`public`, `private`, `internal`, `protected`) each creates a scope for which the operand expression is declarative. The operators can be nested. In that case the rule about overlapping declarative contexts comes into play, which means that identifiers that are declared in multiple contexts are unified. This effectively means that `protected internal int Answer` declares `Answer` as both a *protected* identifier and an *internal* identifier, and that both declarations refer to the same value.


### Arithmetic operators

#### `+` addition

Signatures: 
- `Numeric*Numeric^^Numeric` where `Numeric:{int,float,double,byte,sbyte,long,ulong,short,ushort,uint,decimal}`
- `string*string^^string`

#### `-` subtraction

#### `*` multiplication

#### `/` division

#### `^` eksponentiation

#### `%` remainder

#### `/%` division with remainder

### Exception handling operators

#### `catch` 

### Choice operators

#### `;` choice

#### `;;` exclusive choice

### Class operators

#### `class` class constructor

#### `extend` class extension

### Relational operators

#### `<` less than / proper subset

#### `<=` less than or equal to / subset

#### `==` equivalent

#### `>=` greater than or equal to / superset

#### `>` greater than / proper superset

#### `!=` not equivalent

#### `:`

#### `::`

### Declaration operators

#### `let` local declaration

#### `public` public declaration

#### `private` private declaration

#### `internal` internal declaration

#### `protected` protected declaration

### Signature operators

#### `flow` supported bindings

#### `axiom`

### Functional operators

#### `=>` Lambda

The `=>` operator creates a new function. The left operand is the argument and the right operand is the result of an application.

`=>` is *left-declarative*. 

#### `->` 

#### `-->` 

#### `>>` 

#### `<<` 

#### `|>` 

#### `<|` 

#### `**` 

### Range operators

#### `...` inclusive-inclusive range

#### `<...` exclusive-inclusive range

#### `...<` inclusive-exclusive range

#### `<...<` exclusive-exclusive range

### Projection operators

#### `.` property projection

#### `..` element property projection


### Expression-list folding operators

#### `>>>` expression-list fold left-to-right

#### `<<<` expression-list fold left-to-right


### Structure operators

#### `enum` and `enums`

#### `record` and `records`

#### `tuple` and `tuples`


### Expression operators

#### `$` expression prefix


### Restriction operators

#### `?` but only if

#### `\` such that

#### `??` where


### Scope operators

#### The `./` *property scope* operator

#### The `../` *member property scope* operator



