---
title: Routine Call
description: Call a routine (PROC or FUNC) by name, with argument values.
---

# ![](../../assets/icons/rc/routine-call.png){ .rc-icon--title } Routine Call

<span class="rc-badge">Advanced RAPID Features tab</span>
<span class="rc-badge rc-badge--fork">Fork addition</span>
<span class="rc-badge rc-badge--verified">RAPID checked on IRB 1200 · RobotWare 6.10</span>

**Routine Call** writes the line that runs a routine: one of your own [[Routine]]s, or a
routine that already exists on the controller (for example in a system module loaded with
[[Load Module]]). Click **+** to add *Argument Value* inputs, one per argument. A list of values
makes one call per value.

[[io:Routine Call]]

- *Type* `0` (PROC) → **Routine Call** output, a line for *Actions*: `visit 150;`
- *Type* `1` (FUNC) → **Routine Call Expression** output, a value to use inside another
  expression, e.g. in [[Assign Variable Value]] or [[Comparison Expression]]: `area(10, 20)`

## In RAPID

```
visit -150;          ! a PROC call: name, then arguments, no brackets
visit 150;
x := area(10, 20);   ! a FUNC call: inside an expression, with brackets
```

ABB's reference: [RAPID kernel][abb-kernel] (procedure calls, function calls).

Full example: [[Routine]].

## See also

[[Routine]] · [[Routine Argument]] · [[Return]] · [[Load Module]]
