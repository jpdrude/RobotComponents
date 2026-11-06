---
title: Inverse Kinematics
description: Compute the joint angles the robot needs to reach a target, and see the robot posed there.
---

# ![](../../assets/icons/rc/inverse-kinematics.png){ .rc-icon--title } Inverse Kinematics

<span class="rc-badge">Simulation tab</span>

**Inverse Kinematics** (IK) answers: *which joint angles put the tool on this target?* Give
it a [[type:Robot]] and a [[Move]] (or a [[Robot Target]] directly) and it outputs the six
joint angles and shows the robot in that pose. Use it to check single targets, to find the
right [[Configuration Data]], or to get joint values for a [[Robot Joint Position]].

[[io:Inverse Kinematics]]

Right-click options: *Output Configuration Data* (the configuration of the found pose),
*Output Posed Meshes*, *Output Posed Axis Planes* ★ (the plane of every joint in the pose),
*Preview Posed Meshes* (on/off), and *Closest Robot Joint Position*: choose the solution
closest to the previous one, useful when stepping along a path.

★ <span class="rc-badge rc-badge--fork">Fork addition</span>

## Example

[:material-download: ik-fk.gh](../../examples/part-a/ik-fk.gh){ .md-button download="ik-fk.gh" }

<div class="rc-canvas" markdown>
![Grasshopper definition: a target plane at (400, 0, 400) pointing down goes into Robot Target and Inverse Kinematics; the resulting joint position goes into Forward Kinematics](../../assets/images/part-a/ik-fk-canvas.png)
</div>

![The IRB 1200 posed by Inverse Kinematics with its tool flange on the target at (400, 0, 400), Z axis pointing down](../../assets/images/part-a/ik-fk-view.png)

For the target from [Getting started](../../getting-started/index.md), (400, 0, 400), tool
down, IK gives:

| Output | Value |
|---|---|
| Robot Joint Position | `[0, 14.63, 41.4, 0, 33.97, 0]` (degrees) |
| Configuration Data | `[0, 0, 0, 0]` |

Feeding those joint angles into [[Forward Kinematics]] gives back exactly (400, 0, 400)
with Z pointing down. IK and FK are each other's inverse.

## The GoFa (CRB 15000) solver <span class="rc-badge rc-badge--fork">Fork addition</span>

ABB's collaborative GoFa robots (CRB 15000) have an offset wrist, which the standard solver
can't handle. In this fork, IK uses a dedicated solver for them and also detects wrist,
elbow and shoulder **singularities**: poses where the robot loses a degree of freedom and
moves badly.

!!! info "Configuration quadrants that can't be reached"
    IK checks that the target's *cf1*, *cf4* and *cf6* can be reached and warns otherwise, e.g.
    *"cf6 = 1 is not possible for this target (robot joint 6 is at 50.2 degrees, cf6 = 0)"*. A
    quadrant of `0` isn't checked: that's also what you get without [[Configuration Data]], or with
    only a *CFX*. The [[Path Generator]] shows the warning only for the target the *Animation Slider*
    currently points to.

!!! info "Check the whole path, not just targets"
    IK checks one pose. A [[abb:MoveL]] between two reachable targets can still pass through a
    singularity or out of reach. Check the whole program with the [[Path Generator]] or
    [[Check Actions]].

## See also

[[Forward Kinematics]] · [[Configuration Data]] · [[Path Generator]] · [[Check Actions]]
