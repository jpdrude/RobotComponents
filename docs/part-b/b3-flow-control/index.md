# B3 Flow control

Decisions and repetition on the controller: IF, WHILE and FOR.

Pages marked *reference tables only* already show the component's description and its inputs and outputs; the tutorial text follows.

| Components | Status |
|---|---|
| [[For Loop]] | ✅ written |
| [[IF Statement]] | ✅ written |
| [[While Loop]] | ✅ written |

Tutorial: [a row of points with a loop](../b8-tutorials/row-loop.md).

## Chapter exercises

★ basic · ★★ combine · ★★★ open

1. ★ Make the [row tutorial](../b8-tutorials/row-loop.md) visit its points in reverse order.

2. ★★ Repeat the row until the operator says stop, instead of a fixed number of times.

??? success "Solutions"
    1. *From* `3`, *To* `0`. RAPID's FOR counts down by itself when *From* > *To* (checked on the Virtual Controller).
    2. Wrap the row in a [[While Loop]] with a [[abb:bool]] condition and end it from a [[Message Box]], as in the [operator loop](../b5-operator-interaction/message-box.md). Remember to wire something into **every** button.
