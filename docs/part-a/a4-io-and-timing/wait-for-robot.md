---
title: Wait for Robot
description: Pause the program until the robot has finished its movement.
---

# ![](../../assets/icons/rc/wait-for-robot.png){ .rc-icon--title } Wait for Robot

<span class="rc-badge">Code Generation tab</span>
<span class="rc-badge rc-badge--fork">Fork addition</span>
<span class="rc-badge rc-badge--verified">RAPID syntax checked on IRB 1200 · RobotWare 6.10</span>

The controller reads ahead: while the robot is still moving, it already prepares (and
sometimes executes) the next lines. **Wait for Robot** stops that: execution continues only
when the robot has reached its target, or, with *Zero Speed*, when it has completely stopped.
Use it before something that must happen *at* the point: reading a sensor, switching an
output with exact timing.

[[io:Wait for Robot]]

## In RAPID

```
WaitRob \InPos;        ! Zero Speed = False: wait until in position
WaitRob \ZeroSpeed;    ! Zero Speed = True: wait until standing still
```

ABB's reference: [RAPID Instructions, Functions and Data types][abb-ifd] (`WaitRob`).

## See also

[[Wait Time]] (with *In Position*) · [[Zone Data]] (`fine`) · [[Set Digital Output]]
