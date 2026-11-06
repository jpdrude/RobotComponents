# A2 Targets

Where the robot should go: positions and orientations from planes, joint angles, external axis positions and the axis configuration the robot should use. In RAPID these are [[abb:robtarget]] and [[abb:jointtarget]], with [[abb:confdata]] and [[abb:extjoint]] inside.

| Components | What for |
|---|---|
| [[Robot Target]] | Turn a plane into a position and orientation the robot's tool should reach. |
| [[Robot Joint Position]] | Six joint angles in degrees: a robot pose given by the joints instead of the tool position. |
| [[Joint Target]] | Combine robot joint angles and external axis positions into one joint target. |
| [[External Joint Position]] | Where the external axes (track, turntable) should be at a target. |
| [[Configuration Data]] · [[Configuration Number]] | Tell the robot which of its possible arm poses to use for a target. |
| [[Flip Plane X]] · [[Flip Plane Y]] | Turn a plane upside down so the tool points the other way. |
| [[Plane to Quaternion]] · [[Quaternion to Plane]] | Convert between Rhino planes and the position + quaternion numbers used by ABB robots. |
| [[Plane Visualizer]] | Draw a plane's axes as coloured arrows in Rhino to check orientations. |

## Chapter exercises

★ basic · ★★ combine · ★★★ open

1. ★ In the first program, replace [[Flip Plane X]] by [[Flip Plane Y]]. Does the tool still point down? What changes in the robtarget?

2. ★★ Use [[Inverse Kinematics]] to turn the first program's target into joint angles, and use those as the start pose of a [[abb:MoveAbsJ]].

??? success "Solutions"
    1. Yes, the Z axis still points down. Only the orientation changes: Flip Plane Y turns the plane 180° about its X axis instead of its Y axis, so the quaternion (second block of the robtarget) is different and the wrist is turned by 180° around the tool axis.
    2. Wire the IK's *Robot Joint Position* output into a [[Move]] with type `MoveAbsJ`. For the target (400, 0, 400) the joint angles are `[0, 14.63, 41.4, 0, 33.97, 0]`; see [[Inverse Kinematics]].
