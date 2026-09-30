# Comments

Comments are whitespace.

```ting
a // line comment
+ b
```

```ting-output
(+ a b)
```

```ting
a /* outer /* nested */ still comment */ + b
```

```ting-output
(+ a b)
```

```ting
a/**/b
```

```ting-output
(apply a b)
```

```ting
"// not a comment"
```

```ting-output
"// not a comment"
```

```ting
a /* unterminated
```

```ting-output
error (1,3-1,5): unterminated block comment
```
