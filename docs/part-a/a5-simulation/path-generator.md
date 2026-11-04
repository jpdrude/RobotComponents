---
title: Path Generator
description: Preview the robot moving through your program in Rhino, and estimate the cycle time.
---

# ![](../../assets/icons/rc/path-generator.png){ .rc-icon--title } Path Generator

<span class="rc-badge">Simulation tab</span>

**Path Generator** plays your program in Rhino: it draws the tool path and shows the robot
at any point along it. Wire the same [[type:Robot]] and the same list of actions you give
the [[RAPID Generator]], and drag a slider from 0 (start) to 1 (end).

[[io:Path Generator]]

Right-click to output more: the current [[type:Movement]], the TCP plane, the
[[type:Robot Joint Position]] and [[type:Configuration Data]] at the slider position, all
of them for the whole path, the **program time** (an estimate, in minutes), the **error
messages** collected along the path (unreachable targets, joint limits), and the posed meshes.

## Example

The [[Move]] example with a Path Generator added: *Robot* from [[Robot Preset]], *Actions*
from the same two Moves (flattened), and a [[gh:Number Slider]] from 0 to 1 on
*Animation Slider*.

![Rhino viewport: the IRB 1200 posed at one corner of the square, the tool path drawn as thin lines between the four target frames](../../assets/images/part-a/path-generator-view.png)

At slider value `0.62` the robot is at a corner of the square. *Program Time* gives
**0.096 min ≈ 5.8 s**. The same program measured **6.2 s** on the Virtual Controller: close,
but an estimate. The controller's acceleration and corner zones decide the real time.

## What it can and can't tell you

| It shows | It doesn't check |
|---|---|
| whether every target is reachable, and joint-limit problems | **collisions** with your cell, fixtures or the robot itself |
| how `MoveJ` and `MoveL` paths differ | the exact path inside corner zones |
| roughly how long the program takes | anything that is only known on the controller: [Part B](../../part-b/index.md) variables, `Offs(…, i * 100, …)`, operator input |

!!! tip "Hide the Robot Preset preview"
    [[Robot Preset]] draws its own robot in the home pose. Right-click it → *Preview* off,
    so only the animated robot is visible.

## Try it

- [ ] Switch the square's movement type to `MoveJ` and compare the paths between the corners.
- [ ] Add the *Error Messages* output and push the square out of reach (X slider to 600, Size to 300, so the far corners are 900 mm away). What does it say?

## See also

[[Timed Path Generator]] · [[Inverse Kinematics]] · [[Check Actions]] · [[RAPID Generator]]
