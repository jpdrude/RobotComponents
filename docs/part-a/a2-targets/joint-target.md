---
title: Joint Target
description: Combine robot joint angles and external axis positions into one joint target.
---

# ![](../../assets/icons/rc/joint-target.png){ .rc-icon--title } Joint Target

<span class="rc-badge">Code Generation tab</span>

**Joint Target** combines a [[type:Robot Joint Position]] (the six robot joints) and an
[[type:External Joint Position]] (tracks, turntables) into one named `jointtarget` for a
`MoveAbsJ`.

[[io:Joint Target]]

## In RAPID

```
VAR jointtarget home := [[0, 0, 0, 0, 30, 0], [9E9, 9E9, 9E9, 9E9, 9E9, 9E9]];
```

ABB's reference: [RAPID Instructions, Functions and Data types][abb-ifd] ([[abb:jointtarget]]).

## Gotchas

!!! bug "Known issue: does not load with a named Robot Joint Position"
    Robot Components writes the robot joints **by name**:
    `VAR jointtarget home := [home_rj, [9E9, …]];`. RAPID only accepts constant values in a
    `VAR` declaration, so the controller rejects the module (checked on a Virtual Controller).
    The *Name* of [[Robot Joint Position]] is required, so this always happens.

    **Workaround:** without external axes, skip the Joint Target and wire the
    [[Robot Joint Position]] directly into the [[Move]]. It is then written inline
    (`MoveAbsJ [home, [9E9, …]], …`) and loads.

## See also

[[Robot Joint Position]] · [[External Joint Position]] · [[Move]]
