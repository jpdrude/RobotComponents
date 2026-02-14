// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components
// Project: https://github.com/RobotComponents/RobotComponents
//
// For license details, see the LICENSE file in the project root.

// System Libs
using System.Collections.Generic;
using System.Linq;
// Rhino Libs
using Rhino.Geometry;
// Xunit Libs
using Xunit;
// Robot Components Libs
using RobotComponents.ABB.Definitions;

namespace RobotComponents.Tests.Definitions
{
    public class RobotTests
    {
        private const double Tolerance = 1e-6;

        /// <summary>
        /// Creates a test robot with IRB120-3/0.58 kinematic parameters and empty meshes.
        /// </summary>
        private static Robot CreateTestRobot(Plane basePlane = default, RobotTool tool = null)
        {
            if (basePlane == default) basePlane = Plane.WorldXY;
            if (tool == null) tool = new RobotTool();

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

            return new Robot("TestRobot", meshes, parameters, limits, basePlane, tool);
        }

        #region Constructor
        [Fact]
        public void Constructor_WithKinematicParameters_ProducesValidRobot()
        {
            Robot robot = CreateTestRobot();

            Assert.True(robot.IsValid);
            Assert.Equal("TestRobot", robot.Name);
        }

        [Fact]
        public void Constructor_Empty_ProducesInvalidRobot()
        {
            Robot robot = new Robot();

            Assert.False(robot.IsValid);
            Assert.Equal("Empty Robot", robot.Name);
        }

        [Fact]
        public void Constructor_WorldXYBase_SetsBasePlane()
        {
            Robot robot = CreateTestRobot();

            Assert.Equal(Plane.WorldXY.Origin, robot.BasePlane.Origin);
        }

        [Fact]
        public void Constructor_OffsetBase_SetsBasePlane()
        {
            Plane basePlane = new Plane(new Point3d(1000, 500, 0), Vector3d.XAxis, Vector3d.YAxis);
            Robot robot = CreateTestRobot(basePlane);

            Assert.InRange(robot.BasePlane.Origin.X, 1000 - Tolerance, 1000 + Tolerance);
            Assert.InRange(robot.BasePlane.Origin.Y, 500 - Tolerance, 500 + Tolerance);
        }
        #endregion

        #region Axis Planes
        [Fact]
        public void InternalAxisPlanes_FromKinematicParameters_HasSixPlanes()
        {
            Robot robot = CreateTestRobot();

            Assert.Equal(6, robot.InternalAxisPlanes.Length);
        }

        [Fact]
        public void InternalAxisPlanes_Axis1_AtBaseOrigin()
        {
            Robot robot = CreateTestRobot();
            Plane axis1 = robot.InternalAxisPlanes[0];

            // Axis 1 origin should be at (0, 0, 0) for WorldXY base with A1=0
            Assert.InRange(axis1.Origin.X, -Tolerance, Tolerance);
            Assert.InRange(axis1.Origin.Y, -Tolerance, Tolerance);
            Assert.InRange(axis1.Origin.Z, -Tolerance, Tolerance);
        }

        [Fact]
        public void InternalAxisPlanes_Axis2_AtC1Height()
        {
            Robot robot = CreateTestRobot();
            Plane axis2 = robot.InternalAxisPlanes[1];

            // Axis 2 should be at (A1, 0, C1) = (0, 0, 290) for WorldXY base
            Assert.InRange(axis2.Origin.X, -Tolerance, Tolerance);
            Assert.InRange(axis2.Origin.Y, -Tolerance, Tolerance);
            Assert.InRange(axis2.Origin.Z, 290 - Tolerance, 290 + Tolerance);
        }

        [Fact]
        public void InternalAxisPlanes_Axis3_AtC1PlusC2Height()
        {
            Robot robot = CreateTestRobot();
            Plane axis3 = robot.InternalAxisPlanes[2];

            // Axis 3 should be at (A1, 0, C1+C2) = (0, 0, 560) for WorldXY base
            Assert.InRange(axis3.Origin.X, -Tolerance, Tolerance);
            Assert.InRange(axis3.Origin.Z, 560 - Tolerance, 560 + Tolerance);
        }
        #endregion

        #region Mounting Frame
        [Fact]
        public void MountingFrame_IRB120Parameters_CorrectPosition()
        {
            Robot robot = CreateTestRobot();

            // MountingFrame origin = planes[5].Origin = (A1+C3+C4, -B, C1+C2-A2-A3)
            // = (0+302+72, 0, 290+270-(-70)-0) = (374, 0, 630)
            Assert.InRange(robot.MountingFrame.Origin.X, 374 - Tolerance, 374 + Tolerance);
            Assert.InRange(robot.MountingFrame.Origin.Y, -Tolerance, Tolerance);
            Assert.InRange(robot.MountingFrame.Origin.Z, 630 - Tolerance, 630 + Tolerance);
        }
        #endregion

