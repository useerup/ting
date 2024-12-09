# Reflections of Checkec exceptions

There has been much debate about *checked exceptions* in Java


What a checked exception really is saying is: "You are using this method in the wrong way". If you somehow index an array out of bounds, dereference a null, causes an integer calculation overflow or try to convert the string "Foo" into a number, you are asking a function/method to do something it was not designed to do. That, or the programmer made an implementation mistake. Both cases should be preventable.

Java's checked exceptions is a valiant attempt at solving this. Unfortunately, it has been shunned by a large number of Java programmers, and few other languages have adopted them.



How do you handle it when one of thefunctions' dependencies declare checked exceptions? Either you


By constraining the argument we circumvent the need to declare each exception. Because we are upfront about the limitations, the exception messages are not needed to explain what happened after.


Java also forces you to deal with the contingency right away. The ordinal result and the contingency are returned in two different ways, through two different return mechanisms. Actually you could argue that there is the ordinal result and then one extra return mechanism for each checked exception. Java *forces* you to deal with the contingency right away.

When you aggregate functionality, that may not be what you want to do.

    Account $ \.ProcessCheck check
        catch StopPayment _ \ ...
            catch InsufficientFunds _ \ ...

