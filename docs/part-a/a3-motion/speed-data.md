---
title: Speed Data
description: How fast the robot moves, from predefined values (v5 … v7000) or your own.
---

# ![](../../assets/icons/rc/speed-data.png){ .rc-icon--title } Speed Data

<span class="rc-badge">Code Generation tab</span>

Every [[Move]] needs a speed. For most programs, use **[[Predefined Speed Data]]**: a
drop-down of ABB's standard speeds (`v5` … `v7000`, the number is the tool speed in mm/s).
**Speed Data** defines your own, named speed when you need to set the rotation or
external-axis speeds too.

The quickest way: wire a **number** straight into the Move's *Speed Data*. It picks the
nearest predefined speed (`200` → `v200`, with a warning if it isn't exact).

## Predefined Speed Data

![](../../assets/icons/rc/predefined-speed-data.png){ .rc-icon } Placing [[Predefined Speed Data]]
creates a drop-down list. Pick a value and wire it into *Speed Data* of one or more Moves.

## Speed Data (custom)

[[io:Speed Data]]

## In RAPID

```
v200                                              ! predefined: [200, 500, 5000, 1000]
VAR speeddata slow := [50, 30, 200, 15];          ! custom
```

The four numbers: tool (TCP) mm/s, tool reorientation °/s, linear external axes mm/s,
rotating external axes °/s. The robot moves at the slowest that still respects all four.
ABB's reference: [RAPID Instructions, Functions and Data types][abb-ifd] ([[abb:speeddata]]).

## Try it

- [ ] In the [[Move]] example, slow the square down to `v50` but keep the move to the start pose at `v200`. *(Hint: two value lists.)*

??? success "Solution"
    Give each Move its own [[Predefined Speed Data]] list: `v200` for the MoveAbsJ, `v50` for the square.

## See also

[[Zone Data]] · [[Move]] · [[Deconstruct Speed Data]]
