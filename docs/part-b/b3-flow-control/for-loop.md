---
title: For Loop
description: Repeat a set of actions a fixed number of times with a counter (FOR … ENDFOR).
---

# ![](../../assets/icons/rc/for-loop.png){ .rc-icon--title } For Loop

<span class="rc-badge">Advanced RAPID Features tab</span>
<span class="rc-badge rc-badge--fork">Fork addition</span>
<span class="rc-badge rc-badge--verified">RAPID checked on IRB 1200 · RobotWare 6.10</span>

**For Loop** repeats its *Actions* once for every whole number from *From* to *To*. The
counter (by default `i`) holds the current number and can be used inside the loop, for
example in an [[Offset Target]] (`i * 100`) to move a little further each time.

[[io:For Loop]]

*Counter* is optional. Leave it empty and the loop counts with `i`. To use another
name, wire a [[RAPID Variable]]'s *Variable* output (type [[abb:num]], Level `Routine`). The
*Counter* output passes the counter on for components **after** the loop (not inside it; see Gotchas).

## In RAPID

```
FOR i FROM 0 TO 3 DO
    MoveL Offs(base, 0, i * 100, 0), v200, fine, tool0\WObj:=wobj0;
    count := count + 1;
ENDFOR
```

The body runs for `i` = 0, 1, 2, 3, so four times. RAPID's `FOR` also has an optional
`STEP`, which this component does not set. Use a multiplier inside the body
(`i * 50`) or a [[While Loop]] for other step sizes. ABB's reference:
[RAPID Instructions, Functions and Data types][abb-ifd] ([[abb:FOR]]).

Full example: [row tutorial](../b8-tutorials/row-loop.md).

## Try it

- [ ] Count backwards: visit the row from the far end to the start. What do *From* and *To* become, and does it work? *(Hint: without `STEP -1`, a RAPID FOR from 3 to 0 counts down by itself.)*

??? success "Solution"
    *From* `3`, *To* `0`. RAPID's FOR counts down automatically when *From* > *To*, so the
    row is visited in reverse. (Checked on the Virtual Controller: `FOR i FROM 3 TO 0`
    runs four times and ends with `i` = 0.)

## Gotchas

!!! warning "Nested loops: give each one its own counter"
    Two For Loops without a *Counter*, one inside the other, both count with `i`
    (`FOR i … FOR i …`). It loads, but the inner `i` hides the outer one. Give the outer loop a
    [[RAPID Variable]] counter (e.g. `j`).

!!! warning "The *Counter* output can't be used inside the same loop"
    The loop's actions are an *input* of the loop, so wiring its *Counter* output into them makes
    the loop depend on itself (*"Recursive data stream found"*). To use the counter inside the
    body, create it as a [[RAPID Variable]] and wire that into both the *Counter* input and the
    body, or type its name (`i`) into a [[gh:Panel]] as in the
    [row tutorial](../b8-tutorials/row-loop.md). See [looping over an array](../b8-tutorials/array-offsets.md).

## See also

[[While Loop]] · [[IF Statement]] · [[Offset Target]] · [[RAPID Variable]]
