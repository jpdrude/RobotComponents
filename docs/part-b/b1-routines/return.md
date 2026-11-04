---
title: Return
description: End a routine, and hand a value back from a function (RETURN).
---

# ![](../../assets/icons/rc/return.png){ .rc-icon--title } Return

<span class="rc-badge">Advanced RAPID Features tab</span>
<span class="rc-badge rc-badge--fork">Fork addition</span>
<span class="rc-badge rc-badge--verified">RAPID checked on IRB 1200 · RobotWare 6.10</span>

**Return** ends the routine it is in. In a `FUNC` ([[Routine]] with Type `FUNC`) it also
hands back the function's result: wire the value into *Return Value*. Without a value it
writes a plain `RETURN;`, which leaves a `PROC` or `TRAP` early, typically inside an
[[IF Statement]].

[[io:Return]]

## In RAPID

```
FUNC num currentZ()
    VAR robtarget now;
    now := CRobT();
    RETURN now.trans.z;
ENDFUNC
```

A `FUNC` must reach a `RETURN` with a value of its declared type (`num` here). The
caller then uses the call like a value: `z := currentZ();`. ABB's reference:
[RAPID Instructions, Functions and Data types][abb-ifd] ([[abb:RETURN]]).

Full example: [[Current Robot Target]].

## See also

[[Routine]] · [[Routine Call]] · [[IF Statement]]
