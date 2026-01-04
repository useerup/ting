So I am doing a logic programming language. I am still struggling with writing the compiler, but for what it is worth, here is what I have planned.

In my language, the types sets. To do what you are contemplating with your `struct Field`, I'd do this:


    IsField = (set, add, mul, neg, inv, zero, one) -> && >>>
        set : Sets
        add : set^2=>set 
        neg : set^2=>set 
        mul : set^2=>set
        inv : (set??!=zero)=>set
        zero : set
        one : set

        (set x, set y, set z) all & >>>
            add(x,add(y,z)) = add(add(x,y),z) 
            add(x,add(y,z)) = add(add(x,y),z) 
            add(x,y) = add(y,x)
            add(x,zero) = x
            add(x,neg x) = zero
            mul(x,mul(y,z)) = mul(mul(x,y),z)
            mul(x,y) = mul(y,x)
            mul(x,one) = x
            x != 0 then mul(x,inv x) = one
            mul(x,add(y,z)) = add(mul(x,y),mul(x,z))


Here `IsField` is a function which accepts a number of parameters and returns a bool value indicating whether the tuple of arguments constitute a `field`. When this `IsField` function is used in an *assertive* manner like

    IsField( float, float.Add, float.Multiply, float x->1f/x, float.Reciprocal, 0f, 1f )

where the goal of the compiler is to make this expression `true`, then the compiler infers that it has to make the invocation of `IsField` true. It can do that by substituting (rewriting) the body of the function with the actual parameters. This wil yield an first-order logic expression roughly like

