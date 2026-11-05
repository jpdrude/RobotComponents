---
title: While Loop
description: Repeat actions as long as a condition is true (WHILE … ENDWHILE).
---

# ![](../../assets/icons/rc/while-loop.png){ .rc-icon--title } While Loop

<span class="rc-badge">Advanced RAPID Features tab</span>
<span class="rc-badge rc-badge--fork">Fork addition</span>
<span class="rc-badge rc-badge--verified">RAPID syntax checked on IRB 1200 · RobotWare 6.10</span>

**While Loop** repeats its *Actions* for as long as *Condition* is true. It is checked
**before** each pass. Use it when you don't know in advance how often to repeat: until the
operator says stop, until a counter reaches a limit, until a sensor reports a part. To repeat
a known number of times, a [[For Loop]] is simpler.

[[io:While Loop]]

*Condition* takes a [[Comparison Expression]], a [[abb:bool]] [[RAPID Variable]], or an
expression typed into a [[gh:Panel]] (`count < 10 AND NOT stop`).

## In RAPID

```
WHILE again DO
    ...
ENDWHILE
```

Something inside the loop must eventually make the condition false (here: the operator
pressing *No* sets `again := FALSE`). Otherwise the program never leaves the loop. ABB's
reference: [RAPID Instructions, Functions and Data types][abb-ifd] ([[abb:WHILE]]).

Full example: [[Message Box]] (operator-driven loop).

## Gotchas

!!! warning "One loop per data-tree branch"
    Actions on different data-tree branches give **one loop per branch**. All Robot Components
    actions come out on the branch they were given, so this only happens when your own data
    is split, e.g. Moves made from [[gh:Discontinuity]] points. Flatten that data, or the
    loop's *Actions* input.

## See also

[[For Loop]] · [[IF Statement]] · [[Comparison Expression]]
