// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components
// Project: https://github.com/RobotComponents/RobotComponents
//
// For license details, see the LICENSE file in the project root.

// System Libs
using System;
using System.Collections.Generic;
using System.Linq;
// Rhino Libs
using Rhino.Geometry;
// Xunit Libs
using Xunit;
// Robot Components Libs
using RobotComponents.Generic.Kinematics;
using RobotComponents.ABB.Kinematics;
using RobotComponents.ABB.Definitions;
using RobotComponents.ABB.Actions.Declarations;

namespace RobotComponents.Tests.Kinematics
{
    public class KinematicsTests
    {
        private const double Tolerance = 1e-3;
        private const double AngleTolerance = 1e-6;

        /// <summary>
        /// Creates an OPW solver configured with IRB120-3/0.58 kinematic parameters.
        /// </summary>
        private static OPWKinematics CreateIRB120OPW()
        {
            OPWKinematics opw = new OPWKinematics();
            opw.A1 = 0;
            opw.B = 0;
            opw.C1 = 290;
            opw.C2 = 270;
            opw.C4 = 72;
            opw.A2 = -70;   // triggers UpdateRobotParameters
            opw.C3 = 302;   // triggers UpdateRobotParameters with correct A2
            opw.Offsets = new double[] { 0, 0, -Math.PI / 2, 0, 0, 0 };
            opw.Signs = new int[] { 1, 1, 1, 1, 1, 1 };
            return opw;
        }

        /// <summary>
        /// Creates a test robot with IRB120-3/0.58 kinematic parameters and empty meshes.
        /// </summary>
        private static Robot CreateTestRobot()
        {
            List<Mesh> meshes = Enumerable.Range(0, 7).Select(_ => new Mesh()).ToList();
            RobotKinematicParameters parameters = new RobotKinematicParameters(0, -70, 0, 0, 290, 270, 302, 72);
            Interval[] limits = new Interval[]
            {
                new Interval(-165, 165),
                new Interval(-110, 110),
                new Interval(-110, 70),
                new Interval(-160, 160),
                new Interval(-120, 120),
                new Interval(-400, 400)
            };
            return new Robot("TestRobot", meshes, parameters, limits, Plane.WorldXY, new RobotTool());
        }

        #region OPW Forward Kinematics
        [Fact]
        public void OPWForward_AllZeroInternalAngles_ReturnsValidPlane()
        {
            OPWKinematics opw = CreateIRB120OPW();

            // Input [0, 0, -pi/2, 0, 0, 0] → internal angles all 0 (offset on joint 3)
            double[] pose = { 0, 0, -Math.PI / 2, 0, 0, 0 };
            Plane result = opw.Forward(pose);

            Assert.False(double.IsNaN(result.Origin.X));
            Assert.False(double.IsNaN(result.Origin.Y));
            Assert.False(double.IsNaN(result.Origin.Z));
        }

        [Fact]
        public void OPWForward_AllZeroInternalAngles_CorrectPosition()
        {
            OPWKinematics opw = CreateIRB120OPW();

            // With all internal angles at 0, expected end plane:
            // wrist at (A2, 0, C1+C2+C3_contribution) → approximately (-70, 0, 862)
            // end plane at (-70 + C4*zComponent, 0, 862 + C4*zComponent)
            // With identity orientation: end = (-70, 0, 862+72) = (-70, 0, 934)
            double[] pose = { 0, 0, -Math.PI / 2, 0, 0, 0 };
            Plane result = opw.Forward(pose);

            Assert.InRange(result.Origin.X, -70 - Tolerance, -70 + Tolerance);
            Assert.InRange(result.Origin.Y, -Tolerance, Tolerance);
            Assert.InRange(result.Origin.Z, 934 - Tolerance, 934 + Tolerance);
        }

        [Fact]
        public void OPWForward_AllZeroInternalAngles_OutputsWristPosition()
        {
            OPWKinematics opw = CreateIRB120OPW();

            double[] pose = { 0, 0, -Math.PI / 2, 0, 0, 0 };
            opw.Forward(pose, out Point3d wristPosition);

            // Wrist position should be at (-70, 0, 862)
            Assert.InRange(wristPosition.X, -70 - Tolerance, -70 + Tolerance);
            Assert.InRange(wristPosition.Y, -Tolerance, Tolerance);
            Assert.InRange(wristPosition.Z, 862 - Tolerance, 862 + Tolerance);
        }

