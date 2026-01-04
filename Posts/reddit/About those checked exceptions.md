I used to hate checked exceptions. 

I believe it was because checked exceptions, when they arrived as a mandatory feature (in C++ they were optional), seemed to hold such a great promise. However, trying to program with them soon revealed their - IMHO - less than ergonomic characteristics. Being forced to use something that constantly gets in the way for seemingly little gain makes you wary. And then when all kinds of issues creep up that are attributable to checked exceptions, such as implementation details creeping into contracts (*interfaces*), I grew to dislike them. Even hate them.

These days I still hate them, but perhaps a little less so. Maybe I dislike them.

I used to wonder what was it that was so bad about checked exceptions, when - in theory - they should be able to alleviate an entire class of bugs. My conclusion at the time - born from experience using them - was that it was a mistake to demand that *every function on the call stack* deal with exceptions arising from the lower levels. After all, the initial allure of exceptions (in general) was that you only needed to be concerned about a specific error condition in *two* places: 1) where the error condition occured and 2) where you handle the error. Checked exceptions - as they were implemented in Java - broke that promise.

Many later languages have shunned checked exceptions. Some languages have shunned exceptions altogether, others - including innovations on the JVM platform - kept exceptions but did away with the "checked" regime. 

I was in that camp. In my defense I always felt that - maybe - it was just that some of the choices of Java were too draconian. What if they could be tweaked to only require checked exceptions to be declared on functions exported from a module? Inside a module maybe statically analysis could do away with the requirement that you label every function on the call stack with a `throws` clause. But basically I dreaded checked exceptions.

Today I have come to realize that my checked exceptions may have - sorta - crept into my own language through the back door. 😱

I work with the concept of "definedness". In my language you have to model the types of arguments to a function so tight that the each function ideally becomes *total functions* in the mathematical sense. As an example, the division operator is only defined for non-zero divisors. It is a type error to invoke a division with a divisor which may be zero. So rather than catching a checked exception, the programmer must prove that the divisor cannot be zero, for instance through a constraint. While it is not *checked exceptions* per se, I believe you can imagine how this requirement can spread up the call stack in much the same way as checked exceptions.

Obviously, functions exists that may not be defined for all values of its domain. Consider a function which accepts a file path and returns the content of a that file. The domain (the type of the argument) of such a function is perhaps `string`. It may even be something even tighter such as `FilePath`, constraining how the string is composed. However, even with maximal constraints on the shape of such a string, the actual file may not exist at runtime.

Such functions are *partial* in my language, borrowing from the mathematical concept. The function to read the content of a file is only defined for file paths that point to a readable file. It is undefined for all other arguments. But we dont know at compile time. It *may* be undefined for any value in its domain.

What should such a function do when invoked with a file path to a file that does not exist or is not readable? In my language, such a function throws an exception. What should I call that exception? I think - hmmm - `UndefinedException`, because - despite the declared domain of the function - it was not really defined at that point/for that value?

So, a *partial function* in my language is a function which may throw an `UndefinedException`. I think I may have to mark those functions explicitly with a `partial` or `throws` keyword. However, without a feature to *handle* exceptions, an exception is just a panic. So I will have to be able to catch exceptions. But then I may want to handle the different reasons for a function to be undefined differently. Did the file not exist, is it locked for reading by somebody else, or is it a permissions issue?

Ah - so I need to be able to distinguish different reasons for `UndefinedException`. Perhaps `UndefinedException` is a class, and specific subclasses can spell out the reason for the function to be undefined?

Oh the horror! That looks suspiciously like checked exceptions by another name!

Maybe I was wrong about them?