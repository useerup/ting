### Logical operators

#### `&` strict conjunction

Strict (non-short-circuit) conjunction evaluates both operands; if either side is undefined the result is undefined.

| left        | right       | result     |
|-------------|-------------|------------|
| `true`      | `true`      | `true`     |
| `true`      | `false`     | `false`    |
| `true`      | *undefined* | *undefined* |
| `false`     | `true`      | `false`    |
| `false`     | `false`     | `false`    |
| `false`     | *undefined* | *undefined* |
| *undefined* | `true`      | *undefined* |
| *undefined* | `false`     | *undefined* |
| *undefined* | *undefined* | *undefined* |


#### `&&` guarded conjunction

If the left operand is false, any undefinedness from the right operand is caught and the result is `false`. If the left operand is `true`, the result equals the right operand. If the left operand is *undefined*, the result is *undefined*.

| left        | right       | result      |
|-------------|-------------|-------------|
| `true`      | `true`      | `true`      |
| `true`      | `false`     | `false`     |
| `true`      | *undefined* | *undefined* |
| `false`     | `true`      | `false`     |
| `false`     | `false`     | `false`     |
| `false`     | *undefined* | `false`     |
| *undefined* | `true`      | *undefined* |
| *undefined* | `false`     | *undefined* |
| *undefined* | *undefined* | *undefined* |

#### `&&&` mutually guarded conjunction

Mutually guarded conjunction: if either operand is `false`, the result is `false` and any undefinedness on the other side is caught. If both are `true`, the result is `true`. Otherwise the result is *undefined*.

| left        | right       | result      |
|-------------|-------------|-------------|
| `true`      | `true`      | `true`      |
| `true`      | `false`     | `false`     |
| `true`      | *undefined* | *undefined* |
| `false`     | `true`      | `false`     |
| `false`     | `false`     | `false`     |
| `false`     | *undefined* | `false`     |
| *undefined* | `true`      | *undefined* |
| *undefined* | `false`     | `false`     |
| *undefined* | *undefined* | *undefined* |

#### `|` strict disjunction

Strict (non-short-circuit) disjunction evaluates both operands; if either side is undefined the result is *undefined*.

| left        | right       | result      |
|-------------|-------------|-------------|
| `true`      | `true`      | `true`      |
| `true`      | `false`     | `true`      |
| `true`      | *undefined* | *undefined* |
| `false`     | `true`      | `true`      |
| `false`     | `false`     | `false`     |
| `false`     | *undefined* | *undefined* |
| *undefined* | `true`      | *undefined* |
| *undefined* | `false`     | *undefined* |
| *undefined* | *undefined* | *undefined* |

#### `||` guarded disjunction

If the left operand is `true`, the value of the right operand does not matter and any undefinedness from evaluating the right operand is caught. If the left operand is `false`, the result equals the right operand. If the left operand is *undefined*, the result is *undefined*.

| left        | right       | result      |
|-------------|-------------|-------------|
| `true`      | `true`      | `true`      |
| `true`      | `false`     | `true`      |
| `true`      | *undefined* | `true`      |
| `false`     | `true`      | `true`      |
| `false`     | `false`     | `false`     |
| `false`     | *undefined* | *undefined* |
| *undefined* | `true`      | *undefined* |
| *undefined* | `false`     | *undefined* |
| *undefined* | *undefined* | *undefined* |

#### `|||` mutually guarded disjunction

Mutually guarded disjunction: if either operand is `true`, the result is `true` and any undefinedness on the other side is caught. If both are `false`, the result is `false`. Otherwise the result is *undefined*.

| left        | right       | result      |
|-------------|-------------|-------------|
| `true`      | `true`      | `true`      |
| `true`      | `false`     | `true`      |
| `true`      | *undefined* | `true`      |
| `false`     | `true`      | `true`      |
| `false`     | `false`     | `false`     |
| `false`     | *undefined* | *undefined* |
| *undefined* | `true`      | `true`      |
| *undefined* | `false`     | *undefined* |
| *undefined* | *undefined* | *undefined* |

#### `!` not 

