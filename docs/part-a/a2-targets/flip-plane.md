---
title: Flip Plane X and Y
description: Turn a plane upside down so the tool points the other way.
---

# ![](../../assets/icons/rc/flip-plane-x.png){ .rc-icon--title } Flip Plane X and Y

<span class="rc-badge">Utility tab</span>

Planes from Rhino usually have their Z axis pointing **up**, but a tool usually points
**down** at the work. The flip components turn a plane over:

- [[Flip Plane X]] reverses the plane's **X axis** (and so its Z axis). The Y axis stays.
- [[Flip Plane Y]] reverses the plane's **Y axis** (and so its Z axis). The X axis stays.

Either way the Z axis now points down. Which one you use decides where the tool's X axis
points, and so how the wrist is turned at the target. Watch the target frames in Rhino.

### Flip Plane X
[[io:Flip Plane X]]

### Flip Plane Y
[[io:Flip Plane Y]]

Example: [Getting started](../../getting-started/index.md). An [[gh:XY Plane]] at a point →
Flip Plane X → [[Robot Target]] gives the orientation `[0, 0, 1, 0]`: tool straight down.

## See also

[[Robot Target]] · [[Plane Visualizer]] · [Plane and quaternion](plane-and-quaternion.md)
