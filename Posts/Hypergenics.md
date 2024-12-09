I have considered something similar for my language. 

My (planned) language will be well suited for writing parsers and compilers. Basically a parser and semantics can be written using function composition and a few basic parser combinators, while at the same time controlling the AST. 
My language allows grammar and semantics to be written in a really concise format which is close a *specification*. 

(In case you wonder: Yes, I am obviously planning on *dogfooding* my compiler. ;) )

So far so good. It is really cool to be able to write a compiler that looks like a specification, but that compiler will not be very resilient against errors.

Basically it will fail for any non-wellformed program. Even a small syntactic error will cause such a compiler to fail.

Modern compilers need to make an effort to pull itself together after an error, so that it can report on *other* errors. Yes, the program may be invalid, but if only the first error is reported on every compiler invocation, a programmer will soon lose their patience.

Furthermore, modern compilers should also be able to *partially* compile only changes, so that it can support tooling through e.g. Language Server Protocol (LSP).

This made me wonder:

What if I can write a compiler as *specification* with very strict grammar and semantics, and later use the homoiconic features of the language to *reinterpret* the original compiler and use rules to extend it so that it may serve the additional purposes.

