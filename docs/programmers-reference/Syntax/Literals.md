## Literals

This page describes the literal forms used in Ting source code.

The formal grammar currently specifies string literal details in depth, while numeric literals are specified at a higher level (integer and floating forms). This guide keeps close to the specification and existing examples.

### String literals

String literals produce values of type `string`.

#### Regular strings

Regular strings use double quotes:

```ting
"Hello, world"
"Line 1\nLine 2"
```

- Open with `"` and close at the next unescaped `"` on the same line.
- Escape sequences are processed.
- Newline characters are not allowed directly inside the literal.

Recognized escapes:

- `\\` backslash
- `\"` quote
- `\n`, `\r`, `\t`, `\a`, `\b`, `\f`, `\v`, `\0`
- `\uHHHH`, `\UHHHHHHHH`, `\xH[H[H[H]]]`

#### Verbatim strings

Verbatim strings use `@"..."`:

```ting
@"C:\Users\alice\Documents"
@"She said ""hello""."
```

- Escapes are not processed.
- Write `"` as `""`.
- Newlines are allowed.

#### Raw strings

Raw strings use 3 or more quotes as delimiters:

```ting
"""
	line one
	line two
	"""
```

- If the opener uses K quotes, the closer must use exactly K quotes.
- Shorter quote runs inside content are literal text.
- Escapes are not processed.
- Closing-line indentation defines the indentation stripped from content lines.

#### Interpolated strings

Interpolated strings add one or more `$` immediately before a string literal form:

```ting
$"Hello, {name}!"
$$"Value: {{x + 1}}"
$@"Path: {basePath}\file.txt"
$"""
Name: {name}
"""
```

- If you use N dollar signs, each replacement block uses N braces on each side.
- `N = 1` uses `{expr}`.
- `N = 2` uses `{{expr}}`.
- Replacement expressions are evaluated and converted to string values.

### Char literals

Char literals use single quotes and represent one Unicode character:

```ting
'A'
'0'
```

You will commonly see char literals in ranges and character sets:

```ting
{ '0'...'9' }
```

### Bool literals

Bool literals are the keywords:

```ting
true
false
```

These are the two values of the `bool` set.

### Number literals

The language specification distinguishes integer and floating forms, and numeric separators (`_`) are supported.

```ting
42
667_776
3.14
```

Numeric literal types:

#### `int` literals

`int` literals are signed 32-bit integer literals.

Example:

```ting
123
```

#### `uint` literals

`uint` literals are unsigned 32-bit integer literals.

Example:

```ting
123u
```

#### `short` literals

`short` literals are signed 16-bit integer literals.

Example:

```ting
123s
```

#### `ushort` literals

`ushort` literals are unsigned 16-bit integer literals.

Example:

```ting
123us
```

#### `long` literals

`long` literals are signed 64-bit integer literals.

Example:

```ting
123L
```

#### `ulong` literals

`ulong` literals are unsigned 64-bit integer literals.

Example:

```ting
123UL
```

#### `float` literals

`float` literals are single-precision floating-point literals.

Examples:

```ting
123.45
123f
```

#### `double` literals

`double` literals are double-precision floating-point literals.

Examples:

```ting
123d
123.45d
```

#### `decimal` literals

`decimal` literals are base-10 decimal literals.

Example:

```ting
123.45m
```

