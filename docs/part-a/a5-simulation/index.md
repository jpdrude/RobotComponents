# A5 Simulation and checking

Preview and check programs in Rhino before they reach a controller: kinematics, path preview, timing, and checking every action.

Pages marked *reference tables only* already show the component's description and its inputs and outputs; the tutorial text follows.

| Components | Status |
|---|---|
| [[Check Actions]] | ✅ written |
| [[Check Variable Names]] | ✅ written |
| [[Forward Kinematics]] | ✅ written |
| [[Inverse Kinematics]] | ✅ written |
| [[Path Generator]] | ✅ written |
| [[Timed Path Generator]] | ✅ written |

## Chapter exercises

★ basic · ★★ combine · ★★★ open

1. ★ Add a [[Path Generator]] to the [pick and place](../a9-tutorials/pick-and-place.md) definition and scrub the slider. Where does the robot move fastest?

2. ★★ Compare the *Program Time* of the [[Path Generator]] and the [[Timed Path Generator]] for the [[Move]] square with the 6.2 s measured on the Virtual Controller.

??? success "Solutions"
    1. On the two [[abb:MoveJ]] moves across (`v200`, joint interpolation). The [[abb:MoveL]] moves near the parts run at `v50`.
    2. Both are estimates from path length and programmed speed (the Path Generator gave 5.8 s). The controller is slower because it accelerates, decelerates and rounds corners. Use the estimates to compare variants, not as exact cycle times.
