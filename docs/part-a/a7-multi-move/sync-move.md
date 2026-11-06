---
title: Sync Move On and Off
description: Move several robots together, synchronised, between two points of their programs.
---

# ![](../../assets/icons/rc/sync-move-on.png){ .rc-icon--title } Sync Move On and Off

<span class="rc-badge">Multi Move tab</span>

Between **Sync Move On** and **Sync Move Off**, the robots in a [[Task List]] move
**coordinated**: their movements start and end together, for example two robots carrying one
part, or a robot working on a part held by another. Each task's program needs the same
pair, with the same Sync IDs.

### Sync Move On
[[io:Sync Move On]]

### Sync Move Off
[[io:Sync Move Off]]

## In RAPID

```
VAR syncident sync2;
VAR syncident sync3;
...
SyncMoveOn sync2, allTasks;
    ! coordinated movements
SyncMoveOff sync3;
```

Inside a synchronised section every task must have the **same number** of move instructions,
and each needs an identity (`\ID`) to match them up across tasks. The [[RAPID Generator]]
numbers the Moves between Sync Move On and Off for you (`MoveL p\ID:=…, v200, fine, tool0;`: the identity follows the target).
You only have to keep the count equal in every task. ABB's reference:
[RAPID Instructions, Functions and Data types][abb-ifd] ([[abb:SyncMoveOn]], [[abb:SyncMoveOff]]), and
ABB's MultiMove application manual.

!!! tip "One component per sync point, shared by all tasks"
    Wire the same component into every task's [[RAPID Generator]], as in the
    [Multi Move tutorial](../a9-tutorials/multi-move.md). A second component with the same
    *Sync ID* shows *"The variable name "…" is already in use."*

## See also

[[Task List]] · [[Wait Sync Task]]
