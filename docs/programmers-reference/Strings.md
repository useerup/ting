## Strings

Strings in Ting are lists of Unicode characters. 

String literals are enclosed in double quotes (`"`).

```ting
let message = "Hello, World!";
```

Escape sequences can be used to represent special characters, such as `\n` for a new line, `\t` for a tab, and `\"` for a double quote.


### Verbatim string literals

Verbatim string literals start with the `@` symbol and are also enclosed in double quotes. In a verbatim string, escape sequences are not processed, which makes them useful for file paths or regular expressions. The only exception is the double quote, which must be escaped by doubling it (`""`). Verbatim strings can also span multiple lines.

```ting
let path = @"C:\Users\Public\Documents";
let multiline = @"This is a
multiline string.";
let quote = @"He said, ""Hello!""";
```

### Raw string literals

Raw string literals are enclosed in a minimum of three double quotes (`"""`). They allow you to include arbitrary text, including whitespace, newlines, and other quote characters, without needing escape sequences.

The closing quotes must be on their own line, and the indentation of the closing quotes determines the base indentation for the string content.

```ting
let xml = """
    <element attr="content">
        <body>
            This is some text.
        </body>
    </element>
    """;
```

If the string content contains a sequence of three or more double quotes, you can start and end the raw string literal with more quotes (e.g., four quotes) to avoid ambiguity.


### Interpolated strings

Interpolated strings allow you to embed expressions directly within a string literal. They are prefixed with the `$` symbol. Expressions are enclosed in curly braces `{}`.

```ting
let name = "Alice";
let age = 30;
let message = $"Name: {name}, Age: {age}";
```

You can combine interpolation with verbatim or raw strings.

```ting
let path = $@"C:\Users\{userName}\Documents";
```

