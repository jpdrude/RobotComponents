---
title: Forward Kinematics
description: Pose the robot from joint angles and get the resulting tool position.
---

# ![](../../assets/icons/rc/forward-kinematics.png){ .rc-icon--title } Forward Kinematics

<span class="rc-badge">Simulation tab</span>

**Forward Kinematics** (FK) is the opposite of [[Inverse Kinematics]]: give it joint angles,
and it shows the robot in that pose and outputs where the tool ends up (*End Plane*). Use it
to check a [[Robot Joint Position]] (is my home pose safe? where is the tool?) or to animate
joints with sliders.

[[io:Forward Kinematics]]

Example: in [ik-fk.gh](../../examples/part-a/ik-fk.gh) (see [[Inverse Kinematics]]) the
joint angles `[0, 14.63, 41.4, 0, 33.97, 0]` give an end plane at (400, 0, 400) with Z
pointing down, the target IK started from.

## Try it

- [ ] Connect six [[gh:Number Slider|sliders]] to a [[Robot Joint Position]] and that to
  Forward Kinematics. Move joint 5 to `0`. What happens to joints 4 and 6? *(Hint: wrist singularity.)*

??? success "Solution"
    With joint 5 at 0°, the axes of joints 4 and 6 line up: turning one has the same effect as
    turning the other. That's the wrist singularity. Programs should avoid passing through it.

## See also

[[Inverse Kinematics]] · [[Robot Joint Position]] · [[External Joint Position]]
