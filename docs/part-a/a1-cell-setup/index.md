# A1 Cell setup

Everything that describes the physical cell: the robot, the tool on its flange, the work objects you program against, payloads and external axes. On the controller, the robot and every external axis is a mechanical unit ([[abb:mecunit]]).

| Components | What for |
|---|---|
| [[Robot Preset]] | Pick an ABB robot model from a list and place it in your scene. |
| [[Robot]] | Define your own robot from meshes, kinematic parameters and joint limits. |
| [[Get Axis Planes from Robot Kinematics Parameters]] · [[Robot Kinematic Parameters]] | The eight dimensions that describe a six-axis robot's geometry for the kinematics solver. |
| [[Robot Tool]] | Describe the tool on the robot's flange (gripper, pen, spindle) so the robot moves its tip, not its flange. |
| [[Work Object]] | Define a coordinate system on your table or part, and program the robot relative to it. |
| [[Load Data]] | Mass and centre of gravity of a tool or payload, so the controller can drive the motors correctly. |
| [[Robot Tool Calibration]] | (Experimental) compute a tool's TCP from several robot poses that touch the same point. |
| [[External Linear Axis]] | A track that moves the robot (or a part) in a straight line. |
| [[External Rotational Axis]] | A turntable or positioner that rotates the part (or the robot). |

## Chapter exercises

★ basic · ★★ combine · ★★★ open

1. ★ Give the [first program](../../getting-started/index.md) a pen: a [[Robot Tool]] whose tip is 120 mm along the flange's Z axis. Which parts of the RAPID change?

2. ★★ In the [[Work Object]] example, move the table 100 mm further in X. Which numbers in the RAPID change?

??? success "Solutions"
    1. Every Move line now says `pen` instead of `tool0`, and (with right-click → *Add Tool Data* on the [[RAPID Generator]]) a `PERS tooldata pen := [TRUE, [[0, 0, 120], …]` declaration appears. The targets stay the same: they describe where the **pen tip** goes, so the flange now stops 120 mm higher.
    2. Only the object frame inside `wobjdata table` (`[[450, -150, 150], …]`). The four robtargets stay `[0, 0, 0]`, `[100, 0, 0]`, … because they are relative to the table, which is the point of a work object.
