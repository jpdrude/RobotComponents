---
title: Joint and Linear Configuration Control
description: Switch the controller's arm-configuration check on or off for MoveJ and MoveL.
---

# ![](../../assets/icons/rc/linear-configuration-control.png){ .rc-icon--title } Joint and Linear Configuration Control

<span class="rc-badge">Code Generation tab</span>
<span class="rc-badge rc-badge--verified">RAPID syntax checked on IRB 1200 · RobotWare 6.10</span>

A six-axis robot can usually reach a target in several ways: elbow up or down, wrist
flipped or not. A target's [[Configuration Data]] says which one you want. With
**configuration control on** (the default), the controller checks that the robot really
arrives in that configuration and stops with an error if it can't. With it **off**, the
controller picks the configuration nearest to where the robot is. That's convenient, but
the arm may flip unexpectedly.

These components switch the check for **all following** movements:
[[Joint Configuration Control]] for [[abb:MoveJ]], [[Linear Configuration Control]] for [[abb:MoveL]].

### Joint Configuration Control
[[io:Joint Configuration Control]]

### Linear Configuration Control
[[io:Linear Configuration Control]]

## In RAPID

```
ConfL\On;      ! Linear Configuration Control, Is Active = True
ConfJ\Off;     ! Joint Configuration Control, Is Active = False
```

ABB's reference: [RAPID Instructions, Functions and Data types][abb-ifd] ([[abb:ConfJ]], [[abb:ConfL]], [[abb:confdata]]).

!!! warning "Leave it on"
    Switching configuration control **off** hides problems instead of fixing them: the robot may
    swing its wrist through 180° in the middle of a path. Give targets correct
    [[Configuration Data]] instead (see [[Inverse Kinematics]] for the configurations that reach a
    target). (`ConfL\On` was syntax-checked on the Virtual Controller; the verifier refuses
    `ConfJ\Off`/`ConfL\Off` by policy for the same reason.)

## See also

[[Configuration Data]] · [[Robot Target]] · [[Inverse Kinematics]]
