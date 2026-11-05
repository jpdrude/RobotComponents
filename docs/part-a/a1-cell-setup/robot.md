---
title: Robot
description: Define your own robot from meshes, kinematic parameters and joint limits.
---

# ![](../../assets/icons/rc/robot.png){ .rc-icon--title } Robot

<span class="rc-badge">Definitions tab</span>

**Robot** builds a robot from scratch: its name, one mesh per link, its
[[Robot Kinematic Parameters]] and its joint limits. You only need it for a robot that isn't
in [[Robot Preset]], or a customised one. For ABB's catalogue robots, use the preset.

[[io:Robot]]

- **Name**: a name for the robot, e.g. its model (`IRB1200-5/0.9`). It isn't written to RAPID.
- **Meshes**: one mesh per link, base first, in the robot's home pose (all joints at 0).
- **Axis Limits**: six intervals in degrees, from the robot's datasheet.

**In RAPID**, the robot is the mechanical unit `ROB_1` of its task ([[abb:mecunit]]); its six
joint angles are a [[abb:robjoint]] inside every [[abb:jointtarget]].

## See also

[[Robot Preset]] · [[Robot Kinematic Parameters]] · [[Deconstruct Robot]]
