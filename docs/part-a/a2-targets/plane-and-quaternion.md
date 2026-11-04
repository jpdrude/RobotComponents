---
title: Plane to Quaternion, Quaternion to Plane
description: Convert between Rhino planes and the position + quaternion numbers used by ABB robots.
---

# ![](../../assets/icons/rc/plane-to-quaternion.png){ .rc-icon--title } Plane to Quaternion, Quaternion to Plane

<span class="rc-badge">Utility tab</span>

ABB robots describe an orientation as a **quaternion**: four numbers `[q1, q2, q3, q4]`
(RAPID) or `a + bi + cj + dk` (here: *A* is the real part `q1`, *B*/*C*/*D* are `q2`/`q3`/`q4`).
Rhino uses planes. These two components convert between them, for example to compare with
numbers from the FlexPendant, or to rebuild a plane from a position taught on the robot.

### Plane to Quaternion
[[io:Plane to Quaternion]]

*Reference Plane* is the coordinate system the numbers are measured in: World XY for the robot
base, or a [[Work Object]]'s plane.

### Quaternion to Plane
[[io:Quaternion to Plane]]

## Examples

| Plane | Quaternion `[q1, q2, q3, q4]` |
|---|---|
| World XY (tool pointing up) | `[1, 0, 0, 0]` |
| World XY flipped with [[Flip Plane X]] (tool pointing down) | `[0, 0, 1, 0]` |

Both appear in this wiki's generated code: `[1, 0, 0, 0]` in `tooldata`/`wobjdata`, and
`[0, 0, 1, 0]` in the robtargets. ABB's reference: [RAPID Instructions, Functions and Data types][abb-ifd] ([[abb:orient]]).

## See also

[[Robot Target]] · [[Flip Plane X]] · [[Plane Visualizer]]
