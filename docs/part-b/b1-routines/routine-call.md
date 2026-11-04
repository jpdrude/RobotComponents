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
- *Module Name* *(optional)*: the module the routine lives in. With it, the call is written
  **late-bound**, `%"Helpers:greet"% 5;`, and the routine is only looked up when the line
  runs. Use it for routines in a module that [[Load Module]] loads while the program runs.

## In RAPID

```
visit -150;          ! a PROC call: name, then arguments, no brackets
visit 150;
x := area(10, 20);   ! a FUNC call: inside an expression, with brackets
```

ABB's reference: [RAPID kernel][abb-kernel] ([[abb-kernel:Procedure call|procedure calls]], [[abb-kernel:Function calls|function calls]]).

!!! warning "Calling a routine that isn't loaded yet"
    A normal call (`greet 5;`) must find the routine when the module is **loaded**. If the
    routine is in a module that is only loaded later, the main module fails to load at that
    line. Give the call a *Module Name* so it is late-bound (`%"Helpers:greet"% 5;`): the
    module then loads, and the routine is looked up when the call runs. (Both checked on a
    Virtual Controller.)

Full example: [[Routine]].

## See also

[[Routine]] · [[Routine Argument]] · [[Return]] · [[Load Module]]
