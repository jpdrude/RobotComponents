---
title: Offset Target
description: Shift a Robot Target by X, Y, Z millimetres on the controller with RAPID's Offs function.
---

# ![](../../assets/icons/rc/offset-target.png){ .rc-icon--title } Offset Target

<span class="rc-badge">Advanced RAPID Features tab</span>
<span class="rc-badge rc-badge--fork">Fork addition</span>
<span class="rc-badge rc-badge--verified">RAPID checked on IRB 1200 · RobotWare 6.10</span>

**Offset Target** takes a [[type:Robot Target]] and shifts it by X, Y and Z millimetres,
keeping its orientation. Unlike moving the plane in Grasshopper, the shift is calculated
**on the controller**, so it can use values that only exist while the program runs: a loop
counter, a variable, an operator's input. Wire the result into a [[Move]]'s *Target*.

[[io:Offset Target]]

## In RAPID

```
MoveL Offs(base, 0, i * 100, 0), v200, fine, tool0\WObj:=wobj0;
```

`Offs(target, x, y, z)` is a RAPID **function**: it returns a new robtarget and leaves
`base` unchanged. The offsets are along the X, Y and Z axes of the **work object** the
target is defined in (here `wobj0`, the robot base), not the tool. A typical use is an
approach point above a target: `Offs(pick, 0, 0, 100)`.
ABB's reference: [RAPID Instructions, Functions and Data types][abb-ifd] ([[abb:Offs]]).

Full example: [row tutorial](../b8-tutorials/row-loop.md).

## Simulation

If X, Y and Z are plain numbers, Grasshopper can compute the shifted target, and the
[[Path Generator]] and [[Inverse Kinematics]] can preview the Move. If any of them is an
expression (`i * 100`), the position only exists on the controller and can't be previewed.

## Try it

- [ ] Add an approach move: before the [[abb:MoveL]] to each row point, [[abb:MoveJ]] to a point 100 mm above it.

??? success "Solution"
    A second Offset Target with *Target* `base`, *Y* `i * 100`, *Z* `100`, into a [[Move]]
    (`MoveJ`) that comes **before** the `MoveL` in the loop's *Actions*.

## See also

[[Current Robot Target]] · [[Get Robtarget Members]] · [[Move]] · [[type:RAPID Expression]]
