---
title: Empty Line
description: Add a blank line to the generated RAPID for readability, or as a placeholder action.
---

# ![](../../assets/icons/rc/empty-line.png){ .rc-icon--title } Empty Line

<span class="rc-badge">Advanced RAPID Features tab</span>
<span class="rc-badge rc-badge--fork">Fork addition</span>

**Empty Line** outputs a blank line. Wire it between groups of actions to make the
generated program easier to read. Right-click to add a *Type* input to place it among the
declarations instead of the instructions.

[[io:Empty Line]]

!!! tip "Also useful as a placeholder"
    Some components need *something* on an input to behave correctly. On a [[Message Box]],
    every button needs an action, or the wrong button runs the actions. An Empty Line is the
    harmless filler. In the [operator loop](../b5-operator-interaction/message-box.md) it
    produces an empty `CASE 1:` for the *Yes* button.

## See also

[[Comment]] · [[Code]] · [[Message Box]]
