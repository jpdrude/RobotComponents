# B2 Data and expressions

Declare your own RAPID data, change it while the program runs, index arrays and build conditions.

Pages marked *reference tables only* already show the component's description and its inputs and outputs; the tutorial text follows.

| Components | Status |
|---|---|
| [[Assign Variable Value]] | ✅ written |
| [[Comparison Expression]] · [[Comparison Operators]] · [[Inequality Symbols]] | ✅ written |
| [[RAPID Variable]] | ✅ written |

Tutorial: [a row of points with a loop](../b8-tutorials/row-loop.md).

## Chapter exercises

★ basic · ★★ combine · ★★★ open

1. ★ Declare a `PERS num cycles := 0;` and count program runs with it.

2. ★★ Reset the counter from the [row tutorial](../b8-tutorials/row-loop.md) at 50 instead of 100, using `=` instead of `>=`. Why is `>=` the safer choice?

??? success "Solutions"
    1. [[RAPID Variable]] with Keyword `PERS`, Type [[abb:num]], Name `cycles`, Value `0`; its *Variable Declaration* into *Actions*. An [[Assign Variable Value]] `cycles + 1` once in `main` (not inside a loop).
    2. Operator `=`, B `50`. `>=` also resets if the counter ever jumps past the limit (e.g. after the limit was changed), while `=` would then never reset.
