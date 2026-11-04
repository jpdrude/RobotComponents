# A3 Motion

How the robot moves between targets: movement type, speed, zone, and the settings that shape the path.

| Components | What for |
|---|---|
| [[Move]] · [[Movement Types]] | Create one robot movement (MoveAbsJ, MoveL, MoveJ or MoveC) per target. |
| [[Predefined Speed Data]] · [[Speed Data]] | How fast the robot moves, from predefined values (v5 … v7000) or your own. |
| [[Predefined Zone Data]] · [[Zone Data]] | How closely the robot passes a target, from a stop exactly on the point (fine) to smooth rounded corners (z5 … z200). |
| [[Circle Path Mode]] · [[Circle Path Modes]] | Choose how the tool turns along circular (MoveC) movements. |
| [[Joint Configuration Control]] · [[Linear Configuration Control]] | Switch the controller's arm-configuration check on or off for MoveJ and MoveL. |
| [[Acceleration Set]] · [[Path Acceleration Limitation]] · [[Velocity Set]] | Slow down acceleration or cap the speed for all following movements. |
| [[Override Robot Tool]] | Switch to another tool for all following movements, e.g. after a tool change. |

## Chapter exercises

★ basic · ★★ combine · ★★★ open

1. ★ In the [[Move]] square, give **every** corner the zone `fine`. How does the motion change?

2. ★★ Drive the last edge of the square at `v50` and the others at `v200`.

??? success "Solutions"
    1. The robot stops exactly on each corner instead of rounding it, so the square has sharp corners and takes longer. Use `fine` where the robot must really be on the point.
    2. Like the zones in the Move page's Task 2: a [[gh:Panel]] with four lines `200`, `200`, `200`, `50` on *Speed Data*. Grasshopper matches one speed to each target, and numbers are turned into the nearest predefined speed.
