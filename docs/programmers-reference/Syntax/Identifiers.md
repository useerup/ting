### Identifiers

Ting has three ways to write identifiers, depending on how much flexibility you need:

- Regular identifiers (non-quoted)
- Quoted identifiers
- Interpolated identifiers

The formal rules are in the specification. This page gives the same behavior in a more practical style.

### Regular identifiers (non-quoted)

Use these for normal names:

```ting
name
_temp
Count2
```

- The first character must be a Unicode letter or `_`.
- Remaining characters can be Unicode letters, digits, or `_`.

### Quoted identifiers

Use quoted identifiers when a name contains spaces, punctuation, keywords, or other characters that do not fit regular-identifier rules.

```ting
`first name`
``foo`bar``
```

- Start with one or more backticks.
- End with the same number of backticks.
- The terminator is the first exact-length backtick run that is not part of a longer run.
- Backtick runs of other lengths inside the name are literal text.
- If there is exactly one leading space after the opener and exactly one trailing space before the closer, both are trimmed.

### Interpolated identifiers

Interpolated identifiers are quoted identifiers with one or more `$` before them:

```ting
$`Name{n}`
$$`Name{{n}}`
$$$`Field{{{index}}}`
```

- There must be no whitespace between the last `$` and the opening backtick.
- If you use N dollar signs, each replacement block must use N braces on each side.
- `N = 1`: `{expr}`
- `N = 2`: `{{expr}}`
- In general: N dollars means N-brace delimiters.
- Braces that do not form valid N-brace replacement blocks are treated as literal braces.
- The final identifier text is built by concatenating literal segments with evaluated replacement values.


With a few exceptions, interpolated identifiers **must** be fully bound at compile time.

>TODO: Explain how interpolated identifiers can remain not bound, as long as the compiler can assure that they **will** be bound upon using them.
