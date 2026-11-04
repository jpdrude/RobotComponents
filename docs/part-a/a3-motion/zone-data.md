---
title: Zone Data
description: How closely the robot passes a target, from a stop exactly on the point (fine) to smooth rounded corners (z5 … z200).
---

# ![](../../assets/icons/rc/zone-data.png){ .rc-icon--title } Zone Data

<span class="rc-badge">Code Generation tab</span>
<span class="rc-badge rc-badge--verified">RAPID checked on IRB 1200 · RobotWare 6.10</span>

The zone says how closely a [[Move]] must reach its target before the robot starts on the
next one. **`fine`** stops exactly on the point. **`z10`** lets the robot cut the corner
within 10 mm, which gives a smooth, faster path without stopping. Use [[Predefined Zone Data]]
for ABB's standard zones, or **Zone Data** for your own. A number on a Move's *Zone Data*
picks the nearest predefined zone; `-1` means `fine`.

## Predefined Zone Data

![](../../assets/icons/rc/predefined-zone-data.png){ .rc-icon } Placing [[Predefined Zone Data]]
creates a drop-down list: `fine`, `z0`, `z1`, `z5`, `z10`, … `z200`.

## Zone Data (custom)

[[io:Zone Data]]

## In RAPID

```
MoveL p_2, v200, z10, tool0\WObj:=wobj0;     ! round the corner within 10 mm
MoveL p_3, v200, fine, tool0\WObj:=wobj0;    ! stop exactly here
```

ABB's reference: [RAPID Instructions, Functions and Data types][abb-ifd] ([[abb:zonedata]]).

## Gotchas

!!! warning "End on `fine`"
    If the last movement of a program has a zone (`z10`), the controller can't finish the
    corner and warns *50024 Corner path failure* (seen on the Virtual Controller in the
    [[Move]] example). Use `fine` for the last point, and wherever the robot must really be on
    the point: before gripping, before switching an output.

## See also

[[Speed Data]] · [[Move]] · [[Deconstruct Zone Data]]
