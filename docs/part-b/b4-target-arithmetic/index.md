# B4 Target arithmetic

Compute targets on the controller: offsets, the robot's current position, and the parts of a robtarget.

| Components | What for |
|---|---|
| [[Offset Target]] | Shift a Robot Target by X, Y, Z millimetres on the controller with RAPID's Offs function. |
| [[Current Robot Target]] | Read where the robot actually is while the program runs (RAPID's CRobT function). |
| [[Get Robtarget Members]] | Read single numbers out of a robtarget variable (position, orientation, configuration, external axes). |

Tutorial: [a row of points with a loop](../b8-tutorials/row-loop.md) uses [[Offset Target]] with a loop counter.
