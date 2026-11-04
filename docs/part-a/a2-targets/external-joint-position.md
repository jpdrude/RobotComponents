---
title: External Joint Position
description: Where the external axes (track, turntable) should be at a target.
---

# ![](../../assets/icons/rc/external-joint-position.png){ .rc-icon--title } External Joint Position

<span class="rc-badge">Code Generation tab</span>

With a track or turntable, every target also says where those **external axes** should be.
**External Joint Position** holds up to six values, one per external axis `A` … `F` (mm for
linear axes, degrees for rotational ones). Unused axes are `9E9`. Wire it into a
[[Robot Target]] (right-click → *External Joint Position*) or a [[Joint Target]].

[[io:External Joint Position]]

## In RAPID

A track at 1200 mm, nothing else:

```
[1200, 9E9, 9E9, 9E9, 9E9, 9E9]
```

This is the last part of every `robtarget` and `jointtarget` (`extax`). ABB's reference:
[RAPID Instructions, Functions and Data types][abb-ifd] (`extjoint`).

!!! warning "Real external axes need real values"
    On a controller with external axes, a target with `9E9` for an axis that exists is an
    error. Without an External Joint Position, Robot Components calculates the external axis
    values itself for Moves to [[Robot Target]]s.

## See also

[[External Linear Axis]] · [[External Rotational Axis]] · [[Robot Target]] · [[Deconstruct External Joint Position]]
