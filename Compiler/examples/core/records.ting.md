# Translation of products and records

A product is built by a constructor named by its positional field count and its field names. Field names are sorted, so the order in which fields are written does not matter. Equal products have equal fields.

```ting
(1, 2 @ Name = "Bob")
```

```ting-output
Value:
  value = (1, 2 @ Name = "Bob")
Axioms:
  x1 : any & x2 : any & x3 : any & y1 : any & y2 : any & y3 : any & ((x1, x2 @ Name = x3) = (y1, y2 @ Name = y3)) all (x1 = y1) & (x2 = y2) & (x3 = y3)
```

```ting
(@ b = 2, a = 1) = (@ a = 1, b = 2)
```

```ting-output
Value:
  value = true
Axioms:
  x1 : any & x2 : any & y1 : any & y2 : any & ((@ a = x1, b = x2) = (@ a = y1, b = y2)) all (x1 = y1) & (x2 = y2)
```

A single positional value is fused one level deep; `@@` keeps the positional section as one field.

```ting
((1, 2) @ Name = "Bob")
```

```ting-output
Value:
  value = (1, 2 @ Name = "Bob")
Axioms:
  x1 : any & x2 : any & x3 : any & y1 : any & y2 : any & y3 : any & ((x1, x2 @ Name = x3) = (y1, y2 @ Name = y3)) all (x1 = y1) & (x2 = y2) & (x3 = y3)
```

```ting
(1, 2 @@ Name = "Bob")
```

```ting-output
Value:
  value = ((1, 2) @ Name = "Bob")
Axioms:
  x1 : any & x2 : any & y1 : any & y2 : any & ((x1, x2) = (y1, y2)) all (x1 = y1) & (x2 = y2)
  x1 : any & x2 : any & y1 : any & y2 : any & ((x1 @ Name = x2) = (y1 @ Name = y2)) all (x1 = y1) & (x2 = y2)
```

Fusing a value whose structure is not known is not supported yet.

```ting
x = (1, 2) in (x @ Name = "Bob")
```

```ting-output
Value:
  x_1 : any & (x_1 = (1, 2)) & unsupported
Axioms:
  x1 : any & x2 : any & y1 : any & y2 : any & ((x1, x2) = (y1, y2)) all (x1 = y1) & (x2 = y2)
Errors:
  (1,15-1,33): not yet supported: fusion of a positional value whose structure is not known
```

Selecting a field of a value built by a known constructor yields the field directly. Otherwise the selection is a relation, defined by an axiom for every constructor with that field.

```ting
(@ Name = "Bob", Age = 42).Age
```

```ting-output
Value:
  value = 42
Axioms:
  x1 : any & x2 : any & y1 : any & y2 : any & ((@ Age = x1, Name = x2) = (@ Age = y1, Name = y2)) all (x1 = y1) & (x2 = y2)
```

```ting
p = (@ Name = "Bob", Age = 42) in p.Age
```

```ting-output
Value:
  p_1 : any & (p_1 = (@ Age = 42, Name = "Bob")) & .Age(p_1, value)
Axioms:
  x1 : any & x2 : any & y1 : any & y2 : any & ((@ Age = x1, Name = x2) = (@ Age = y1, Name = y2)) all (x1 = y1) & (x2 = y2)
  x1 : any & x2 : any & v : any all .Age((@ Age = x1, Name = x2), v) == (v = x1)
```

```ting
(0 -> 1).Result
```

```ting-output
Value:
  value = 1
Axioms:
  x1 : any & x2 : any & y1 : any & y2 : any & (x1 -> x2 = y1 -> y2) all (x1 = y1) & (x2 = y2)
```

`record` harvests the names its operand declares. Field expressions refer to other fields through `this`, the record being defined.

```ting
record(a = 1, b = this.a + 1)
```

```ting-output
Value:
  this_1 : any & a_2 : any & b_3 : any & (value = (@ a = a_2, b = b_3)) & (this_1 = (@ a = a_2, b = b_3)) & (a_2 = 1) & v2 : any & .a(this_1, v2) & (b_3 = v2 + 1)
Axioms:
  x1 : any & x2 : any & y1 : any & y2 : any & ((@ a = x1, b = x2) = (@ a = y1, b = y2)) all (x1 = y1) & (x2 = y2)
  x1 : any & x2 : any & v : any all .a((@ a = x1, b = x2), v) == (v = x1)
```

```ting
(record(a = 1, b = this.a + 1)).b
```

