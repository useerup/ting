# Switch and pattern matching

This is a function:

    { int n \\ n%3=0 -> "Fizz" }

It is defined *only* for integers that are divisible by 3. It is an error to call it with a number
that is not divisible by 3. Applied to any legal argument, it will return "Fizz"

    { int n \\ n%5=0 -> "Buzz" }

That is a similar function, but it is defined only for integers divisible by 5 and returns "Buzz" instead.

    { int n \\ n%3=0 & n%5=0 -> "FizzBuzz" }

The above function is only defined for integers that are divisible by 3 *and* 5. 

    { int n -> n.ToString }

The above function simply accepts any integer an returns the string representation for that integer. It is defined for *all* integers. 

To create a function for the game of FizzBuzz, we can combine these.

    FizzBuzz = 
        { int n \\ n%3=0 & n%5=0 -> "FizzBuzz" }
        || { int n \\ n%3=0 -> "Fizz" }
        || { int n \\ n%5=0 -> "Buzz" }
        || { int n -> n.ToString }

By combining the functions using `||` we are *excluding* function points from the following functions, if they are already supplied by the former functions. 

But this is actually what a set constructor already does. So we could write the function like this instead:

    FizzBuzz = int+string && {
        n \\ n%3=0 & n%5=0 -> "FizzBuzz"
        n \\ n%3=0 -> "Fizz"
        n \\ n%5=0 -> "Buzz"
        n -> n.ToString
    }