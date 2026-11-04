---
title: Override Robot Tool
description: Switch to another tool for all following movements, e.g. after a tool change.
---

# ![](../../assets/icons/rc/override-robot-tool.png){ .rc-icon--title } Override Robot Tool

<span class="rc-badge">Code Generation tab</span>

**Override Robot Tool** changes which [[Robot Tool]] the **following** movements use, for
example after a tool changer has swapped the gripper for a spindle. It writes no RAPID
instruction of its own. From that point on, the [[RAPID Generator]] simply writes the new tool
into every Move line (`MoveL p, v200, fine, spindle\WObj:=wobj0;`) and adds a comment.

[[io:Override Robot Tool]]

Without a tool connected it switches back to `tool0`; the generated comment reads
`! Default Robot Tool changed to tool0.`.

```
MoveAbsJ [home, [9E9, 9E9, 9E9, 9E9, 9E9, 9E9]], v5, z0, tool0\WObj:=wobj0;
! Default Robot Tool changed to spindle.
MoveAbsJ [home, [9E9, 9E9, 9E9, 9E9, 9E9, 9E9]], v5, z0, spindle\WObj:=wobj0;
```

!!! warning "The tool must exist on the controller"
    In this test the `tooldata spindle` was **not** declared in the module. Make sure the new tool
    is defined on the controller (e.g. in a system module), or the module won't load.

To change the tool for **one** movement only, use the [[Move]]'s right-click option
*Override Robot Tool* instead.

## See also

[[Robot Tool]] · [[Move]] · [[RAPID Generator]]
