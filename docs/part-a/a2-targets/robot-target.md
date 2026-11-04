---
title: Robot Target
description: Turn a plane into a position and orientation the robot's tool should reach.
---

# ![](../../assets/icons/rc/robot-target.png){ .rc-icon--title } Robot Target

<span class="rc-badge">Code Generation tab</span>
<span class="rc-badge rc-badge--verified">RAPID checked on IRB 1200 · RobotWare 6.10</span>

**Robot Target** turns a Rhino **plane** into a place for the tool: the plane's origin is
where the tool tip (TCP) goes, and the plane's axes are how the tool is turned. Most
targets point the tool **down**, so the plane's Z axis must point down: use [[Flip Plane X]]
on an upward plane. Feed the result into a [[Move]].

[[io:Robot Target]]

- **Name** becomes the RAPID name. With a list of planes and one name, the targets are
  numbered: `p` → `p_0`, `p_1`, ….
- **Configuration Data** picks *how* the arm reaches the target when there are several ways
  (elbow up/down, wrist flipped). Leave it empty for `[0, 0, 0, 0]`; see [[Configuration Data]].
- Right-click → **Reference Plane**: the target is written relative to this plane instead of
  the world, e.g. the plane of a [[Work Object]]. Right-click → **External Joint Position**: where
  external axes (a track, a turntable) should be at this target.

## In RAPID

```
VAR robtarget target := [[400, 0, 400], [0, 0, 1, 0], [0, 0, 0, 0], [9E9, 9E9, 9E9, 9E9, 9E9, 9E9]];
```

Position (mm) · orientation (quaternion) · configuration · external axes. The declaration
is written automatically when a [[Move]] uses the target. You don't wire the Robot Target into
the [[RAPID Generator]] yourself. See the [primer](../../part-b/b0-rapid-primer.md#the-data-types-youll-meet)
and ABB's [RAPID Instructions, Functions and Data types][abb-ifd] (`robtarget`).

Examples: [Getting started](../../getting-started/index.md) (one target) and [[Move]] (four from a list).

## Try it

- [ ] Feed an upward [[gh:XY Plane]] straight into Robot Target, without [[Flip Plane X]], and look at the target frame in Rhino. Which way would the tool point?

??? success "Solution"
    Up: the blue Z axis points to the sky. For a tool mounted on the flange that means pointing
    away from the table, and usually the robot can't reach that pose. Flip the plane.

## See also

[[Move]] · [[Flip Plane X]] · [[Configuration Data]] · [[Joint Target]] · [[Offset Target]]
