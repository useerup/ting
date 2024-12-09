>which also means that function declarations will just be assignments to variables, which also also means that functions will not be overloadable.

I have considered this. What I ended up with was this:

1. Modules are types, albeit special *module types*. But still types and thus may be subtyped.

2. `Import` is subtyping. The importing module is a subtype of all the imported *module types*. If a module *M* imports modules *A*, *B* and *C*, then *M* is a subtype of *A*, *B* and *C*.

4. Execution of a program executes an *instance* of the program module.

5. Functions are values. Like the value `42` they are just values and do not have any inherent name. All functions are lambdas, if you will.

**A module may specify a functon like**

    protected Add = Any**Any virtual (int a,int b) => System.Int.Add(a,b)

Here the right hand of the `=` is the function which is assigned to the module member `Add`. But it may also