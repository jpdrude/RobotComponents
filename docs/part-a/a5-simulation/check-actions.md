---
title: Check Actions
description: Check every action of a program for reachability and joint limits before you generate code.
---

# ![](../../assets/icons/rc/check-actions.png){ .rc-icon--title } Check Actions

<span class="rc-badge">Simulation tab</span>
<span class="rc-badge rc-badge--fork">Fork addition</span>

**Check Actions** goes through your list of actions one by one and reports every problem it
finds, before you upload anything. Wire in the same [[type:Robot]] and actions you give the
[[RAPID Generator]].

[[io:Check Actions]]

## What it checks

| Action | Check |
|---|---|
| [[Move]] (a [[type:Movement]]) | inverse kinematics: can the robot reach the target with the given configuration, within its joint limits? |
| [[Joint Target]] | forward kinematics: are the joint values within the limits? |
| anything else | is the action valid? |

The *Messages* output lists one line per faulty action (*"Errors in action 3: …"*,
counted from 0 in the order of the list) or *"All actions are valid."*. If there is an
error, the component warns with the number of the first invalid action and shows the robot in
the **last good pose before it**, so you can see where things go wrong.

## Compared to the Path Generator

[[Path Generator]] shows the path and a slider position. Check Actions gives you **every**
problem at once as text, without scrubbing through the path. Use both.

!!! info "Loops, routines and RAPID expressions"
    Actions inside [[For Loop]]s, [[IF Statement]]s or [[Routine]]s, and targets built from
    RAPID expressions, only exist on the controller. Check Actions can't evaluate them. Test
    those programs in RobotStudio or on a Virtual Controller.

## See also

[[Path Generator]] · [[Inverse Kinematics]] · [[Forward Kinematics]] · [[RAPID Generator]]
