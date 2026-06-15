## Entities

An *entity* is an object whose identity is independent of its payload value. Two entities created with identical arguments are still distinct — they compare unequal unless they literally refer to the same entity. This is in contrast to classes, where two members are equal whenever their candidate values compare equal and they belong to the same class.

Entities model things that are *irreducibly individual*: a particular person, a specific session, a given order. The fact that two orders have the same contents does not make them the same order.

### The `entity` operator

The `entity` prefix operator defines a new entity set. Its syntax is identical to the `class` operator: it accepts a constructor function, or any set that is automatically promoted to its own identity function.

    Persons = entity record(Name:string, BirthYear:int)

    Sessions = entity string   -- payload is a session-token string

Each entity set is distinct. A member of `Sessions` is not interchangeable with a member of `Persons`, even if both happen to carry the same underlying string or record value.

Like `class`, the operand can be a full constructor function:

    Invoices = entity (string customer, decimal amount) =>
        rec { Customer=customer, Amount=amount }

### Creating members

Members of an entity set are created with `new`, exactly as with classes:

    alice = new Persons("Alice", 1990)
    bob   = new Persons("Alice", 1990)

`alice` and `bob` are **distinct** members of `Persons` even though their payloads are identical. Each call to `new` produces a fresh entity.

### Equality

Two entity values are equal if and only if they are **the same entity** — i.e. they were produced by the same single construction and bound to names that refer to that same instance.

    alice == alice   -- true
    alice == bob     -- false, even though Name and BirthYear are identical

The payload is not consulted during equality comparison. Equality is purely identity-based.

### Payload access

The payload carried by an entity member is accessible through the normal field or member access syntax, depending on what the candidate set produces. It can be inspected, pattern-matched, and used in computations, but it does not affect equality.

    alice.Name      -- "Alice"
    bob.Name        -- "Alice"
    alice == bob    -- still false

### The `Candidates` property

Every entity set has a `Candidates` property that yields the candidate set (or constructor) that was supplied to the `entity` operator. If the entity was built over an existing class, `Candidates` returns that class, including its construction capability.

    Persons.Candidates    -- the record set record(Name:string, BirthYear:int)

### Identity and process boundaries

Within a single process, the compiler may represent entity identity as an in-memory address. This is an unobservable optimization: the semantics are those of a fresh opaque identity token, but the runtime cost is zero.

If the compiler's escape analysis determines that an entity value may cross a process boundary — by being written to a file, sent over a network, or stored in a database — the compiler requires the programmer to declare a *stable identity* for the entity set. This is because an address cannot survive serialization; a stable, programmer-defined ID is needed to preserve identity across boundaries.

The stable identity is declared as part of the entity definition:

    Persons = entity records(Name:string, BirthYear:int) \  identity : Guid

Here `uuid` specifies the type of the stable ID field. When constructing a member that will escape, the ID must be supplied explicitly:

    alice = new Persons(id: someUUID, "Alice", 1990)

If the entity never escapes the current process, the `id` field is managed internally and is invisible to the programmer.

> Note: The split between local and escaping entities is determined at compile time by escape analysis. If a code path exists through which an entity could escape, the compiler will require the stable ID declaration even if that path is rarely taken at runtime. This ensures identity is never silently lost.

### The set of all entity sets (`Entities`)

The built-in set `Entities` contains all entity sets, analogously to `Classes`. It is not enumerable; it can only be used for membership testing.

### Comparison with classes

| | Class | Entity |
|---|---|---|
| Equality basis | Candidate value + class membership | Opaque identity |
| Two instances with same payload | Equal | Distinct |
| Runtime representation (local) | Value | Value + implicit address |
| Runtime representation (escaping) | Value | Value + programmer-declared ID |
| Modeled by | Mathematical set extension | Object identity |

### Examples

This models colors as a fixed set of well-known values. Two color values with the same name *are* the same — a class is the right tool:

    Colors = class string??.(Length > 0)
    Red   = new Colors "red"
    Blue  = new Colors "blue"

This models game sessions, where each session is irreducibly individual even if its initial state is identical to another:

    GameSessions = entity record(Player:string, Level:int)

    s1 = new GameSessions("Ada", 1)
    s2 = new GameSessions("Ada", 1)
    -- s1 != s2: they are different sessions

This models invoices that must survive persistence, requiring a stable ID:

    Invoices = entity rec { Customer:string, Amount:decimal } id uuid

    inv = new Invoices(id: UUID.new, "ACME Corp", 1299.00)
