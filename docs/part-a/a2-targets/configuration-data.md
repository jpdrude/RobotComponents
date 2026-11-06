---
title: Configuration Data
description: Tell the robot which of its possible arm poses to use for a target.
---

# ![](../../assets/icons/rc/configuration-data.png){ .rc-icon--title } Configuration Data

<span class="rc-badge">Code Generation tab</span>

Most targets can be reached in several ways: shoulder turned forward or back, elbow up or
down, wrist flipped or not, and axes 1, 4 and 6 a full turn further. **Configuration Data**
(RAPID: `confdata`) says which way you want. Wire it into a [[Robot Target]]'s
*Configuration Data* input. Without it a target gets `[0, 0, 0, 0]`.

[[io:Configuration Data]]

The four values are written exactly as you give them. *CF1*, *CF4* and *CF6* are the
quadrants of axes 1, 4 and 6 (negative values are normal); *CFX* is the arm's posture. They
are independent of each other. The [RAPID primer](../../part-b/b0-rapid-primer.md#arm-configuration-confdata)
explains both with pictures. *CFX* must be 0 to 7; anything else is an error and gives no output.

## In RAPID

```
VAR robtarget p := [[250, 300, 600], [0, 0, 1, 0], [0, 1, -2, 1], [9E9, ...]];
                                                    ^^^^^^^^^^^^^ cf1, cf4, cf6, cfx
```

- **cf1, cf4, cf6**: the *quadrant* of axes 1, 4 and 6: `0` for 0…90°, `1` for 90…180°,
  `-1` for −90…0°, `-2` for −180…−90°, and so on.
- **cfx**: which of the (up to 8) postures of the arm is meant: shoulder, elbow and wrist as
  three bits (4 + 2 + 1).

ABB's reference: [RAPID Instructions, Functions and Data types][abb-ifd] ([[abb:confdata]]).

Finding the right values by hand is tedious. [[Inverse Kinematics]] gives you the robot joint
positions for a target together with their configuration data, and the [[Path Generator]]
can output the configuration along a path (right-click → *Output all Configuration Datas*).

## Configuration Number

![](../../assets/icons/rc/configuration-number.png){ .rc-icon } **Configuration Number**
<span class="rc-badge rc-badge--fork">Fork addition</span> builds *CFX* from the three
posture choices, so you don't have to add up bits:

[[io:Configuration Number]]

| *Shoulder* | *Elbow* | *Wrist* | CFX |
|---|---|---|---|
| | | | 0 |
| | | ✓ | 1 |
| | ✓ | | 2 |
| ✓ | | | 4 |
| ✓ | ✓ | | 6 |
| ✓ | ✓ | ✓ | 7 |

Wire its *CFX* output into Configuration Data's *CFX* input. It does not touch the quadrants;
give those separately.

## Gotchas

!!! note "Whether a configuration is possible depends on the target"
    Configuration Data can't check its values on its own: whether a quadrant can be reached
    depends on the target and the robot. [[Inverse Kinematics]] and the [[Path Generator]] (for the
    target its *Animation Slider* points to) check every quadrant that isn't `0` and warn, e.g.
    *"cf6 = 1 is not possible for this target (robot joint 6 is at 50.2 degrees, cf6 = 0)"*. On the
    controller the same mistake stops a `MoveL` with error 50442.

!!! note "Older files: Upgrade Components"
    Up to version 1.4.5 this component replaced negative quadrants with `0` and worked out
    CFX from the quadrants (and the quadrants from CFX). Both were wrong. Files that contain
    the old component still load it unchanged, so they generate exactly what they did before.
    Use Grasshopper's **Solution → Upgrade Components** (or place a new Configuration Data)
    to switch to the fixed one. Check CFX afterwards: the new component no longer computes
    it, so an unconnected *CFX* is `0`.

## See also

[[Robot Target]] · [[Inverse Kinematics]] · [Configuration control](../a3-motion/configuration-control.md) · [[Deconstruct Configuration Data]] · [RAPID primer: confdata](../../part-b/b0-rapid-primer.md#arm-configuration-confdata)
