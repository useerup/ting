# Sections

A binary operator in prefix position denotes partial application.

```ting
( : rhs)
```

```ting-output
(section : rhs)
```

```ting
.Name
```

```ting-output
(section . Name)
```

```ting
..prop
```

```ting-output
(section .. prop)
```

```ting
.(string Name)
```

```ting-output
(section . (apply string Name))
```

```ting
(- 1)
```

```ting-output
error (1,2-1,3): '-' without whitespace before it is subtraction, which needs a left operand; write ' -x' for negation
```
