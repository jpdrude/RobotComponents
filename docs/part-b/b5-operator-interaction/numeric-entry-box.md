---
title: Numeric Entry Box
description: Ask the operator for a number on the FlexPendant and store it in a variable.
---

# ![](../../assets/icons/rc/numeric-entry-box.png){ .rc-icon--title } Numeric Entry Box

<span class="rc-badge">Advanced RAPID Features tab</span>
<span class="rc-badge rc-badge--fork">Fork addition</span>
<span class="rc-badge rc-badge--verified">RAPID syntax checked on IRB 1200 · RobotWare 6.10</span>

**Numeric Entry Box** shows a number pad on the FlexPendant and stores what the operator
types in a [[abb:num]] [[RAPID Variable]]: a height, a count, an offset. *Range* limits what they
can enter; *As Integer* allows whole numbers only.

[[io:Numeric Entry Box]]

## In RAPID

```
h := UINumEntry(\Header:="Height" \Message:="Height above base in mm?" \InitValue:=h \MinValue:=0 \MaxValue:=300 \AsInteger);
```

*Range* `0 To 300` becomes `\MinValue` and `\MaxValue`. Wiring the variable itself into
*Initial Value* (as here) shows the last entered value as the default. ABB's reference:
[RAPID Instructions, Functions and Data types][abb-ifd] ([[abb:UINumEntry]]).

Full example: [[Message Box]] (operator-driven loop).

## See also

[[Message Box]] · [[RAPID Variable]] · [[Offset Target]]
