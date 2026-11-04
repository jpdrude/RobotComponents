---
title: External Rotational Axis
description: A turntable or positioner that rotates the part (or the robot).
---

# ![](../../assets/icons/rc/external-rotational-axis.png){ .rc-icon--title } External Rotational Axis

<span class="rc-badge">Definitions tab</span>

An **external rotational axis** is a turntable or positioner: it rotates around *Axis
Plane*'s Z axis within *Axis Limits* (degrees). Usually it turns the **part** so the robot can
reach all sides. Wire it into [[Robot Preset]] (*External Axis*), and give targets the
turntable angle with [[External Joint Position]].

[[io:External Rotational Axis]]

Right-click → *Axis Logic Number* and *Moves Robot*: as for [[External Linear Axis]].

**In RAPID**, the axis is a mechanical unit on the controller ([[abb:mecunit]]), and its
angle in degrees is the `eax_…` value of the [[abb:extjoint]] inside every robtarget and jointtarget.

## See also

[[External Linear Axis]] · [[External Joint Position]] · [[Work Object]] · [[Deconstruct External Rotational Axis]]
