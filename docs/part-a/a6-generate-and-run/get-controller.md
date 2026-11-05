---
title: Get Controller and Get Log
description: Connect Grasshopper to a real or virtual ABB controller, and read its event log.
---

# ![](../../assets/icons/rc/get-controller.png){ .rc-icon--title } Get Controller and Get Log

<span class="rc-badge">Controller Utility tab</span>

**Get Controller** is the start of every connection: it finds the ABB controllers on your
network or PC (real robots and RobotStudio's virtual controllers) and lets you pick one. Its
*Controller* output feeds all other Controller Utility components. **Get Log** reads the
controller's event log, the same messages the FlexPendant shows. That's useful when an upload
or a run fails.

### Get Controller
[[io:Get Controller]]

### Get Log
[[io:Get Log]]

!!! info "Requirements"
    Windows, and the ABB PC SDK runtime (installed with RobotStudio, or the ABB Robot
    Communication Runtime). See [Getting started → Install](../../getting-started/index.md#install).
    For a real robot, your PC must be on the controller's network and allowed to connect.

## See also

[[Upload Program]] · [[Run Program]] · [Controller signals](controller-signals.md) · [Controller poses](controller-poses.md)
