# A2 Targets

Where the robot should go: positions and orientations from planes, joint angles, external axis positions and the axis configuration the robot should use.

Pages marked *reference tables only* already show the component's description and its inputs and outputs; the tutorial text follows.

| Components | Status |
|---|---|
| [[Configuration Data]] | ✅ written |
| [[External Joint Position]] | ✅ written |
| [[Flip Plane X]] · [[Flip Plane Y]] | ✅ written |
| [[Joint Target]] | ✅ written |
| [[Plane to Quaternion]] · [[Quaternion to Plane]] | ✅ written |
| [[Plane Visualizer]] | ✅ written |
| [[Robot Joint Position]] | ✅ written |
| [[Robot Target]] | ✅ written |

## Chapter exercises

★ basic · ★★ combine · ★★★ open

1. ★ In the first program, replace [[Flip Plane X]] by [[Flip Plane Y]]. Does the tool still point down? What changes in the robtarget?

2. ★★ Use [[Inverse Kinematics]] to turn the first program's target into joint angles, and use those as the start pose of a `MoveAbsJ`.

??? success "Solutions"
    1. Yes, the Z axis still points down. Only the orientation changes: Flip Plane Y turns the plane 180° about its X axis instead of its Y axis, so the quaternion (second block of the robtarget) is different and the wrist is turned by 180° around the tool axis.
    2. Wire the IK's *Robot Joint Position* output into a [[Move]] with type `MoveAbsJ`. For the target (400, 0, 400) the joint angles are `[0, 14.63, 41.4, 0, 33.97, 0]`; see [[Inverse Kinematics]].
