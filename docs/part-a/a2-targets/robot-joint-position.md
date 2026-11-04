---
title: Robot Joint Position
description: Six joint angles in degrees: a robot pose given by the joints instead of the tool position.
---

# ![](../../assets/icons/rc/robot-joint-position.png){ .rc-icon--title } Robot Joint Position

<span class="rc-badge">Code Generation tab</span>
<span class="rc-badge rc-badge--verified">RAPID checked on IRB 1200 · RobotWare 6.10</span>

**Robot Joint Position** describes a pose by its **six joint angles** in degrees rather than
by where the tool is. It is the safest way to start a program: a [[Move]] of type `MoveAbsJ`
goes there directly, whatever pose the robot was in.

[[io:Robot Joint Position]]

Wire it **straight into a [[Move]]'s *Target*** (type `MoveAbsJ`), as in
[Getting started](../../getting-started/index.md).

## In RAPID

```
VAR robjoint home := [0, 0, 0, 0, 30, 0];
...
MoveAbsJ [home, [9E9, 9E9, 9E9, 9E9, 9E9, 9E9]], v200, z10, tool0\WObj:=wobj0;
```

ABB's reference: [RAPID Instructions, Functions and Data types][abb-ifd] (`robjoint`, `MoveAbsJ`).

!!! tip "Don't start straight"
    Joint 5 at `0` lines up joints 4 and 6, a wrist **singularity** where the robot can't
    move smoothly. Start with joint 5 bent (e.g. 30°).

## Gotchas

!!! bug "Through a Joint Target it doesn't load"
    [[Robot Joint Position]] → [[Joint Target]] → [[Move]] generates a declaration the controller
    rejects. Wire the Robot Joint Position into the Move directly. See [[Joint Target]].

## See also

[[Joint Target]] · [[Move]] · [[Forward Kinematics]]