        #region Tool
        [Fact]
        public void Tool_DefaultTool0_IsValid()
        {
            Robot robot = CreateTestRobot();

            Assert.True(robot.Tool.IsValid);
            Assert.Equal("tool0", robot.Tool.Name);
        }

        [Fact]
        public void ToolPlane_DefaultTool0_AtMountingFrame()
        {
            Robot robot = CreateTestRobot();

            // With tool0 (WorldXY attachment and tool), TCP = mounting frame position
            Assert.InRange(robot.ToolPlane.Origin.X, 374 - Tolerance, 374 + Tolerance);
            Assert.InRange(robot.ToolPlane.Origin.Y, -Tolerance, Tolerance);
            Assert.InRange(robot.ToolPlane.Origin.Z, 630 - Tolerance, 630 + Tolerance);
        }
        #endregion

        #region Properties
        [Fact]
        public void NumberOfAxes_Always6()
        {
            Robot robot = CreateTestRobot();

            Assert.Equal(6, robot.NumberOfAxes);
        }

        [Fact]
        public void InternalAxisLimits_SetCorrectly()
        {
            Robot robot = CreateTestRobot();

            Assert.Equal(6, robot.InternalAxisLimits.Length);
            Assert.Equal(-165, robot.InternalAxisLimits[0].T0);
            Assert.Equal(165, robot.InternalAxisLimits[0].T1);
            Assert.Equal(-400, robot.InternalAxisLimits[5].T0);
            Assert.Equal(400, robot.InternalAxisLimits[5].T1);
        }

        [Fact]
        public void RobotKinematicParameters_Accessible()
        {
            Robot robot = CreateTestRobot();
            RobotKinematicParameters p = robot.RobotKinematicParameters;

            Assert.True(p.IsValid);
            Assert.Equal(0, p.A1);
            Assert.Equal(-70, p.A2);
            Assert.Equal(0, p.A3);
            Assert.Equal(0, p.B);
            Assert.Equal(290, p.C1);
            Assert.Equal(270, p.C2);
            Assert.Equal(302, p.C3);
            Assert.Equal(72, p.C4);
        }

        [Fact]
        public void ExternalAxes_DefaultEmpty()
        {
            Robot robot = CreateTestRobot();

            Assert.Empty(robot.ExternalAxes);
        }

        [Fact]
        public void Meshes_Contains8Elements()
        {
            Robot robot = CreateTestRobot();

            // 7 robot body meshes + 1 tool mesh = 8
            Assert.Equal(8, robot.Meshes.Count);
        }

        [Fact]
        public void InverseKinematics_Initialized()
        {
            Robot robot = CreateTestRobot();

            Assert.NotNull(robot.InverseKinematics);
        }

        [Fact]
        public void ForwardKinematics_Initialized()
        {
            Robot robot = CreateTestRobot();

            Assert.NotNull(robot.ForwardKinematics);
        }
        #endregion

        #region Duplicate
        [Fact]
        public void Duplicate_CreatesIndependentCopy()
        {
            Robot original = CreateTestRobot();
            Robot copy = original.Duplicate();

            copy.Name = "CopiedRobot";

            Assert.Equal("TestRobot", original.Name);
            Assert.Equal("CopiedRobot", copy.Name);
        }

        [Fact]
        public void Duplicate_PreservesKinematicParameters()
        {
            Robot original = CreateTestRobot();
            Robot copy = original.Duplicate();

            Assert.Equal(original.RobotKinematicParameters.A1, copy.RobotKinematicParameters.A1);
            Assert.Equal(original.RobotKinematicParameters.C4, copy.RobotKinematicParameters.C4);
            Assert.True(copy.IsValid);
        }

        [Fact]
        public void Duplicate_PreservesAxisLimits()
        {
            Robot original = CreateTestRobot();
            Robot copy = original.Duplicate();

            for (int i = 0; i < 6; i++)
            {
                Assert.Equal(original.InternalAxisLimits[i].T0, copy.InternalAxisLimits[i].T0);
                Assert.Equal(original.InternalAxisLimits[i].T1, copy.InternalAxisLimits[i].T1);
            }
        }

        [Fact]
        public void DuplicateMechanicalUnit_ReturnsValidRobot()
        {
            Robot original = CreateTestRobot();
            IMechanicalUnit copy = original.DuplicateMechanicalUnit();

            Assert.Equal(6, copy.NumberOfAxes);
        }
        #endregion

        #region ToString
        [Fact]
        public void ToString_ValidRobot_IncludesName()
        {
            Robot robot = CreateTestRobot();

            Assert.Equal("Robot (TestRobot)", robot.ToString());
        }

        [Fact]
        public void ToString_InvalidRobot_ReturnsInvalid()
        {
            Robot robot = new Robot();

            Assert.Equal("Invalid Robot", robot.ToString());
        }
        #endregion
    }
}
