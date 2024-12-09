Functions can be marked *partial* 

A partial function is one which may not be defined for all possible arguments.

A function which does not restrict the domain of its argument is assumed to accept all arguments. 

    a => a + 1          // implicitly partial because a + 1 is not defined for all possible values of a
    int a => a + 1      // total

Implicitly partial functions can not be exported from a module. 

