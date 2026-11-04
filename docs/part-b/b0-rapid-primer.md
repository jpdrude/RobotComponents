---
title: "B0 RAPID primer"
description: Just enough RAPID to read what Robot Components generates and to use the Advanced RAPID Features.
---

# B0 RAPID primer

**RAPID** is the programming language of ABB robot controllers. In Part A you never had to
read it. Part B's components write real RAPID logic, so this page gives you the
vocabulary: how a program is built, what data looks like, and how to read a generated
module. Everything here is checked on a Virtual Controller.

For the complete language rules, see ABB's [RAPID kernel][abb-kernel] and
[RAPID overview][abb-overview] manuals.

## A program has three levels

| Level | What it is | In Robot Components |
|---|---|---|
| **Task** | One program running on the controller, usually one per robot (`T_ROB1`). | One [[RAPID Generator]] per task; several in [Multi Move](../part-a/index.md). |
| **Module** | A file of code inside a task: `MODULE … ENDMODULE`. A task can load several. | The *Module* output of the [[RAPID Generator]] (default name `MainModule`). |
| **Routine** | A named block of instructions inside a module. | `PROC main()` from your actions; more with [[Routine]]. |

There are three kinds of routines:

- **`PROC`**, a procedure: does something, returns nothing. `main` is the one the controller starts.
- **`FUNC`**, a function: computes and **returns** a value (like `Offs(...)` below).
- **`TRAP`**, an interrupt routine: runs when a signal changes. See [[Connect Interrupt]].

## Reading a module line by line

This is the module from [Getting started](../getting-started/index.md):

```
MODULE MainModule                                              ! (1)
    VAR robjoint home := [0, 0, 0, 0, 30, 0];                  ! (2)
    VAR robtarget target := [[400, 0, 400], [0, 0, 1, 0],
                             [0, 0, 0, 0], [9E9, 9E9, 9E9, 9E9, 9E9, 9E9]];

    PROC main()                                                ! (3)
        MoveAbsJ [home, [9E9, 9E9, 9E9, 9E9, 9E9, 9E9]], v200, z10, tool0\WObj:=wobj0;   ! (4)
        MoveJ target, v200, fine, tool0\WObj:=wobj0;
    ENDPROC
ENDMODULE
```

1. Everything lives inside `MODULE <name> … ENDMODULE`. A `!` starts a comment.
2. **Declarations** first: `VAR <type> <name> := <value>;`. Here a set of joint angles
   (`robjoint`) and a position (`robtarget`). Values in `[ … ]` are *aggregates*: one value
   made of several parts.
3. `PROC main()` … `ENDPROC`: the routine with the instructions.
4. An **instruction** with its **arguments**, separated by commas, ending in `;`. Arguments
   starting with `\` are *optional*: `\WObj:=wobj0` means "measured in work object
   `wobj0`". `tool0` and `wobj0` are built in: the bare flange and the robot base.

## Data: three kinds of storage

| Keyword | Meaning | Typical use |
|---|---|---|
| `CONST` | A fixed value; cannot change while the program runs. | Distances, sizes, fixed poses |
| `VAR` | A variable. Gets its declared value again when the module is loaded or the program pointer is reset to `main` (*PP to Main*). | Counters within one run, flags, intermediate results |
| `PERS` | Persistent. Changes are **kept**: written back into the module and remembered between runs. | Part counters, calibrated positions |

ABB's reference: [[abb-kernel:Variable declarations|variable]], [[abb-kernel:Persistent declarations|persistent]] and [[abb-kernel:Constant declarations|constant]] declarations.

!!! warning "Initial values must be constants"
    The value after `:=` in a declaration must be a **literal or a `CONST`**, never another
    `VAR`. `VAR jointtarget j := [home, …];` (where `home` is a `VAR`) is rejected
    when the module loads. That is exactly the [known Joint Target issue](../part-a/a3-motion/move.md#gotchas).

### Scope: who can see it

| Scope | Visible in | Robot Components |
|---|---|---|
| global (default) | The whole task: every module, every routine. | `GLOBAL` |
| `LOCAL` | Only the module it is declared in. | `LOCAL` |
| `TASK` | `PERS` only: the whole task, but **not shared** with other tasks. (A global `PERS` with the same name in two tasks is one shared value.) | `TASK` |

Data declared *inside* a routine exists only while that routine runs. Pick the scope with
[[Predefined Scopes]] on [[Routine]], [[RAPID Variable]] and the [[RAPID Generator]].

## The data types you'll meet

| Type | Holds | Example |
|---|---|---|
| [[abb:num]] | a number (integer or decimal) | `42`, `3.5` |
| [[abb:bool]] | `TRUE` or `FALSE` | `TRUE` |
| [[abb:string]] | text, up to 80 characters | `"hello"` |
| [[abb:robjoint]] | the 6 robot joint angles, degrees | `[0, 0, 0, 0, 30, 0]` |
| [[abb:jointtarget]] | robot joints + external axes | `[[0,0,0,0,30,0], [9E9,9E9,9E9,9E9,9E9,9E9]]` |
| [[abb:robtarget]] | a TCP position: **trans**lation (mm), **rot**ation (quaternion), **robconf** (axis configuration), **extax** (external axes) | `[[400,0,400], [0,0,1,0], [0,0,0,0], [9E9,…]]` |
| [[abb:speeddata]] | TCP mm/s, orientation °/s, external linear mm/s, external rotational °/s | `v200` = `[200, 500, 5000, 1000]` |
| [[abb:zonedata]] | how close to pass a point; `fine` = stop exactly | `z10`, `fine` |
| [[abb:tooldata]] / [[abb:wobjdata]] / [[abb:loaddata]] | tool, work object, payload | `tool0`, `wobj0`, `load0` |

`9E9` (9 × 10⁹) means *"not used"*, e.g. for external axes the robot doesn't have.

The rotation `[0, 0, 1, 0]` is a quaternion (`q1` = w, then x, y, z): a 180° turn about
Y, so the tool's Z axis points straight down. Robot Components works these out from your
planes; you never type them.

## Expressions, assignments and functions

Part B's components write lines like these:

```
CONST num approach := 50;
PERS num partCount := 0;

    partCount := partCount + 1;                              ! assignment
    MoveJ Offs(pick, 0, 0, approach), v200, z10, tool0;      ! a function as an argument
    TPWrite "Parts picked: " \Num:=partCount;                ! text on the FlexPendant
