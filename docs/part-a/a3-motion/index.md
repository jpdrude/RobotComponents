# A3 Motion

How the robot moves between targets: movement type, speed, zone, and the settings that shape the path.

Pages marked *reference tables only* already show the component's description and its inputs and outputs; the tutorial text follows.

| Components | Status |
|---|---|
| [[Acceleration Set]] · [[Path Acceleration Limitation]] · [[Velocity Set]] | ✅ written |
| [[Circle Path Mode]] · [[Circle Path Modes]] | ✅ written |
| [[Joint Configuration Control]] · [[Linear Configuration Control]] | ✅ written |
| [[Move]] · [[Movement Types]] | ✅ written |
| [[Override Robot Tool]] | ✅ written |
| [[Predefined Speed Data]] · [[Speed Data]] | ✅ written |
| [[Predefined Zone Data]] · [[Zone Data]] | ✅ written |

## Chapter exercises

★ basic · ★★ combine · ★★★ open

1. ★ In the [[Move]] square, give **every** corner the zone `fine`. How does the motion change?

2. ★★ Drive the last edge of the square at `v50` and the others at `v200`.

??? success "Solutions"
    1. The robot stops exactly on each corner instead of rounding it, so the square has sharp corners and takes longer. Use `fine` where the robot must really be on the point.
    2. Like the zones in the Move page's Task 2: a [[gh:Panel]] with four lines `200`, `200`, `200`, `50` on *Speed Data*. Grasshopper matches one speed to each target, and numbers are turned into the nearest predefined speed.