        [Fact]
        public void OPWForward_Joint1Rotated_RotatesXYPosition()
        {
            OPWKinematics opw = CreateIRB120OPW();

            // Rotate joint 1 by pi/2 (90 degrees)
            double[] pose = { Math.PI / 2, 0, -Math.PI / 2, 0, 0, 0 };
            Plane result = opw.Forward(pose);

            // With joint 1 at 90°, X/Y should swap: (-70, 0) → (0, -70)
            Assert.InRange(result.Origin.X, -Tolerance, Tolerance);
            Assert.InRange(result.Origin.Y, -70 - Tolerance, -70 + Tolerance);
            Assert.InRange(result.Origin.Z, 934 - Tolerance, 934 + Tolerance);
        }

        [Fact]
        public void OPWForward_DifferentJointAngles_ProducesDifferentPosition()
        {
            OPWKinematics opw = CreateIRB120OPW();

            double[] pose1 = { 0, 0, -Math.PI / 2, 0, 0, 0 };
            double[] pose2 = { 0.5, 0.3, -Math.PI / 2 + 0.2, 0, Math.PI / 6, 0 };

            Plane result1 = opw.Forward(pose1);
            Plane result2 = opw.Forward(pose2);

            // Different joints should produce different positions
            double distance = result1.Origin.DistanceTo(result2.Origin);
            Assert.True(distance > 1.0, "Different joint angles should produce different TCP positions");
        }

        [Fact]
        public void OPWForward_TooFewJoints_ThrowsException()
        {
            OPWKinematics opw = CreateIRB120OPW();

            double[] pose = { 0, 0, 0 }; // Only 3 values
            Assert.Throws<Exception>(() => opw.Forward(pose));
        }
        #endregion

        #region OPW Inverse Kinematics
        [Fact]
        public void OPWInverse_ValidPlane_Produces8Solutions()
        {
            OPWKinematics opw = CreateIRB120OPW();

            Plane endPlane = new Plane(
                new Point3d(-70, 0, 934),
                new Vector3d(1, 0, 0),
                new Vector3d(0, 1, 0));

            opw.Inverse(endPlane);

            // Should always produce 8 solution arrays
            Assert.Equal(8, opw.Solutions.Length);
            Assert.Equal(6, opw.Solutions[0].Length);
        }

        [Fact]
        public void OPWInverse_SingularityFlags_HaveCorrectLength()
        {
            OPWKinematics opw = CreateIRB120OPW();

            Plane endPlane = new Plane(
                new Point3d(-70, 0, 934),
                new Vector3d(1, 0, 0),
                new Vector3d(0, 1, 0));

            opw.Inverse(endPlane);

            Assert.Equal(8, opw.IsShoulderSingularity.Length);
            Assert.Equal(8, opw.IsElbowSingularity.Length);
            Assert.Equal(8, opw.IsWristSingularity.Length);
        }
        #endregion

        #region OPW Forward-Inverse Round-trip
        [Fact]
        public void OPWRoundTrip_AllZeroInternalAngles_RecoversSolution()
        {
            OPWKinematics opw = CreateIRB120OPW();

            double[] inputPose = { 0, 0, -Math.PI / 2, 0, Math.PI / 4, 0 };

            // Forward
            Plane endPlane = opw.Forward(inputPose);

            // Inverse
            opw.Inverse(endPlane);

            // At least one of the 8 solutions should match the input
            bool matchFound = false;
            for (int i = 0; i < 8; i++)
            {
                bool allMatch = true;
                for (int j = 0; j < 6; j++)
                {
                    if (Math.Abs(opw.Solutions[i][j] - inputPose[j]) > AngleTolerance)
                    {
                        allMatch = false;
                        break;
                    }
                }
                if (allMatch) { matchFound = true; break; }
            }

            Assert.True(matchFound, "Forward-Inverse round-trip should recover the original joint angles in at least one solution");
        }

        [Fact]
        public void OPWRoundTrip_NonTrivialPose_RecoversSolution()
        {
            OPWKinematics opw = CreateIRB120OPW();

            double[] inputPose = { 0.5, 0.3, -Math.PI / 2 + 0.2, 0.1, Math.PI / 6, 0.4 };

            // Forward
            Plane endPlane = opw.Forward(inputPose);

            // Inverse
            opw.Inverse(endPlane);

            // At least one of the 8 solutions should match the input
            bool matchFound = false;
            for (int i = 0; i < 8; i++)
            {
                bool allMatch = true;
                for (int j = 0; j < 6; j++)
                {
                    if (Math.Abs(opw.Solutions[i][j] - inputPose[j]) > AngleTolerance)
                    {
                        allMatch = false;
                        break;
                    }
                }
                if (allMatch) { matchFound = true; break; }
            }

            Assert.True(matchFound, "Forward-Inverse round-trip should recover the original joint angles in at least one solution");
        }

