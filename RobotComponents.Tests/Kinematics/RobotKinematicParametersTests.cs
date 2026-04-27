// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components
// Project: https://github.com/RobotComponents/RobotComponents
//
// For license details, see the LICENSE file in the project root.

// Rhino Libs
using Rhino.Geometry;
// Xunit Libs
using Xunit;
// Robot Components Libs
using RobotComponents.ABB.Definitions;

namespace RobotComponents.Tests.Kinematics
{
    [Collection("RequiresRhino")]
    [Trait("Category", "RequiresRhino")]
    public class RobotKinematicParametersTests
    {
        private const double Tolerance = 1e-6;

        #region Constructor
        [Fact]
        public void Constructor_Empty_AllNaN()
        {
            RobotKinematicParameters p = new RobotKinematicParameters();

            Assert.True(double.IsNaN(p.A1));
            Assert.True(double.IsNaN(p.A2));
            Assert.True(double.IsNaN(p.A3));
            Assert.True(double.IsNaN(p.B));
            Assert.True(double.IsNaN(p.C1));
            Assert.True(double.IsNaN(p.C2));
            Assert.True(double.IsNaN(p.C3));
            Assert.True(double.IsNaN(p.C4));
        }

        [Fact]
        public void Constructor_Empty_IsValidFalse()
        {
            RobotKinematicParameters p = new RobotKinematicParameters();

            Assert.False(p.IsValid);
        }

        [Fact]
        public void Constructor_WithValues_SetsAllProperties()
        {
            RobotKinematicParameters p = new RobotKinematicParameters(150, -100, 0, 135, 615, 705, 755, 85);

            Assert.Equal(150, p.A1);
            Assert.Equal(-100, p.A2);
            Assert.Equal(0, p.A3);
            Assert.Equal(135, p.B);
            Assert.Equal(615, p.C1);
            Assert.Equal(705, p.C2);
            Assert.Equal(755, p.C3);
            Assert.Equal(85, p.C4);
        }

        [Fact]
        public void Constructor_WithValues_IsValidTrue()
        {
            RobotKinematicParameters p = new RobotKinematicParameters(0, -70, 0, 0, 290, 270, 302, 72);

            Assert.True(p.IsValid);
        }

