# B1 Routines

Your own PROC, FUNC and TRAP routines: write a sequence once, call it with arguments, return values, load modules.

| Components | What for |
|---|---|
| [[Predefined Scopes]] · [[Routine]] · [[Routine Types]] | Write your own RAPID procedure (PROC), function (FUNC) or interrupt routine (TRAP) and call it from main. |
| [[Routine Argument]] | Define a parameter of your own routine, and use it inside the routine body. |
| [[Routine Call]] | Call a routine (PROC or FUNC) by name, with argument values. |
| [[Return]] | End a routine, and hand a value back from a function (RETURN). |
| [[Load Module]] | Load and unload extra RAPID modules while the program runs, synchronously or in the background. |

## Chapter exercises

★ basic · ★★ combine · ★★★ open

1. ★ In the [[Routine]] example, rename the routine from `visit` to `touch`. Which lines change?

2. ★★ Turn the approach height of `visit` into a second argument and call it with 100 and 50.

??? success "Solutions"
    1. The routine header `PROC touch(num y)` and the two calls `touch -150;` / `touch 150;`. Change the *Name* of the [[Routine]] **and** the *Routine Name* of the [[Routine Call]]: if they differ, the module doesn't load (the call refers to a routine that doesn't exist).
    2. Click **+** on the [[Routine]] and add a [[Routine Argument]] `num h`; use its *Variable* for *Z* of the upper [[Offset Target]]. Click **+** on the [[Routine Call]] for *Argument Value 2*. The routine becomes `PROC visit(num y, num h)` and the calls `visit -150, 100;` etc.
