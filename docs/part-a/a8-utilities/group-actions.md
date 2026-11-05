---
title: Group and Ungroup Actions
description: Pack a list of actions into one item and unpack it again.
---

# ![](../../assets/icons/rc/group-actions.png){ .rc-icon--title } Group and Ungroup Actions

<span class="rc-badge">Utility tab</span>

**Group Actions** packs a list of actions into a single [[type:Action Group]]. It travels
through Grasshopper as **one item**, so a whole sequence ("approach, grip, retract") can be
treated like a single step: copied into a list several times, picked with [[gh:List Item]],
reordered. **Ungroup Actions** unpacks it again.

### Group Actions
[[io:Group Actions]]

### Ungroup Actions
[[io:Ungroup Actions]]

!!! tip "Groups vs. routines"
    A group is only a Grasshopper convenience: every copy is written out in full in the
    RAPID program. To write a sequence **once** and call it several times on the controller,
    use a [[Routine]] (Part B).

## See also

[[Routine]] · [[Multi Relay]] · [[RAPID Generator]]
