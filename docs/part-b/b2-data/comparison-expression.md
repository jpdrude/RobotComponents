---
title: Comparison Expression
description: Build a true/false condition such as count >= 100 for IF statements and loops.
---

# ![](../../assets/icons/rc/comparison-expression.png){ .rc-icon--title } Comparison Expression

<span class="rc-badge">Advanced RAPID Features tab</span>
<span class="rc-badge rc-badge--fork">Fork addition</span>
<span class="rc-badge rc-badge--verified">RAPID checked on IRB 1200 · RobotWare 6.10</span>

**Comparison Expression** compares two values and gives a condition that is either true
or false, like `count >= 100`. Wire it into the condition of an [[IF Statement]] or a
[[While Loop]].

[[io:Comparison Expression]]

When placed, it creates a [[Comparison Operators]] drop-down with the six RAPID operators.
*A* and *B* take numbers, a [[RAPID Variable]]'s *Variable* output, or any expression.

## In RAPID

| Operator | Means | Example |
|---|---|---|
| `<` | less than | `count < 10` |
| `>` | greater than | `force > 50` |
| `<=` | less than or equal | `i <= 3` |
| `>=` | greater than or equal | `count >= 100` |
| `=` | equal | `mode = 2` |
| `<>` | not equal | `answer <> 1` |

The result has the RAPID type [[abb:bool]]. To combine conditions, type the expression into a
[[gh:Panel]] instead and wire it straight into the condition input:
`count >= 100 AND NOT finished`. ABB's reference: [RAPID kernel][abb-kernel] ([[abb-kernel:Expressions|expressions]], [[abb-kernel:Conditional expressions|conditional expressions]]).

From the [row tutorial](../b8-tutorials/row-loop.md): A = the `count` variable,
Operator `>=`, B = `100` gives

```
IF count >= 100 THEN
```

## Try it

- [ ] Change the reset rule so the counter resets when it reaches exactly 40.

??? success "Solution"
    Operator `=` and B = `40` → `IF count = 40 THEN`. (`>= 40` is safer: it also resets if the
    count ever jumps past 40.)

## See also

[[IF Statement]] · [[While Loop]] · [[Comparison Operators]] · [[type:RAPID Expression]]
