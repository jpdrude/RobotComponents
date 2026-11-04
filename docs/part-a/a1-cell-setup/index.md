# A1 Cell setup

Everything that describes the physical cell: the robot, the tool on its flange, the work objects you program against, payloads and external axes. On the controller, the robot and every external axis is a mechanical unit ([[abb:mecunit]]).

Pages marked *reference tables only* already show the component's description and its inputs and outputs; the tutorial text follows.

| Components | Status |
|---|---|
| [[External Linear Axis]] | ✅ written |
| [[External Rotational Axis]] | ✅ written |
| [[Load Data]] | ✅ written |
| [[Get Axis Planes from Robot Kinematics Parameters]] · [[Robot Kinematic Parameters]] | ✅ written |
| [[Robot Preset]] | ✅ written |
| [[Robot Tool Calibration]] | ✅ written |
| [[Robot Tool]] | ✅ written |
| [[Robot]] | ✅ written |
| [[Work Object]] | ✅ written |

## Chapter exercises

★ basic · ★★ combine · ★★★ open

1. ★ Give the [first program](../../getting-started/index.md) a pen: a [[Robot Tool]] whose tip is 120 mm along the flange's Z axis. Which parts of the RAPID change?

2. ★★ In the [[Work Object]] example, move the table 100 mm further in X. Which numbers in the RAPID change?

??? success "Solutions"
    1. Every Move line now says `pen` instead of `tool0`, and (with right-click → *Add Tool Data* on the [[RAPID Generator]]) a `PERS tooldata pen := [TRUE, [[0, 0, 120], …]` declaration appears. The targets stay the same: they describe where the **pen tip** goes, so the flange now stops 120 mm higher.
    2. Only the user frame inside `wobjdata table` (`[[450, -150, 150], …]`). The four robtargets stay `[0, 0, 0]`, `[100, 0, 0]`, … because they are relative to the table, which is the point of a work object.
