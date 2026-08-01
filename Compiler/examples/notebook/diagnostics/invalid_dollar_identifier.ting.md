```ting
invalid$char + 5
```
```ting-output
Compilation failed.
(1,8) Error in Ln: 1 Col: 8
invalid$char + 5
       ^
Expecting: end of input or infix operator

The parser backtracked after:
  Error in Ln: 1 Col: 9
  invalid$char + 5
          ^
  Unknown Error(s)

```