        [Fact]
        public void Constructor_IRB120Parameters_CorrectValues()
        {
            // IRB120-3/0.58 kinematic parameters
            RobotKinematicParameters p = new RobotKinematicParameters(0, -70, 0, 0, 290, 270, 302, 72);

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
        public void Constructor_ZeroWristOffset_StandardSphericalWrist()
        {
            RobotKinematicParameters p = new RobotKinematicParameters(0, -70, 0, 0, 290, 270, 302, 72);

            // A3 = 0 means standard spherical wrist (OPW solver)
            Assert.Equal(0, p.A3);
        }
        #endregion

        #region GetAxisPlanes
        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void GetAxisPlanes_IRB120_ReturnsSixPlanes()
        {
            RobotKinematicParameters p = new RobotKinematicParameters(0, -70, 0, 0, 290, 270, 302, 72);
            Plane[] planes = p.GetAxisPlanes(Plane.WorldXY, out _);

            Assert.Equal(6, planes.Length);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void GetAxisPlanes_IRB120_Plane0AtOrigin()
        {
            RobotKinematicParameters p = new RobotKinematicParameters(0, -70, 0, 0, 290, 270, 302, 72);
            Plane[] planes = p.GetAxisPlanes(Plane.WorldXY, out _);

            // Plane 0: (0, 0, 0) with Z-axis vertical
            Assert.InRange(planes[0].Origin.X, -Tolerance, Tolerance);
            Assert.InRange(planes[0].Origin.Y, -Tolerance, Tolerance);
            Assert.InRange(planes[0].Origin.Z, -Tolerance, Tolerance);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void GetAxisPlanes_IRB120_Plane1AtA1C1()
        {
            RobotKinematicParameters p = new RobotKinematicParameters(0, -70, 0, 0, 290, 270, 302, 72);
            Plane[] planes = p.GetAxisPlanes(Plane.WorldXY, out _);

            // Plane 1: (A1, 0, C1) = (0, 0, 290)
            Assert.InRange(planes[1].Origin.X, -Tolerance, Tolerance);
            Assert.InRange(planes[1].Origin.Z, 290 - Tolerance, 290 + Tolerance);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void GetAxisPlanes_IRB120_Plane5AtWrist()
        {
            RobotKinematicParameters p = new RobotKinematicParameters(0, -70, 0, 0, 290, 270, 302, 72);
            Plane[] planes = p.GetAxisPlanes(Plane.WorldXY, out _);

            // Plane 5: (A1+C3+C4, -B, C1+C2-A2-A3) = (374, 0, 630)
            Assert.InRange(planes[5].Origin.X, 374 - Tolerance, 374 + Tolerance);
            Assert.InRange(planes[5].Origin.Y, -Tolerance, Tolerance);
            Assert.InRange(planes[5].Origin.Z, 630 - Tolerance, 630 + Tolerance);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void GetAxisPlanes_OutputsMountingFrame()
        {
            RobotKinematicParameters p = new RobotKinematicParameters(0, -70, 0, 0, 290, 270, 302, 72);
            p.GetAxisPlanes(Plane.WorldXY, out Plane mountingFrame);

            // Mounting frame at planes[5].Origin = (374, 0, 630)
            Assert.InRange(mountingFrame.Origin.X, 374 - Tolerance, 374 + Tolerance);
            Assert.InRange(mountingFrame.Origin.Z, 630 - Tolerance, 630 + Tolerance);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void GetAxisPlanes_WithOffsetBase_TransformsPlanes()
        {
            RobotKinematicParameters p = new RobotKinematicParameters(0, -70, 0, 0, 290, 270, 302, 72);
            Plane basePlane = new Plane(new Point3d(1000, 0, 0), Vector3d.XAxis, Vector3d.YAxis);
            Plane[] planes = p.GetAxisPlanes(basePlane, out _);

            // Plane 0 should be at base plane origin
            Assert.InRange(planes[0].Origin.X, 1000 - Tolerance, 1000 + Tolerance);
        }
        #endregion

        #region Round-trip
        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void RoundTrip_ValuesToPlanesThenBackToValues_MatchesOriginal()
        {
            // Create parameters from values
            RobotKinematicParameters original = new RobotKinematicParameters(0, -70, 0, 0, 290, 270, 302, 72);

            // Generate axis planes
            Plane[] planes = original.GetAxisPlanes(Plane.WorldXY, out _);

            // Reconstruct parameters from planes
            RobotKinematicParameters reconstructed = new RobotKinematicParameters(Plane.WorldXY, planes);

            // Verify values match
            Assert.InRange(reconstructed.A1, original.A1 - Tolerance, original.A1 + Tolerance);
            Assert.InRange(reconstructed.A2, original.A2 - Tolerance, original.A2 + Tolerance);
            Assert.InRange(reconstructed.A3, original.A3 - Tolerance, original.A3 + Tolerance);
            Assert.InRange(reconstructed.B, original.B - Tolerance, original.B + Tolerance);
            Assert.InRange(reconstructed.C1, original.C1 - Tolerance, original.C1 + Tolerance);
            Assert.InRange(reconstructed.C2, original.C2 - Tolerance, original.C2 + Tolerance);
            Assert.InRange(reconstructed.C3, original.C3 - Tolerance, original.C3 + Tolerance);
            Assert.InRange(reconstructed.C4, original.C4 - Tolerance, original.C4 + Tolerance);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void RoundTrip_LargeRobotParameters_MatchesOriginal()
        {
            // IRB6700-like parameters with non-zero lateral offset
            RobotKinematicParameters original = new RobotKinematicParameters(320, -200, 0, 200, 780, 1075, 1142, 200);

            Plane[] planes = original.GetAxisPlanes(Plane.WorldXY, out _);
            RobotKinematicParameters reconstructed = new RobotKinematicParameters(Plane.WorldXY, planes);

            Assert.InRange(reconstructed.A1, original.A1 - Tolerance, original.A1 + Tolerance);
            Assert.InRange(reconstructed.A2, original.A2 - Tolerance, original.A2 + Tolerance);
            Assert.InRange(reconstructed.B, original.B - Tolerance, original.B + Tolerance);
            Assert.InRange(reconstructed.C1, original.C1 - Tolerance, original.C1 + Tolerance);
            Assert.InRange(reconstructed.C2, original.C2 - Tolerance, original.C2 + Tolerance);
            Assert.InRange(reconstructed.C3, original.C3 - Tolerance, original.C3 + Tolerance);
            Assert.InRange(reconstructed.C4, original.C4 - Tolerance, original.C4 + Tolerance);
        }
        #endregion

        #region Duplicate
        [Fact]
        public void Duplicate_PreservesAllValues()
        {
            RobotKinematicParameters original = new RobotKinematicParameters(0, -70, 0, 0, 290, 270, 302, 72);
            RobotKinematicParameters copy = original.Duplicate();

            Assert.Equal(original.A1, copy.A1);
            Assert.Equal(original.A2, copy.A2);
            Assert.Equal(original.A3, copy.A3);
            Assert.Equal(original.B, copy.B);
            Assert.Equal(original.C1, copy.C1);
            Assert.Equal(original.C2, copy.C2);
            Assert.Equal(original.C3, copy.C3);
            Assert.Equal(original.C4, copy.C4);
        }

        [Fact]
        public void Duplicate_EmptyParameters_PreservesNaN()
        {
            RobotKinematicParameters original = new RobotKinematicParameters();
            RobotKinematicParameters copy = original.Duplicate();

            Assert.True(double.IsNaN(copy.A1));
            Assert.False(copy.IsValid);
        }
        #endregion

        #region ToString
        [Fact]
        public void ToString_ValidParameters_ReturnsExpected()
        {
            RobotKinematicParameters p = new RobotKinematicParameters(0, -70, 0, 0, 290, 270, 302, 72);

            Assert.Equal("Robot Kinematic Parameters", p.ToString());
        }

        [Fact]
        public void ToString_InvalidParameters_ReturnsInvalid()
        {
            RobotKinematicParameters p = new RobotKinematicParameters();

            Assert.Equal("Invalid Robot Kinematic Parameters", p.ToString());
        }
        #endregion
    }
}