```ting-output
Value:
  v1 : any & (this_1 : any & a_2 : any & b_3 : any & (v1 = (@ a = a_2, b = b_3)) & (this_1 = (@ a = a_2, b = b_3)) & (a_2 = 1) & v3 : any & .a(this_1, v3) & (b_3 = v3 + 1)) & .b(v1, value)
Axioms:
  x1 : any & x2 : any & y1 : any & y2 : any & ((@ a = x1, b = x2) = (@ a = y1, b = y2)) all (x1 = y1) & (x2 = y2)
  x1 : any & x2 : any & v : any all .a((@ a = x1, b = x2), v) == (v = x1)
  x1 : any & x2 : any & v : any all .b((@ a = x1, b = x2), v) == (v = x2)
```

A single name declares a field and takes its value from the surrounding binding.

```ting
value = 1 in record(value)
```

```ting-output
Value:
  value_1 : any & (value_1 = 1) & (value = (@ value = value_1))
Axioms:
  x1 : any & y1 : any & ((@ value = x1) = (@ value = y1)) all x1 = y1
```

A field declaration that is not a proposition must be defined.

```ting
record(int a, a > 0)
```

```ting-output
Value:
  this_1 : any & a_2 : any & (value = (@ a = a_2)) & (this_1 = (@ a = a_2)) & a_2 : int & a_2 > 0
Axioms:
  x1 : any & y1 : any & ((@ a = x1) = (@ a = y1)) all x1 = y1
```

`records` is the set of the records its operand describes.

```ting
records(string Name, int Age)
```

```ting-output
Value:
  value = set1
Axioms:
  x1 : any & x2 : any & y1 : any & y2 : any & ((@ Age = x1, Name = x2) = (@ Age = y1, Name = y2)) all (x1 = y1) & (x2 = y2)
  v1 : any all v1 : set1 == (this_1 : any & Name_2 : any & Age_3 : any & (v1 = (@ Age = Age_3, Name = Name_2)) & (this_1 = (@ Age = Age_3, Name = Name_2)) & Name_2 : string & Age_3 : int)
  v4 : any & v5 : any all app(set1, v4, v5) == (v4 : set1 & (v5 = v4))
```

```ting
(@ Name = "Bob", Age = 42) : records(string Name, int Age)
```

```ting-output
Value:
  (@ Age = 42, Name = "Bob") : set1 & (value = true) | !((@ Age = 42, Name = "Bob") : set1) & (value = false)
Axioms:
  x1 : any & x2 : any & y1 : any & y2 : any & ((@ Age = x1, Name = x2) = (@ Age = y1, Name = y2)) all (x1 = y1) & (x2 = y2)
  v1 : any all v1 : set1 == (this_3 : any & Name_4 : any & Age_5 : any & (v1 = (@ Age = Age_5, Name = Name_4)) & (this_3 = (@ Age = Age_5, Name = Name_4)) & Name_4 : string & Age_5 : int)
  v4 : any & v5 : any all app(set1, v4, v5) == (v4 : set1 & (v5 = v4))
```

`tuple` binds `this` to the tuple; `tuples` is the set of the tuples its operand describes.

```ting
tuples(int a, int b \ this != (0, 0))
```

```ting-output
Value:
  value = set1
Axioms:
  x1 : any & x2 : any & y1 : any & y2 : any & ((x1, x2) = (y1, y2)) all (x1 = y1) & (x2 = y2)
  v1 : any all v1 : set1 == (this_1 : any & a_2 : any & b_3 : any & (this_1 = v1) & a_2 : int & b_3 : int & !(this_1 = (0, 0)) & (v1 = (a_2, b_3)))
  v4 : any & v5 : any all app(set1, v4, v5) == (v4 : set1 & (v5 = v4))
```

A lambda can select a field of its argument.

```ting
f = (p => p.Name) in f (@ Name = "Bob")
```

```ting-output
Value:
  f_1 : any & (f_1 = lambda1) & app(f_1, (@ Name = "Bob"), value)
Axioms:
  v1 : any & v2 : any all app(lambda1, v1, v2) == (p_2 : any & (v1 = p_2) & .Name(p_2, v2))
  x1 : any & y1 : any & ((@ Name = x1) = (@ Name = y1)) all x1 = y1
  x1 : any & v : any all .Name((@ Name = x1), v) == (v = x1)
```

Hosted properties and implicit lookup through `this` need semantic analysis.

```ting
int.Zero = 0 in int.Zero
```

```ting-output
Value:
  unsupported & .Zero(int, value)
Errors:
  (1,1-1,9): not yet supported: hosted property declaration
```
