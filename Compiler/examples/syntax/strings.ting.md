# Strings

Regular strings and escape sequences.

```ting
"Hello, world!"
```

```ting-output
"Hello, world!"
```

```ting
"tab\there, quote \" and backslash \\ \u00e9 \U0001F600 \x41"
```

```ting-output
"tab\there, quote \" and backslash \\ é \U0001f600 A"
```

```ting
"unknown \q escape"
```

```ting-output
error (1,10-1,12): unrecognised escape sequence
```

```ting
"no newline
allowed"
```

```ting-output
error (1,1-1,12): unterminated string literal
```

Verbatim strings.

```ting
@"C:\Users\alice\Documents"
```

```ting-output
"C:\\Users\\alice\\Documents"
```

```ting
@"She said ""hello""."
```

```ting-output
"She said \"hello\"."
```

```ting
@"line one
line two"
```

```ting-output
"line one\nline two"
```