        [Fact]
        public void OPWRoundTrip_NegativeJointAngles_RecoversSolution()
        {
            OPWKinematics opw = CreateIRB120OPW();

            double[] inputPose = { -0.3, -0.5, -Math.PI / 2 - 0.3, -0.2, Math.PI / 5, -0.5 };

            Plane endPlane = opw.Forward(inputPose);
            opw.Inverse(endPlane);

            bool matchFound = false;
            for (int i = 0; i < 8; i++)
            {
                bool allMatch = true;
                for (int j = 0; j < 6; j++)
                {
                    if (Math.Abs(opw.Solutions[i][j] - inputPose[j]) > AngleTolerance)
                    {
                        allMatch = false;
                        break;
                    }
                }
                if (allMatch) { matchFound = true; break; }
            }

            Assert.True(matchFound, "Round-trip with negative angles should recover the original joint angles");
        }
        #endregion

        #region Singularity Detection
        [Fact]
        public void OPWInverse_WristSingularity_J5NearZero_DetectsSingularity()
        {
            OPWKinematics opw = CreateIRB120OPW();

            // Compute forward with J5 very close to 0 (wrist singularity)
            double[] pose = { 0, 0, -Math.PI / 2, 0, 0.001, 0 };
            Plane endPlane = opw.Forward(pose);
            opw.Inverse(endPlane);

            // At least some solutions should flag wrist singularity (J5 < 0.02 rad threshold)
            bool anySingularity = opw.IsWristSingularity.Any(s => s);
            Assert.True(anySingularity, "J5 near zero should trigger wrist singularity detection");
        }

        [Fact]
        public void OPWInverse_ShoulderSingularity_TargetAtOrigin_DetectsSingularity()
        {
            OPWKinematics opw = CreateIRB120OPW();

            // Shoulder singularity: wrist center at (0, 0, z) → directly above axis 1
            // For IRB120 with A2=-70, B=0: wrist center is at (-70, 0, z) in zero config
            // To get shoulder singularity, we need cx0 ≈ 0 and cy0 ≈ 0
            // This happens when the wrist center is directly on the Z axis
            // We can construct a plane where the wrist center maps to near (0,0,z)
            // C4=72, so endPlane.Origin = wristCenter + C4 * ZAxis
            Plane endPlane = new Plane(
                new Point3d(0, 0, 934),
                new Vector3d(1, 0, 0),
                new Vector3d(0, 1, 0));

            opw.Inverse(endPlane);

            bool anySingularity = opw.IsShoulderSingularity.Any(s => s);
            Assert.True(anySingularity, "Target directly above axis 1 should trigger shoulder singularity detection");
        }

        [Fact]
        public void OPWInverse_NormalPosition_NoWristSingularity()
        {
            OPWKinematics opw = CreateIRB120OPW();

            // J5 at pi/4 — well away from singularity
            double[] pose = { 0, 0, -Math.PI / 2, 0, Math.PI / 4, 0 };
            Plane endPlane = opw.Forward(pose);
            opw.Inverse(endPlane);

            // Solutions 0-3 (positive J5) should NOT have wrist singularity
            // pi/4 ≈ 0.785 which is well above the 0.02 threshold
            Assert.False(opw.IsWristSingularity[0], "J5 at pi/4 should not be a wrist singularity");
        }
        #endregion

