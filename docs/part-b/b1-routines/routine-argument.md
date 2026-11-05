---
title: Routine Argument
description: Define a parameter of your own routine, and use it inside the routine body.
---

# ![](../../assets/icons/rc/routine-argument.png){ .rc-icon--title } Routine Argument

<span class="rc-badge">Advanced RAPID Features tab</span>
<span class="rc-badge rc-badge--fork">Fork addition</span>
<span class="rc-badge rc-badge--verified">RAPID checked on IRB 1200 · RobotWare 6.10</span>

**Routine Argument** defines one parameter of a [[Routine]]: its type and name, like `num y`
in `PROC visit(num y)`. Wire **Argument** into one of the Routine's **+** inputs. Wire
**Variable** into the components *inside* the routine that should use the value (an
[[Offset Target]], an [[Assign Variable Value]], a [[Comparison Expression]], …).

[[io:Routine Argument]]

## In RAPID

| Keyword | Writes | The routine gets |
|---|---|---|
| *(none)* | `PROC visit(num y)` | a copy of the value; changing `y` inside doesn't affect the caller |
| `INOUT` | `PROC visit(INOUT num y)` | the caller's variable itself; changes are visible to the caller |
| `PERS` | `PROC visit(PERS num y)` | a persistent passed by reference |

ABB's reference: [RAPID kernel][abb-kernel] ([[abb-kernel:Parameter declarations|parameter declarations]]).

Full example: [[Routine]].

## Gotchas

!!! warning "A list on *Value* makes one argument per value"
    Like any Grasshopper component, Routine Argument runs once per value: a list gives one
    argument (and one *Variable*) per value, so the [[Routine]] is built once per value too
    (see [[Routine]] → Gotchas). *Value* is meant for the Routine's own single call. For several
    calls, leave *Value* empty and use a [[Routine Call]] component.

!!! info "Robot Targets as values"
    A Robot Target (or Joint Target, joint position, Configuration Data) on *Value* is written
    as its value, e.g. `visit [[400, -100, 300], [0, 0, 1, 0], [0, 0, 0, 0], [9E9, …]];`, so
    it needs no declaration. An `INOUT` or `PERS` parameter needs a variable instead: pass a
    [[RAPID Variable]].

## See also

[[Routine]] · [[Routine Call]] · [[RAPID Variable]]
