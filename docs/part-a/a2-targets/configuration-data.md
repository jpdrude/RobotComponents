---
title: Configuration Data
description: Tell the robot which of its possible arm poses to use for a target.
---

# ![](../../assets/icons/rc/configuration-data.png){ .rc-icon--title } Configuration Data

<span class="rc-badge">Code Generation tab</span>

Most targets can be reached in several ways: shoulder turned forward or back, elbow up or
down, wrist flipped or not. **Configuration Data** (RAPID: `confdata`) says which way you
want. Wire it into a [[Robot Target]]'s *Configuration Data* input. Without it a target gets
`[0, 0, 0, 0]`.

[[io:Configuration Data]]

## In RAPID

```
VAR robtarget p := [[400, -150, 300], [0, 0, 1, 0], [-1, 0, -1, 0], [9E9, ...]];
                                                      ^^^^^^^^^^^^^ cf1, cf4, cf6, cfx
```

- **cf1, cf4, cf6**: the *quadrant* of axes 1, 4 and 6: `0` for 0…90°, `1` for 90…180°,
  `-1` for −90…0°, `-2` for −180…−90°, and so on.
- **cfx**: which of the (up to 8) solutions for the arm is meant. Its exact meaning depends on
  the robot family.

ABB's reference: [RAPID Instructions, Functions and Data types][abb-ifd] (`confdata`).

Finding the right values by hand is tedious. [[Inverse Kinematics]] gives you the robot joint
positions for a target together with their configuration data, and the [[Path Generator]]
can output the configuration along a path (right-click → *Output all Configuration Datas*).

## The quadrant inputs <span class="rc-badge rc-badge--fork">Fork addition</span>

In this fork you can leave *CFX* unset and give only *CF1/CF4/CF6*: the component then builds
CFX from them as a bitmask (written for the CRB 15000 / GoFa convention). If you give CFX, it
derives CF1/CF4/CF6 from it. Giving both gives a warning, and CFX wins.

## Gotchas

!!! bug "Known issue: negative quadrants become 0"
    The component treats any negative CF1/CF4/CF6 as "not set" and replaces it with `0`
    (`(-1, 0, 1)` gives `[0, 0, 1, 1]`). Negative quadrants are valid and common, e.g. targets
    on the robot's −Y side. **Until this is fixed**, check the generated robtarget; for negative
    quadrants, define the target's configuration another way (e.g. with a [[Code]] declaration).

## See also

[[Robot Target]] · [[Inverse Kinematics]] · [Configuration control](../a3-motion/configuration-control.md) · [[Deconstruct Configuration Data]]
