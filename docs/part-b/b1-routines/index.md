# B1 Routines

Your own PROC, FUNC and TRAP routines: write a sequence once, call it with arguments, return values, load modules.

Pages marked *reference tables only* already show the component's description and its inputs and outputs; the tutorial text follows.

| Components | Status |
|---|---|
| [[Load Module]] | ✅ written |
| [[Return]] | ✅ written |
| [[Routine Argument]] | ✅ written |
| [[Routine Call]] | ✅ written |
| [[Predefined Scopes]] · [[Routine]] · [[Routine Types]] | ✅ written |

## Chapter exercises

★ basic · ★★ combine · ★★★ open

1. ★ In the [[Routine]] example, rename the routine from `visit` to `touch`. Which lines change?

2. ★★ Turn the approach height of `visit` into a second argument and call it with 100 and 50.

??? success "Solutions"
    1. The routine header `PROC touch(num y)` and the two calls `touch -150;` / `touch 150;`. Change the name in the one [[gh:Panel]] that feeds both the Routine and the [[Routine Call]].
    2. Click **+** on the [[Routine]] and add a [[Routine Argument]] `num h`; use its *Variable* for *Z* of the upper [[Offset Target]]. Click **+** on the [[Routine Call]] for *Argument Value 2*. The routine becomes `PROC visit(num y, num h)` and the calls `visit -150, 100;` etc.
