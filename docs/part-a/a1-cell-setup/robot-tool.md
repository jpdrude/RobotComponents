---
title: Robot Tool
description: Describe the tool on the robot's flange (gripper, pen, spindle) so the robot moves its tip, not its flange.
---

# ![](../../assets/icons/rc/robot-tool.png){ .rc-icon--title } Robot Tool

<span class="rc-badge">Definitions tab</span>
<span class="rc-badge rc-badge--verified">RAPID checked on IRB 1200 · RobotWare 6.10</span>

A target says where the **tool center point (TCP)** should go: the pen tip, the middle of
the gripper fingers, the end of the spindle. **Robot Tool** tells the robot where that point
is relative to its flange, so it can do the maths. Without a tool, the TCP is the flange
itself (`tool0`).

[[io:Robot Tool]]

- **Attachment Plane**: where the tool sits on the flange, in the coordinates you modelled
  the tool in. Model the tool standing on the World XY plane and use [[gh:XY Plane]].
- **Tool Plane**: the TCP and its orientation, in the same coordinates. A pen 120 mm long,
  pointing straight out: an [[gh:XY Plane]] at (0, 0, 120).
- **Mesh** *(optional)*: the tool's geometry for the simulation.
- **Load Data** *(optional)*: mass and centre of gravity, from [[Load Data]].

Wire the tool into [[Robot Preset]] (*Robot Tool*). The [[RAPID Generator]] writes it as
`tooldata` with right-click → *Add Tool Data*, and every Move then uses it.

## In RAPID

```
PERS tooldata pen := [TRUE, [[0, 0, 120], [1, 0, 0, 0]], [0.001, [0, 0, 0.001], [1, 0, 0, 0], 0, 0, 0]];
MoveL p_0, v100, fine, pen\WObj:=table;
```

`TRUE` (the robot holds the tool) · TCP position and orientation · load (mass in kg, centre
of gravity, …). ABB's reference: [RAPID Instructions, Functions and Data types][abb-ifd] (`tooldata`).

Full example: [[Work Object]] (a pen drawing on a rotated table).

!!! warning "Measure real tools"
    A TCP that's off by a few millimetres moves every point. On the real robot, measure the
    TCP with the controller's TCP calibration, or use [[Robot Tool Calibration]], and give
    heavy tools correct [[Load Data]], since the controller uses it to drive the motors.

## See also

[[Robot Preset]] · [[Load Data]] · [[Work Object]] · [[Override Robot Tool]] · [[Deconstruct Robot Tool]]
