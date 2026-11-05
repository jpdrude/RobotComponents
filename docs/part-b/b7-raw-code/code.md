---
title: Code
description: Write a line of RAPID by hand, as an instruction or a declaration.
---

# ![](../../assets/icons/rc/code.png){ .rc-icon--title } Code

<span class="rc-badge">Advanced RAPID Features tab</span>

**Code** puts one hand-written line of RAPID into the program, for the things no
component covers yet. *Type* `0` places it among the instructions, *Type* `1` among the
declarations at the top of the module ([[Code Types]] gives a drop-down).

[[io:Code]]

Example from [[Connect Interrupt]], a declaration (*Type* `1`):

```
VAR signaldi diPartSensor;
```

Instructions (*Type* `0`) go where you wire them in *Actions*, e.g. `TPWrite "Cycle done";`
to show a message on the FlexPendant.

ABB's reference: [[abb-kernel:Statements]] and [[abb-kernel:Data declarations]] in the
RAPID kernel manual, and each instruction in [RAPID Instructions, Functions and Data types][abb-ifd].

## Gotchas

!!! warning "Robot Components does not check your RAPID"
    The line is copied as written, apart from basic sanitizing against injected extra lines.
    Typos only show up when the controller loads the module. Check it with
    RobotStudio or a Virtual Controller.

!!! warning "Flatten the output"
    *Code* outputs on a deeper data-tree branch than a [[Move]]. To keep your wire order,
    right-click its output → **Flatten** (see [[Connect Interrupt]] → Gotchas).

## See also

[[Comment]] · [[Empty Line]] · [[Code Types]]
