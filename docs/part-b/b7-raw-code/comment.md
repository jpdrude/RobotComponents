---
title: Comment
description: Add a comment line to the generated RAPID, to explain the program to whoever reads it on the controller.
---

# ![](../../assets/icons/rc/comment.png){ .rc-icon--title } Comment

<span class="rc-badge">Advanced RAPID Features tab</span>

**Comment** adds a line starting with `!` to the program. The controller ignores it, but
the person reading the code on the FlexPendant or in RobotStudio doesn't: "approach the
part", "glue path 2 of 4". *Type* `0` places it among the instructions (where you wire it in
*Actions*), `1` among the declarations.

[[io:Comment]]

## In RAPID

```
! Approach the part
MoveJ Offs(pick, 0, 0, 100), v200, z10, tool0\WObj:=wobj0;
```

Everything after `!` on a line is a comment. ABB's reference: [RAPID kernel][abb-kernel] ([[abb-kernel:Comments|comments]]).

## See also

[[Code]] · [[Empty Line]]
