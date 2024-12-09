(I am not sure that I understand you correct, so this answer may be way off)

But confirming that h(x)={f(x)->g(x)} is actually a function is hard.
I gather that you mean to define h to be a function that maps values f(x) to values g(x) for any given x?
In my langage this would be:
h = { f x -> g x }
(The syntax is not important, but I need to clarify it this way - mostly for my own understanding.)
The mere fact that this is a set of ordered pairs (constructed by ->) makes h a function.
Of course, there are several classifications of functions. A function may be partial/total and/or deterministic/nondeterministic.
If f x has the same value for different values of x above, then the function h becomes nondeterministic. The language should be able to handle such nondeterminism the same way Prolog does - by allowing for backtracking. Nondeterminism is generally undesired and should be contained. I plan to make the compiler warn about nondeterminism gone wild the same way that many Prolog compilers do. 
If the compiler cannot determine that a value is deterministic, then it must assume that it is nondeterministic.
