---
title: For Loop
description: Repeat a set of actions a fixed number of times with a counter (FOR … ENDFOR).
---

# ![](../../assets/icons/rc/for-loop.png){ .rc-icon--title } For Loop

<span class="rc-badge">Advanced RAPID Features tab</span>
<span class="rc-badge rc-badge--fork">Fork addition</span>
<span class="rc-badge rc-badge--verified">RAPID checked on IRB 1200 · RobotWare 6.10</span>

**For Loop** repeats its *Actions* once for every whole number from *From* to *To*. The
counter (by default `i`) holds the current number and can be used inside the loop by its name,
for example in an [[Offset Target]] (`i * 100`) to move a little further each time.

[[io:For Loop]]

## When to use it

The For Loop is made for a **sequence with a known number of repetitions**: visit 4 points of a
row, stack 10 parts, draw 3 passes. Leave *Counter* empty: the loop counts with `i` (or `j`, `k`, …
if a loop inside it already uses `i`), and the body refers to it by that name, typed into a
[[gh:Panel]] or an expression.

For **everything else**, create your own variables with [[RAPID Variable]] and set them with
[[Assign Variable Value]]:

- a counter you need in several places, or under a name of your own: a [[RAPID Variable]]
  (`num`, Level `Routine`) wired into *Counter* **and** into the body components (as in
  [looping over an array](../b8-tutorials/array-offsets.md));
- a count that has to survive the loop or the program run (parts made, cycles): a `PERS`
  variable you increase yourself (as in the [row tutorial](../b8-tutorials/row-loop.md));
- repeating until something happens (a sensor, an operator's answer) rather than a fixed number of
  times: a [[While Loop]] with a [[abb:bool]] variable.

The *Counter* output hands the counter on to components **after** the loop.

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

!!! info "Nested loops: the innermost loop counts with `i`"
    Without a *Counter*, each loop takes the first of `i`, `j`, `k`, … that no loop inside it
    uses. Grasshopper builds nested loops from the inside out, so the **innermost** loop gets
    `i` and the ones around it `j`, `k`, …: `FOR j … DO FOR i … DO`. That's the reverse of the
    usual convention (outer `i`, inner `j`). Each loop's *Counter* output tells you its name. If you
    prefer the usual order, give the loops their own counters with [[RAPID Variable]].

!!! warning "The *Counter* output is for after the loop"
    The body is an *input* of the loop, so wiring the loop's own *Counter* output into it makes the
    loop depend on itself (*"Recursive data stream found"*). That's how Grasshopper works, not a
    bug. Inside the body, use the counter's name, or your own [[RAPID Variable]] (see
    [When to use it](#when-to-use-it)).

## See also

[[While Loop]] · [[IF Statement]] · [[Offset Target]] · [[RAPID Variable]]
