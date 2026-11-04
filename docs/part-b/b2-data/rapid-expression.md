---
title: RAPID Expression
description: The data type behind Part B, a piece of RAPID that the controller evaluates while the program runs.
---

# ![](../../assets/icons/rc/param-rapid-expression.png){ .rc-icon--title } RAPID Expression

<span class="rc-badge">Parameters tab</span>
<span class="rc-badge rc-badge--fork">Fork addition</span>

A **RAPID Expression** is a piece of RAPID text that stands for a value: `count + 1`,
`i * 100`, `Offs(base, 0, 0, 50)`, `ys{k}`, `now.trans.z`, `currentZ()`. Grasshopper doesn't
compute it; it's written into the program **as is**, and the **controller** evaluates it while
the program runs. That's what lets Part B programs react to counters, sensors and operator
input.

## Where expressions come from

| Source | Gives |
|---|---|
| a [[gh:Panel]] with text, or a number | literally that: `i * 100`, `TRUE`, `"done"`, `42` |
| [[RAPID Variable]] (*Variable* output) | the variable's name: `count` |
| [[Comparison Expression]] | a condition: `count >= 100` |
| [[Offset Target]] | `Offs(base, 0, y, 100)` |
| [[Current Robot Target]] | `CRobT()` |
| [[Get Array At Index]] | `ys{k}` |
| [[Get Robtarget Members]] | `now.trans.z` |
| [[Routine Call]] / [[Routine]] (FUNC) | a function call: `currentZ()` |

## Where expressions are accepted

Inputs of type RAPID Expression take any of the above: the conditions of [[IF Statement]]
and [[While Loop]], the bounds of [[For Loop]], *Value* of [[Assign Variable Value]] and
[[RAPID Variable]], [[Return]], the offsets of [[Offset Target]], the *State*/*Duration* inputs of
the I/O and wait components, and the values of [[Acceleration Set]] and [[Velocity Set]]. A
[[Move]] accepts an [[Offset Target]] or a robtarget variable as its target.

!!! warning "Grasshopper can't check or preview expressions"
    A typo in `cuont + 1` is only found when the controller loads the module. Movements to
    targets built from expressions can't be shown by the [[Path Generator]]. See
    [B9 Pitfalls](../b9-pitfalls.md#what-the-simulation-cant-know).

## See also

[B0 RAPID primer](../b0-rapid-primer.md#expressions-assignments-and-functions) · [[RAPID Variable]] · [[Comparison Expression]]