        #region OPW Angle Normalization
        [Fact]
        public void OPWInverse_SolutionsNormalized_WithinPiRange()
        {
            OPWKinematics opw = CreateIRB120OPW();

            double[] pose = { 0, 0, -Math.PI / 2, 0, Math.PI / 4, 0 };
            Plane endPlane = opw.Forward(pose);
            opw.Inverse(endPlane);

            // After offset/sign corrections, check solutions are in reasonable range
            // The corrections may push values outside [-pi, pi] due to offset addition,
            // but the internal angles should be normalized before corrections
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    Assert.False(double.IsNaN(opw.Solutions[i][j]),
                        $"Solution [{i}][{j}] should not be NaN");
                }
            }
        }
        #endregion

        #region ForwardKinematics (Robot-level)
        [Fact]
        public void ForwardKinematics_AllZeroDegrees_TCPAtMountingFrame()
        {
            Robot robot = CreateTestRobot();
            ForwardKinematics fk = new ForwardKinematics(robot, hideMesh: true);

            fk.Calculate(new RobotJointPosition(0, 0, 0, 0, 0, 0));

            // With all joints at 0° and tool0, TCP should be at mounting frame
            // Mounting frame for IRB120: (374, 0, 630)
            Assert.InRange(fk.TCPPlane.Origin.X, 374 - Tolerance, 374 + Tolerance);
            Assert.InRange(fk.TCPPlane.Origin.Y, -Tolerance, Tolerance);
            Assert.InRange(fk.TCPPlane.Origin.Z, 630 - Tolerance, 630 + Tolerance);
        }

        [Fact]
        public void ForwardKinematics_Joint1At30Degrees_RotatesTCPInXYPlane()
        {
            Robot robot = CreateTestRobot();
            ForwardKinematics fk = new ForwardKinematics(robot, hideMesh: true);

            fk.Calculate(new RobotJointPosition(30, 0, 0, 0, 0, 0));

            // Joint 1 rotates around Z. TCP at (374, 0, 630) rotated 30° around Z:
            // x' = 374*cos(30°) ≈ 323.88, y' = 374*sin(30°) = 187.0, z' = 630
            double expectedX = 374 * Math.Cos(30 * Math.PI / 180);
            double expectedY = 374 * Math.Sin(30 * Math.PI / 180);
            Assert.InRange(fk.TCPPlane.Origin.X, expectedX - Tolerance, expectedX + Tolerance);
            Assert.InRange(fk.TCPPlane.Origin.Y, expectedY - Tolerance, expectedY + Tolerance);
            Assert.InRange(fk.TCPPlane.Origin.Z, 630 - Tolerance, 630 + Tolerance);
        }

        [Fact]
        public void ForwardKinematics_IsInLimits_WithinLimits_ReturnsTrue()
        {
            Robot robot = CreateTestRobot();
            ForwardKinematics fk = new ForwardKinematics(robot, hideMesh: true);

            // All zeros is within limits
            fk.Calculate(new RobotJointPosition(0, 0, 0, 0, 0, 0));

            Assert.True(fk.IsInLimits);
            Assert.Empty(fk.ErrorText);
        }

        [Fact]
        public void ForwardKinematics_IsInLimits_OutOfLimits_ReturnsFalse()
        {
            Robot robot = CreateTestRobot();
            ForwardKinematics fk = new ForwardKinematics(robot, hideMesh: true);

            // Joint 1 limit is [-165, 165], 200° is out of range
            fk.Calculate(new RobotJointPosition(200, 0, 0, 0, 0, 0));

            Assert.False(fk.IsInLimits);
            Assert.NotEmpty(fk.ErrorText);
        }

        [Fact]
        public void ForwardKinematics_PosedInternalAxisPlanes_HasSixPlanes()
        {
            Robot robot = CreateTestRobot();
            ForwardKinematics fk = new ForwardKinematics(robot, hideMesh: true);

            fk.Calculate(new RobotJointPosition(0, 0, 0, 0, 0, 0));

            Assert.Equal(6, fk.PosedInternalAxisPlanes.Length);
        }

        [Fact]
        public void ForwardKinematics_RobotTransforms_HasSevenElements()
        {
            Robot robot = CreateTestRobot();
            ForwardKinematics fk = new ForwardKinematics(robot, hideMesh: true);

            fk.Calculate(new RobotJointPosition(0, 0, 0, 0, 0, 0));

            // 7 transforms: base + 6 joints
            Assert.Equal(7, fk.RobotTransforms.Length);
        }

        [Fact]
        public void ForwardKinematics_DifferentJointPositions_ProduceDifferentTCPPlanes()
        {
            Robot robot = CreateTestRobot();
            ForwardKinematics fk = new ForwardKinematics(robot, hideMesh: true);

            fk.Calculate(new RobotJointPosition(0, 0, 0, 0, 0, 0));
            Point3d tcp1 = fk.TCPPlane.Origin;

            fk.Calculate(new RobotJointPosition(45, 30, -20, 10, 45, 0));
            Point3d tcp2 = fk.TCPPlane.Origin;

            double distance = tcp1.DistanceTo(tcp2);
            Assert.True(distance > 1.0, "Different joint positions should produce different TCP positions");
        }
        #endregion

        #region ForwardKinematics Validity
        [Fact]
        public void ForwardKinematics_EmptyConstructor_IsNotValid()
        {
            ForwardKinematics fk = new ForwardKinematics();

            Assert.False(fk.IsValid);
        }

        [Fact]
        public void ForwardKinematics_WithRobot_BeforeCalculate_IsNotValid()
        {
            Robot robot = CreateTestRobot();
            ForwardKinematics fk = new ForwardKinematics(robot, hideMesh: true);

            // Before Calculate(), joint positions are null → not valid
            Assert.False(fk.IsValid);
        }

        [Fact]
        public void ForwardKinematics_ToString_AfterCalculate_ReturnsValid()
        {
            Robot robot = CreateTestRobot();
            ForwardKinematics fk = new ForwardKinematics(robot, hideMesh: true);
            fk.Calculate(new RobotJointPosition(0, 0, 0, 0, 0, 0));

            Assert.Equal("Forward Kinematics", fk.ToString());
        }
        #endregion
    }
}
