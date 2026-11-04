---
title: Assign Variable Value
description: Give a RAPID variable a new value while the program runs (name := value;).
---

# ![](../../assets/icons/rc/assign-variable-value.png){ .rc-icon--title } Assign Variable Value

<span class="rc-badge">Advanced RAPID Features tab</span>
<span class="rc-badge rc-badge--fork">Fork addition</span>
<span class="rc-badge rc-badge--verified">RAPID checked on IRB 1200 · RobotWare 6.10</span>

**Assign Variable Value** writes a new value into a [[RAPID Variable]] at that point in the
program: count up, set a flag, store a position. It is an instruction, so its
**Assignment** output goes into *Actions*: of the [[RAPID Generator]], a [[For Loop]], an
[[IF Statement]], a [[Routine]], ….

[[io:Assign Variable Value]]

*Value* takes anything RAPID can evaluate: a number (`0`), text in quotes (`"done"`),
`TRUE`/`FALSE`, another variable, or an expression typed into a [[gh:Panel]]
(`count + 1`, `i * 100`). Right-click → array assignment mode writes one element of an
array variable.

## In RAPID

```
count := count + 1;
```

`:=` means *"becomes"*. The right side is calculated first, then stored, so
`count := count + 1` adds one. (`=` alone is a comparison, see [[Comparison Expression]].)
ABB's reference: [RAPID kernel][abb-kernel] (assignment statement).

In the [row tutorial](../b8-tutorials/row-loop.md) two assignments are used: one inside the
loop (`count := count + 1;`) and one inside the IF (`count := 0;`).

## Try it

- [ ] Make the counter count in steps of 10.
- [ ] Assigning to a `CONST` is not allowed. Change the tutorial's counter Keyword to `CONST` and check what the controller says. *(You need a controller or RobotStudio for this one.)*

??? success "Solution to 1"
    *Value* = `count + 10`.

## Gotchas

!!! warning "Wire *Assignment* into Actions"
    The first output (*Variable*) is only a pass-through of the variable, for chaining. The
    instruction itself is *Assignment*.

## See also

[[RAPID Variable]] · [[Comparison Expression]] · [[For Loop]]
