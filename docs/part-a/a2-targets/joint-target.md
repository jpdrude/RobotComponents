---
title: Joint Target
description: Combine robot joint angles and external axis positions into one joint target.
---

# ![](../../assets/icons/rc/joint-target.png){ .rc-icon--title } Joint Target

<span class="rc-badge">Code Generation tab</span>

**Joint Target** combines a [[type:Robot Joint Position]] (the six robot joints) and an
[[type:External Joint Position]] (tracks, turntables) into one named `jointtarget` for a
[[abb:MoveAbsJ]].

[[io:Joint Target]]

## In RAPID

A `jointtarget` is the six robot joints plus the six external axes:

```
VAR jointtarget home := [[0, 0, 0, 0, 30, 0], [9E9, 9E9, 9E9, 9E9, 9E9, 9E9]];
```

The [[Robot Joint Position]] has a name of its own (`home_rj`), and it is declared too. The
`jointtarget` declaration still contains its **values**, not its name: the value after `:=`
must be a literal or a `CONST` (see the [primer](../../part-b/b0-rapid-primer.md#data-three-kinds-of-storage)).
Where a Robot Joint Position is used directly in an instruction, it is written by name:
`MoveAbsJ [home_rj, [9E9, …]], …`.

ABB's reference: [RAPID Instructions, Functions and Data types][abb-ifd] ([[abb:jointtarget]]).

## See also

[[Robot Joint Position]] · [[External Joint Position]] · [[Move]]
