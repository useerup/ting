## Operators

### Arithmetic operators

#### `+` addition

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

The `=>` creates a new local scope which is declarative in its left operand and referential in its RHS operand.

    string sym => sym+rest => rest

This works because an identifier is resolved by searching the current frame for a binding of the identifier. If a binding is found, the same binding is used. If a binding is *not* found, the current frame is searched for  declarative scopes. If one or more declarative scopes are found, the identifier is bound to each scope.

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



