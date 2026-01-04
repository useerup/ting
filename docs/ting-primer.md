# Ting language primer


## Sets

*Sets* are central to Ting. Ting sets are not data structures, rather they are the *types* of Ting.


### Set definitions

A set is defined through a *set definition*. Like in math, a set is defined using curly braces `{` and `}`. The following are examples of *set definitions*:

    { 1, 2, 3 }             // Simple list-form set definition

    { float x \ x >= 0 }    // Set Non-negative floating point numbers

    { '0'...'9' }           // Set of digits

    { let x:int in x^2 }    // Set of quadratic numbers


### Built-in sets



### Composing new sets

Refinement types


## Functions


### Sets as functions

You may have notices 



## Lists


## Classes

    MyFloats = class float

    MyFloats 3.14



## Identifiers

This is a string: `"abc def 123"`

This is an interpolated string: `$"Name: {person.Name}"`

This is a quoted identifier: `` `QuotedIdentifier` ``

This is an interpolated identifier: `` $`NameOf{person.Name}` ``