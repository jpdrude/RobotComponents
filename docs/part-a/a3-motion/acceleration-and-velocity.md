---
title: Acceleration and velocity limits
description: Slow down acceleration or cap the speed for all following movements.
---

# ![](../../assets/icons/rc/acceleration-set.png){ .rc-icon--title } Acceleration and velocity limits

<span class="rc-badge">Code Generation tab</span>
<span class="rc-badge rc-badge--verified">RAPID syntax checked on IRB 1200 · RobotWare 6.10</span>

Three components change how hard the robot accelerates and how fast it may go, for **all
following** movements, without editing every [[Speed Data]]. Typical uses: a fragile or
liquid payload, a first test run, a glue nozzle that needs smooth motion.

| Component | RAPID | Effect |
|---|---|---|
| [[Acceleration Set]] | `AccSet 50, 50;` | acceleration and its ramp as % of normal (here: half) |
| [[Velocity Set]] | `VelSet 50, 250;` | programmed speeds × 50 %, and never faster than 250 mm/s |
| [[Path Acceleration Limitation]] | `PathAccLim TRUE \AccMax:=2, FALSE;` | TCP acceleration limited to 2 m/s², deceleration not limited |

### Acceleration Set
[[io:Acceleration Set]]

### Velocity Set
[[io:Velocity Set]]

### Path Acceleration Limitation
[[io:Path Acceleration Limitation]]

ABB's reference: [RAPID Instructions, Functions and Data types][abb-ifd] ([[abb:AccSet]], [[abb:VelSet]], [[abb:PathAccLim]]).

## Gotchas

!!! bug "Path Acceleration Limitation: the generated code doesn't load"
    The component writes `PathAccelerationLimitation TRUE\AccMax:=2, FALSE;`. That isn't a
    RAPID instruction, and the controller rejects the module (checked on a Virtual Controller;
    upstream Robot Components does the same). **Until it's fixed**, use a [[Code]] component with
    the correct line: `PathAccLim TRUE \AccMax:=2, FALSE;` (checked: loads).

!!! info "They stay in force"
    The settings apply until the next instruction of the same kind, also in later routines.
    Reset them at the end, e.g. `AccSet 100, 100;` and `VelSet 100, 5000;`.

## See also

[[Speed Data]] · [[Move]] · [[Override Robot Tool]]
