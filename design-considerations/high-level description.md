# **Ting: A Language of Lexical Constraints, Controlled Nondeterminism, and Symbolic Sets**

Ting is a declarative programming language built around **lexically‑scoped constraint regions**, **explicit nondeterminism**, and **symbolic reasoning** guided by a **DPLL‑style proof engine**. Its design goal is to make nondeterministic and relational programming *explicit, predictable, and compilable*, while keeping the core language small and pushing domain knowledge into libraries.

Ting is not a logic language in the Prolog sense, nor a functional language with effects. It is a language where **expressions denote values**, but **evaluation may involve nondeterministic branching**, constraint solving, and symbolic reasoning. The compiler is responsible for resolving all nondeterminism—either operationally (enumeration) or symbolically (proof/refutation). If it cannot, compilation fails.

---

# **1. Scopes, Variables, and Constraint Regions**

Ting has two kinds of scopes:

### **1. Published scopes**
Created by top‑level declarations such as:

```
private Answer = 42
```

Published identifiers form a global namespace. They may be shadowed by local scopes.

### **2. Local scopes**
Created by:

- `let E`
- lambda bodies `x => E`

Local scopes define **constraint regions**. Within a region:

- Declarative occurrences of the same name refer to the **same variable**.
- Local scopes **cannot shadow** identifiers from enclosing local scopes.
- Local scopes **can shadow** published identifiers.

Example:

```
let a = 1 in
  let a = 2 in a     -- compile‑time error (illegal shadowing)
```

But:

```
private a = 1
let a = 2 in a       -- OK (local shadows published)
```

Variable identity is determined by **name + lexical region**. Overlapping regions unify; disjoint regions do not.

This gives Ting a predictable, region‑based constraint model without accidental unification across modules or distant code.

---

# **2. Nondeterminism as an Explicit Effect**

Ting distinguishes two forms of nondeterminism:

## **2.1 Enumerable nondeterminism**
Introduced by binding a variable to an **explicit finite set**:

```
x : {1,2,3}
```

This means: *x may take any of these values*.  
Any expression mentioning `x` becomes nondeterministic.

Enumerable nondeterminism is the only kind the compiler is allowed to **run** (enumerate) during code generation.

## **2.2 Unenumerable nondeterminism**
Introduced by binding to an **infinite‑in‑principle domain**:

```
x : int
```

Ting treats primitive domains like `int` as unbounded, even if implemented as 32‑bit integers.  
Thus:

- `x : int` is nondeterministic but **not enumerable**.
- The compiler will never brute‑force search such a domain.
- Constraints may narrow the domain symbolically, but never enough to make it enumerable unless a library provides a solver.

Example:

```
let x : int in x + 1
```

If `x` cannot be resolved to a concrete value or finite set, compilation fails.

---

# **3. Sets as Values, Not Computations**

Ting separates **nondeterministic computation** from **set values**.

## **3.1 Concrete sets**
Finite, explicit, enumerable:

```
{1,2,3}
{ x \ x : {1..10} }
```

These may be materialized and iterated.

## **3.2 Symbolic sets**
Defined by comprehensions over unenumerable domains:

```
{ x \ x : int && x < 10 }
{ int x \ x % 2 == 0 }
```

These sets:

- Are not materialized.
- May be infinite.
- Support membership testing (`v ∈ S`) by evaluating the predicate symbolically.
- Are usable in symbolic reasoning (`all`, subset, equality).

Symbolic sets behave like **predicates packaged as values**.

---

# **4. The `all` Operator: Operational vs Symbolic Quantification**

Ting’s `all` operator expresses universal quantification over a set:

```
S all x => P(x)
```

It has two modes:

## **4.1 Operational `all`**
Allowed when `S` is **enumerable**.

The compiler generates code to iterate over all elements and check the predicate.

## **4.2 Symbolic `all`**
Allowed when `S` is **symbolic**—but only if the compiler can **prove or refute** the universal statement using its proof engine.

Example:

```
{ int x \ x % 2 == 0 } all x => x % 2 == 0   -- provable
```

But:

```
{ int x \ x % 2 == 0 } all x => x > 0        -- refutable
```

And:

```
{ int x \ x % 2 == 0 } all x => x*x >= 0     -- provable if arithmetic library exists
```

If the compiler cannot prove or refute the statement within its bounded search limits, compilation fails.

---

# **5. The Proof Engine: DPLL(T) with Bounded Search**

Ting’s compiler uses a **DPLL‑style SAT solver** extended with **theory plugins** supplied by libraries.

### **5.1 Boolean skeleton**
Constraints are abstracted into propositional form.  
DPLL handles:

- unit propagation  
- pure literal elimination  
- branching (“split rule”)  

Branching corresponds directly to conditional code generation.

### **5.2 Theory solvers**
Libraries may provide:

- arithmetic solvers  
- algebraic solvers  
- set solvers  
- domain reducers  

Example:

```
let 5*x^2 + 10*x - 175 = 0 in x
```

Without a quadratic solver library → compile‑time error.  
With one → domain reduces to a finite set → enumerable nondeterminism.

### **5.3 Bounded search**
To avoid nontermination:

- depth limits  
- step limits  
- timeouts  
- cycle detection  

If the solver cannot decide within bounds → compile‑time error.

---

# **6. Compilation Model**

Ting is a **compile‑time nondeterministic language**:

- All nondeterminism must be resolved at compile time.
- Either by enumeration (finite sets) or symbolic reasoning (proof/refutation).
- If nondeterminism remains unresolved → compilation fails.

This ensures that **generated code is deterministic**, unless the programmer explicitly introduces runtime nondeterminism (a feature Ting may or may not support).

---

# **7. Design Philosophy**

Ting’s design is guided by three principles:

### **1. Nondeterminism must be explicit and controlled**
No implicit search.  
No hidden backtracking.  
Programmer intent determines enumerability.

### **2. The compiler should orchestrate reasoning, not perform it**
The core solver is intentionally small.  
Libraries provide domain knowledge.

### **3. Failure to decide is a compile‑time error**
If the compiler cannot:

- enumerate  
- prove  
- refute  

then the program is rejected.

This keeps semantics predictable and avoids accidental exponential blowups.

---

# **8. What Ting Feels Like**

To a programmer, Ting feels like:

- A **constraint language** with lexical scoping.
- A **logic language** where nondeterminism is explicit and finite by default.
- A **functional language** where expressions denote values.
- A **proof‑aware language** where some expressions become proof obligations.
- A **compilable language** where all nondeterminism must be resolved before codegen.

It is closest in spirit to:

- Datalog with local scopes  
- miniKanren with explicit domains  
- a dependently‑typed language with partial automation  
- a SAT/SMT‑guided functional language  

—but it is not identical to any of these.

Ting is its own thing.

---

If you want, I can now help you write:

- **A formal semantics document**  
- **A whitepaper‑style introduction**  
- **A “Why Ting?” motivation for language designers**  
- **A comparison to Prolog, miniKanren, and SMT‑based languages**  

Just tell me which direction you want to go.