```

- `:=` **assigns** a new value. (`=` alone *compares*: `IF a = 5 THEN`.)
- `Offs(pick, 0, 0, approach)` is a **function call**: it returns a new robtarget,
  `approach` mm above `pick`, without changing `pick`. See [[Offset Target]].
- Comparisons: `=`, `<>`, `<`, `>`, `<=`, `>=`; logic: [[abb:AND]], [[abb:OR]], [[abb:NOT]]. See
  [[Comparison Expression]].

??? example "Complete module (checked on a Virtual Controller)"
    ```
    MODULE PrimerDemo
        ! Data: a constant, a persistent and two variables
        CONST num approach := 50;
        PERS num partCount := 0;
        VAR bool gripperClosed := FALSE;
        VAR robtarget pick := [[400, 0, 400], [0, 0, 1, 0], [0, 0, 0, 0], [9E9, 9E9, 9E9, 9E9, 9E9, 9E9]];

        PROC main()
            MoveAbsJ [[0, 0, 0, 0, 30, 0], [9E9, 9E9, 9E9, 9E9, 9E9, 9E9]], v200, z10, tool0\WObj:=wobj0;
            MoveJ Offs(pick, 0, 0, approach), v200, z10, tool0\WObj:=wobj0;
            MoveL pick, v50, fine, tool0\WObj:=wobj0;
            gripperClosed := TRUE;
            partCount := partCount + 1;
            TPWrite "Parts picked: " \Num:=partCount;
            MoveL Offs(pick, 0, 0, approach), v200, fine, tool0\WObj:=wobj0;
        ENDPROC
    ENDMODULE
    ```
    **Checked on a Virtual Controller** (IRB 1200-5/0.9, RobotWare 6.10): runs with no
    warnings, cycle time **3.8 s**.

## Try it

- [ ] In the complete module above, which line would fail to load if `approach` were declared `VAR` instead of `CONST`? *(Trick question; read the warning box again.)*
- [ ] `partCount` is `PERS`. You run the program three times, each time after *PP to Main*. What is its value now? What would it be if it were `VAR`?
- [ ] Write the declaration of a `robtarget` named `place` 200 mm to the left of `pick` (−Y), same orientation.

??? success "Solutions"
    1. None. `approach` is only *used* inside `main`, never in another declaration's
       initial value, so `VAR` would also load. The rule only applies to values after `:=` in
       declarations.
    2. `3` as `PERS` (the value is kept between runs); `1` as `VAR` (*PP to Main* resets it
       to its declared `0` before each run).
    3. `VAR robtarget place := [[400, -200, 400], [0, 0, 1, 0], [0, 0, 0, 0], [9E9, 9E9, 9E9, 9E9, 9E9, 9E9]];`

## Next

[B1 Routines](index.md): write your own `PROC`, `FUNC` and `TRAP` with [[Routine]].
