# Identifiers

```ting
foo _bar Baz42 ærø
```

```ting-output
(apply (apply (apply foo _bar) Baz42) ærø)
```

Quoted identifiers, from the specification's table.

```ting
[`foo`, ` foo`, ` foo `, `` foo`bar ``, ```foo``bar```, ``` foo ``bar ```]
```

```ting-output
(list foo ` foo` foo `` foo`bar `` ``` foo``bar ``` ``` foo ``bar ```)
```

A quoted identifier may spell a keyword.

```ting
`in` = 1
```

```ting-output
(= `in` 1)
```

```ting
``
```

```ting-output
error (1,1-1,3): unterminated quoted identifier
```

Interpolated identifiers.

```ting
$`Name{n}`
```

```ting-output
$`Name{n}`
```

```ting
$$`Name{{n}}`
```

```ting-output
$`Name{n}`
```

```ting
$$$`Field{{{index}}}`
```

```ting-output
$`Field{index}`
```

A keyword is never an identifier. Here `in` is a binary operator in prefix position, so this is a section.

```ting
in = 1
```

```ting-output
(section in (section = 1))
```
