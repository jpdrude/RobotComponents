---
title: RAPID Variable
description: Declare your own RAPID data (VAR, PERS, CONST) and reference it from other components.
---

# ![](../../assets/icons/rc/rapid-variable.png){ .rc-icon--title } RAPID Variable

<span class="rc-badge">Advanced RAPID Features tab</span>
<span class="rc-badge rc-badge--fork">Fork addition</span>
<span class="rc-badge rc-badge--verified">RAPID checked on IRB 1200 · RobotWare 6.10</span>

**RAPID Variable** declares a piece of data on the controller: a counter, a flag, a
position, a list of positions. Other components then **use** it by name: in
[[Assign Variable Value]], [[Comparison Expression]], [[For Loop]], [[Offset Target]] and
anywhere a [[type:RAPID Expression]] is accepted.

It has two outputs, and they do different jobs:

- **Variable** is the *reference*. Wire it into components that read or write the data.
- **Variable Declaration** is the *declaration line*. Wire it into the [[RAPID Generator]]'s
  *Actions* (or a [[Routine]]'s), or the controller won't know the name.

[[io:RAPID Variable]]

When placed, the component creates drop-down lists for *Level*, *Scope* and *Keyword*.
Right-click → **Set Array Size** turns it into an array (see [[Get Array At Index]]).

## In RAPID

| Setting | Writes | Meaning |
|---|---|---|
| Keyword `VAR` | `VAR num count := 0;` | variable, reset on *PP to Main* |
| Keyword `PERS` | `PERS num count := 0;` | persistent: keeps its value between runs |
| Keyword `CONST` | `CONST num count := 0;` | fixed value; needs *Value* |
| Keyword `INOUT` | — | parameter mode for routine arguments, see [[Routine Argument]] |
| Level `Module` | at the top of the module | visible to all routines (by *Scope*) |
| Level `Routine` | inside the routine, before its instructions | exists only while the routine runs |
| Scope `GLOBAL` / `LOCAL` / `TASK` | `LOCAL PERS …` etc. | see [Scope](../b0-rapid-primer.md#scope-who-can-see-it) |

The RAPID types (`num`, `bool`, `string`, `robtarget`, …) are explained in the
[B0 RAPID primer](../b0-rapid-primer.md#the-data-types-youll-meet). ABB's reference:
[RAPID kernel][abb-kernel] (data declarations).

From the [row tutorial](../b8-tutorials/row-loop.md), Keyword `PERS`, Type `num`, Name `count`, Value `0`:

```
PERS num count := 0;
```

## Try it

- [ ] Change the counter to `VAR`. Run the tutorial program twice (with *PP to Main* in between). What is `count` after the second run?
- [ ] Declare a `bool` named `gripperClosed` with initial value `FALSE`. What does the *Variable Declaration* output show?

??? success "Solutions"
    1. `4`: as `VAR`, *PP to Main* resets it to `0` before each run. As `PERS` it would be `8`.
    2. `VAR bool gripperClosed := FALSE;` (with the default Level `Module`, Scope `GLOBAL`, Keyword `VAR`).

## Gotchas

!!! warning "Wire *Variable Declaration* into Actions, not *Variable*"
    *Variable* is the first output and the one you need everywhere else, but in the
    [[RAPID Generator]]'s *Actions* it fails: *"Data conversion failed from RAPID Variable to
    Action"* plus a solution exception, and no module at all.

!!! warning "Routine-level data can't be global"
    Level `Routine` with Scope `GLOBAL` gives *"Routine variables cannot be global. Scope set
    to LOCAL."*. Data inside a routine is always local to it.

!!! warning "Initial values must be constants"
    *Value* may be a number, text, `TRUE`/`FALSE` or a **constant**, not another `VAR`. See
    the [primer](../b0-rapid-primer.md#data-three-kinds-of-storage).

## See also

[[Assign Variable Value]] · [[Get Array At Index]] · [[Routine Argument]] · [[type:RAPID Variable]]
