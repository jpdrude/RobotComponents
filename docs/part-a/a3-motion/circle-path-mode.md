---
title: Circle Path Mode
description: Choose how the tool turns along circular (MoveC) movements.
---

# ![](../../assets/icons/rc/circle-path-mode.png){ .rc-icon--title } Circle Path Mode

<span class="rc-badge">Code Generation tab</span>
<span class="rc-badge rc-badge--verified">RAPID syntax checked on IRB 1200 · RobotWare 6.10</span>

A `MoveC` moves the tool tip along an arc. **Circle Path Mode** decides how the tool's
**orientation** changes along that arc. It applies to **all following** `MoveC` movements, so
place it in *Actions* before them. Pick the mode from the [[Circle Path Modes]] drop-down.

[[io:Circle Path Mode]]

| Mode | The tool… |
|---|---|
| `PathFrame` (default) | turns with the path, like a car following a curve |
| `ObjectFrame` | keeps its orientation relative to the work object; e.g. a tool that should always point the same way on the part |
| `CirPointOri` | passes through the orientation programmed at the circle point |
| `Wrist45` / `Wrist46` / `Wrist56` | only the two named wrist axes reorient, e.g. for cutting tools whose Z axis must follow the circle |

## In RAPID

```
CirPathMode \ObjectFrame;
```

ABB's reference: [RAPID Instructions, Functions and Data types][abb-ifd] (`CirPathMode`, `MoveC`).

## See also

[[Move]] (type `MoveC`, right-click → *Set Circular Point*) · [[Circle Path Modes]]
