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

!!! bug "Use *Value* only for a single call"
    *Value* is the value for the Routine's own *Routine Call* output. A **list** of values
    duplicates the routine (see [[Routine]] → Gotchas). For several calls, leave *Value* empty and
    use a [[Routine Call]] component.

!!! bug "Robot Targets as values don't work yet"
    A Robot Target on *Value* is written as `Robot Target (name)` instead of its RAPID name.
    Pass numbers or text instead.

## See also

[[Routine]] · [[Routine Call]] · [[RAPID Variable]]
