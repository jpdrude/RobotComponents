---
title: Load Data
description: Mass and centre of gravity of a tool or payload, so the controller can drive the motors correctly.
---

# ![](../../assets/icons/rc/load-data.png){ .rc-icon--title } Load Data

<span class="rc-badge">Definitions tab</span>

The controller needs to know what the robot carries: the mass and where its centre of
gravity is. It uses this to drive the motors, to detect collisions and to avoid overloading
the robot. **Load Data** describes a load; wire it into a [[Robot Tool]] (the tool's own weight).

[[io:Load Data]]

## In RAPID

The load becomes part of the `tooldata`. The [[Robot Tool]] example has the minimal load:

```
PERS tooldata pen := [TRUE, [[0, 0, 120], [1, 0, 0, 0]], [0.001, [0, 0, 0.001], [1, 0, 0, 0], 0, 0, 0]];
                                                           ^ mass  ^ centre of gravity          ^ inertia
```

ABB's reference: [RAPID Instructions, Functions and Data types][abb-ifd] ([[abb:loaddata]]).

!!! warning "Wrong load data is a real problem"
    Too little mass makes the robot move too aggressively and can trigger collision detection
    or overload; ABB robots can identify a tool's load themselves (*LoadIdentify* service
    routine). Use measured values for heavy tools.

## See also

[[Robot Tool]] · [[Deconstruct Load Data]]
