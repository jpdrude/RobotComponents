---
title: Wait Time
description: Pause the program for a number of seconds.
---

# ![](../../assets/icons/rc/wait-time.png){ .rc-icon--title } Wait Time

<span class="rc-badge">Code Generation tab</span>
<span class="rc-badge rc-badge--verified">RAPID syntax checked on IRB 1200 · RobotWare 6.10</span>

**Wait Time** pauses the program for *Duration* seconds, e.g. to give a gripper time to
close or glue time to set.

[[io:Wait Time]]

*In Position* = `True` first waits until the robot has completely **stopped** at its last
target, then starts counting. Use it after a Move with a zone, so the pause really happens at
the point.

## In RAPID

```
WaitTime 0.5;            ! wait 0.5 s
WaitTime \InPos, 0.5;    ! wait until the robot stands still, then 0.5 s
```

ABB's reference: [RAPID Instructions, Functions and Data types][abb-ifd] ([[abb:WaitTime]]).

Example: the gripper sequence on [Set outputs](set-outputs.md).

## Gotchas

!!! warning "Duration is the *second* input"
    The first input is *In Position*. A number wired there by mistake becomes `\InPos`
    (it was `True`), and the duration stays at its default.

## See also

[[Wait for Digital Input]] · [[Wait for Robot]] · [[Set Digital Output]]
