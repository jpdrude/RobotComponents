---
title: Load Module
description: Load and unload extra RAPID modules while the program runs, synchronously or in the background.
---

# ![](../../assets/icons/rc/load-module.png){ .rc-icon--title } Load Module

<span class="rc-badge">Advanced RAPID Features tab</span>
<span class="rc-badge rc-badge--verified">RAPID syntax checked on IRB 1200 · RobotWare 6.10</span>

**Load Module** writes the lines that load another RAPID module (a `.MOD` file on the
controller) into the running task, and unload it again. Use it for libraries of
[[Routine]]s you only need sometimes, or for big programs split into parts. The module files
get onto the controller with [[Upload Helper Modules]].

[[io:Load Module]]

## In RAPID

With *Module Name* `Helpers` and *Load Session Name* `loadHelpers`:

```
LOAD \Dynamic, "HOME:/Robot Components/Additional Modules/" \FILE:="Helpers.MOD";   ! Load Module
UNLOAD "HOME:/Robot Components/Additional Modules/Helpers.MOD";                      ! Unload Module

VAR loadsession loadHelpers;                                                          ! Load Session Declaration
STARTLOAD \Dynamic, "HOME:/Robot Components/Additional Modules/" \FILE:="Helpers.MOD", loadHelpers;   ! Start Load Module
WAITLOAD loadHelpers;                                                                 ! Wait Load Module
```

- **`LOAD`** loads and waits until it's done.
- **`STARTLOAD` … `WAITLOAD`** loads in the background: the robot keeps working (e.g. moves)
  between the two lines, which saves cycle time. The *Load Session Declaration* goes among
  the declarations.
- The modules are expected in `HOME:/Robot Components/Additional Modules/` on the controller.

Routines in a loaded module are called with [[Routine Call]]. Their names are only known
once the module is loaded. ABB's reference: [RAPID Instructions, Functions and Data types][abb-ifd]
([[abb:Load]], [[abb:UnLoad]], [[abb:StartLoad]], [[abb:WaitLoad]]).

## See also

[[Upload Helper Modules]] · [[Routine Call]] · [[Routine]]
