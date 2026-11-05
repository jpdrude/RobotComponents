---
title: Robot Preset
description: Pick an ABB robot model from a list and place it in your scene.
---

# ![](../../assets/icons/rc/robot-preset.png){ .rc-icon--title } Robot Preset

<span class="rc-badge">Definitions tab</span>

**Robot Preset** gives you a ready-made ABB robot: geometry, kinematics and joint limits.
When you place it, a dialog asks which model you want. To pick another model later,
right-click → **Pick Robot Preset**. The model name shows below the component (e.g. `IRB1200`).

[[io:Robot Preset]]

- **Position Plane**: where the robot's base stands, usually an [[gh:XY Plane]] at the origin.
- **Robot Tool** *(optional)*: the tool on the flange, from [[Robot Tool]]. Without one, the
  robot uses `tool0`, the bare flange.
- **External Axis** *(optional)*: a track or turntable that carries the robot or the part,
  from [[External Linear Axis]] / [[External Rotational Axis]].

The *Robot* output goes into the [[RAPID Generator]] and the simulation components
([[Path Generator]], [[Inverse Kinematics]], [[Forward Kinematics]]).

Example: [Getting started](../../getting-started/index.md).

!!! tip "Match the controller"
    Pick the exact model and variant of your robot (e.g. *IRB1200-5/0.9*, not *-7/0.7*). The
    reach and joint limits differ, and the simulation only matches the real robot if the model does.

## See also

[[Robot]] (define your own) · [[Robot Tool]] · [[External Linear Axis]] · [[Deconstruct Robot]]
