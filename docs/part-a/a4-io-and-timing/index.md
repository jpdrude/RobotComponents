# A4 I/O and timing

Switching outputs, waiting for signals and waiting for time: how the robot program talks to grippers, sensors and other machines.

| Components | What for |
|---|---|
| [[Invert Digital Output]] · [[Pulse Digital Output]] · [[Set Analog Output]] · [[Set Digital Output]] · [[Set Group Output]] | Switch the robot's output signals (gripper, lamp, valve) from the program. |
| [[Wait for Analog Input]] · [[Wait for Analog Output]] · [[Wait for Digital Input]] · [[Wait for Digital Output]] · [[Wait for Group Input]] · [[Wait for Group Output]] | Pause the program until an input or output signal has a certain value. |
| [[Wait Time]] | Pause the program for a number of seconds. |
| [[Wait for Robot]] | Pause the program until the robot has finished its movement. |

## Chapter exercises

★ basic · ★★ combine · ★★★ open

1. ★ In [pick and place](../a9-tutorials/pick-and-place.md) the gripper gets 0.5 s to close. Make it 1 s, but only for closing.

2. ★★ Close the gripper **exactly** when the robot arrives at `pick`, without a separate Set Digital Output line.

??? success "Solutions"
    1. Give the first [[Wait Time]] (after `SetDO doGripper, 1;`) a *Duration* of `1`, and keep the second one at `0.5`.
    2. Right-click the Move to `pick` → *Set Digital Output*, and wire a [[Set Digital Output]] (`doGripper`, `1`) into it. The Move becomes a [[abb:MoveLDO]], which switches the output at the target.
