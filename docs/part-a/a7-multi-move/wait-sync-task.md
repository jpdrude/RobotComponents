---
title: Wait Sync Task
description: Make several robots wait for each other at a meeting point in their programs.
---

# ![](../../assets/icons/rc/wait-sync-task.png){ .rc-icon--title } Wait Sync Task

<span class="rc-badge">Multi Move tab</span>

**Wait Sync Task** is a meeting point: each robot's program stops there until **all** tasks
in the [[Task List]] have reached the meeting point with the same *Sync ID*. Then they all
continue. Use it so one robot only starts after the other has put a part down, for example.

[[io:Wait Sync Task]]

## In RAPID

```
VAR syncident sync1;
...
WaitSyncTask \InPos, sync1, allTasks;
```

`\InPos` (*In Position* = `True`) also waits until all robots stand still. Put the same
Wait Sync Task (same *Sync ID*, same [[Task List]]) into every task's program at the matching
point. ABB's reference: [RAPID Instructions, Functions and Data types][abb-ifd] (`WaitSyncTask`, `syncident`).

!!! bug "Known issue: error badge on the component"
    The component shows *"After Solution exception: Object reference not set…"* on every
    solve. The generated RAPID is still correct, but duplicate Sync IDs aren't detected. Use a
    different Sync ID for every meeting point.

## See also

[[Task List]] · [[Sync Move On]] · [[Sync Move Off]]
