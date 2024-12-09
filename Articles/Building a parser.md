# Building a parser

Let's build a parser for the Ting language in Ting.

Some languages use an exception mechanism to signal that a function or method has been called with invalid parameters. Java even has an entire exception hierarchy - checked exceptions - for dealing with those exceptions that represent some limitation of the program logic. Other languages use an error mechanism where the *return value* of a function/method may signal success or failure.

In the Ting language *definedness* is an important concept for functions. We push logical limitations of arguments (value combinations that are invalid in the current situation) to the function signature. This is somewhat akin  to what Java does with checked exceptions, but in Ting the "limitation signature" is not a fault mechannism, rather it is a limitation of the function domain.

Consider a function that calculates the mean value of a set of integers:

    Mean = { {int} m -> m.Sum/m.Count } // will not compile

If we were to invoke `Mean` with an empty set, `m.Count` would be zero and we would divide a number by zero. If the compiler cannot guarantee that this never happens, it will complain that a situation exists where `/` is not defined for all cases.

    Mean = { {int} m \\ !m.IsEmpty -> m.Sum/m.Count }

When we are parsing text, we use this principle: A parser function is only defined for the sources that match the pattern being parsed. A very simple parser function simply parses a single character:

Example:

    { Sources(_,,rest) -> rest }

However, that is not a very useful parser. We usually have an idea *which* character(s) we expect. That is the essense of parsing.

So we introduce a `Char` *rule* which - given a character - returns a parser for that character:

    Char = { { Sources(c,,rest) -> rest }  <-  c }


The following are some *parser combinators*:

    Optional = Rules rule \ {
        Rule h  <-  (h,,)           // value is a list with one element
        Not (rule _)  <-  (,,)      // value is an empty list
    }

    OneOrMore = Rules rule \ {
        prod h >> self t  <-  (h,,t)
        prod h >> Not (prod _) <-  (h,,)
    }

    ZeroOrMore = Productions prod \ {
        prod h >> self t  <-  (h,,t)    
        Not (prod _)  <-  (,,)          
    }


We can write a parser which will match a *word*, defined as a sequence which begins with a character from the `WordBeginCharacters` set and is followed by any number of characters from the `WordFollowCharacters` set:

    // A parser for a word token
    Word = { Char (WordBeginCharacter head) >> ZeroOrMore WordFollowCharacter tail  <-  (head,,tail) }



Likewise

    Digits = {'0','1','2','3','4','5','6','7','8','9'}

    Char (Digits s)






## A small Ting primer 


 `{` and `}` constructs a set from a list of set elements. If an element can be evaluated to more than one value, the set will contain *all* of those possible values. We can construct a set both through a list as well as through intentional expressions:

    { 0,1,2,3,4,5,6,7,8,9 }         // numbers from 0 through 9
    { int x ? > 0}                  // all positive integers
    { 0,1, int x ?>=10 ?<=20  }     // 0,1 and all the numbers from 10 through 20



`<-` maps the right operand to the left operand. If the `<-` expression is declarative, then the left operand is declarative while the right operand is referential.


* *Declarative* means that the expression declares (and binds) identifiers
* *Referential* means that identifiers occurring in the expression *references* the identifiers.
* When a *function invocation* is in declarative context, like `Sources (c,,rest)` above, the function part `Sources` is referential and the argument `(c,,rest)` is declarative.
* `Sources` is an alias for `Strings`, i.e.the set of all potential source texts is the set of all possible strings
* `\` is the "infix lambda" - a shorthand for writing functions. The left operand of `\` is declarative and is the argument of the function and the right operand is the function "result"
* A set is also the identity function for itself. The

