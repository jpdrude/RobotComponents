---
title: IF Statement
description: Run actions only when a condition is true, with optional ELSEIF and ELSE branches.
---

# ![](../../assets/icons/rc/if-statement.png){ .rc-icon--title } IF Statement

<span class="rc-badge">Advanced RAPID Features tab</span>
<span class="rc-badge rc-badge--fork">Fork addition</span>
<span class="rc-badge rc-badge--verified">RAPID checked on IRB 1200 · RobotWare 6.10</span>

**IF Statement** lets the controller decide while the program runs: the *IF Actions* run
only when the condition is true; otherwise the *ELSE Actions* run (if connected). Click the
**+** above *ELSE Actions* to add `ELSEIF` branches, each with its own condition and actions.

[[io:IF Statement]]

The condition is a [[type:RAPID Expression]]: usually from a [[Comparison Expression]], or
typed into a [[gh:Panel]] (`count >= 100 AND NOT finished`).

## In RAPID

```
IF count >= 100 THEN
    count := 0;
ENDIF
```

With ELSE and ELSEIF branches:

```
IF answer = 1 THEN
    ...
ELSEIF answer = 2 THEN
    ...
ELSE
    ...
ENDIF
```

Only the first branch whose condition is true runs. ABB's reference:
[RAPID Instructions, Functions and Data types][abb-ifd] ([[abb:IF]]).

Full example: [row tutorial](../b8-tutorials/row-loop.md).

## Try it

- [ ] Add an ELSE branch that sets a `bool` variable `full` to `FALSE`, and set it to `TRUE` in the IF branch.

??? success "Solution"
    A [[RAPID Variable]] `full` (type `bool`). Two [[Assign Variable Value]]s: `TRUE` into
    *IF Actions*, `FALSE` into *ELSE Actions*.

## Gotchas

!!! warning "Flatten the output"
    Like the loops, IF Statement outputs one data-tree level deeper than a [[Move]]. Flatten
    its output (right-click → **Flatten**) so the [[RAPID Generator]] keeps your wire order.
    See the [tutorial warning](../b8-tutorials/row-loop.md#step-by-step).

## See also

[[Comparison Expression]] · [[While Loop]] · [[Message Box]] (operator choices with `TEST`/`CASE`)
