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
using RobotComponents.ABB.Actions.Declarations;

namespace RobotComponents.Tests.Actions
{
    public class RobotTargetTests
    {
        #region Constructor
        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void Constructor_PlaneOnly_SetsDefaultConfigAndExternal()
        {
            RobotTarget rt = new RobotTarget(Plane.WorldXY);

            Assert.Equal(0, rt.ConfigurationData.Cf1);
            Assert.Equal(0, rt.ConfigurationData.Cfx);
            Assert.Equal(9e9, rt.ExternalJointPosition[0]);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void Constructor_Named_SetsName()
        {
            RobotTarget rt = new RobotTarget("t1", Plane.WorldXY);

            Assert.Equal("t1", rt.Name);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void Constructor_FullArgs_SetsAllFields()
        {
            ConfigurationData cd = new ConfigurationData(1, 2, 3, 4);
            ExternalJointPosition ejp = new ExternalJointPosition(100);
            RobotTarget rt = new RobotTarget("t1", Plane.WorldXY, cd, ejp);

            Assert.Equal("t1", rt.Name);
            Assert.Equal(1, rt.ConfigurationData.Cf1);
            Assert.Equal(100, rt.ExternalJointPosition[0]);
        }
        #endregion

        #region ToRAPID
        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void ToRAPID_WorldXYPlane_ProducesIdentityQuaternion()
        {
            RobotTarget rt = new RobotTarget(Plane.WorldXY);

            string rapid = rt.ToRAPID();

            // Position [0, 0, 0], Quaternion [1, 0, 0, 0], Config [0, 0, 0, 0], ExtJoint [9E9 x6]
            Assert.Equal("[[0, 0, 0], [1, 0, 0, 0], [0, 0, 0, 0], [9E9, 9E9, 9E9, 9E9, 9E9, 9E9]]", rapid);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void ToRAPID_TranslatedPlane_ProducesCorrectPosition()
        {
            Plane plane = new Plane(new Point3d(100, 200, 300), Vector3d.ZAxis);
            RobotTarget rt = new RobotTarget(plane);

            string rapid = rt.ToRAPID();

            Assert.Contains("[100, 200, 300]", rapid);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void ToRAPID_WithConfigData_IncludesConfigValues()
        {
            ConfigurationData cd = new ConfigurationData(1, 2, 3, 4);
            ExternalJointPosition ejp = new ExternalJointPosition();
            RobotTarget rt = new RobotTarget("t1", Plane.WorldXY, cd, ejp);

            string rapid = rt.ToRAPID();

            Assert.Contains("[1, 2, 3, 4]", rapid);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void ToRAPID_WithExternalAxes_IncludesAxisValues()
        {
            ExternalJointPosition ejp = new ExternalJointPosition(100);
            ConfigurationData cd = new ConfigurationData(0, 0, 0, 0);
            RobotTarget rt = new RobotTarget("t1", Plane.WorldXY, cd, ejp);

            string rapid = rt.ToRAPID();

            Assert.Contains("[100, 9E9, 9E9, 9E9, 9E9, 9E9]", rapid);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void ToRAPID_NamedConfigData_UsesName()
        {
            ConfigurationData cd = new ConfigurationData("myconf", 1, 2, 3, 4);
            ExternalJointPosition ejp = new ExternalJointPosition();
            RobotTarget rt = new RobotTarget("t1", Plane.WorldXY, cd, ejp);

            string rapid = rt.ToRAPID();

            Assert.Contains("myconf", rapid);
        }
        #endregion

        #region ToRAPIDDeclaration
        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void ToRAPIDDeclaration_Named_ProducesRobtargetDeclaration()
        {
            RobotTarget rt = new RobotTarget("t1", Plane.WorldXY);

            string result = rt.ToRAPIDDeclaration(null);

            Assert.StartsWith("VAR robtarget t1 := ", result);
            Assert.EndsWith(";", result);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void ToRAPIDDeclaration_Unnamed_ReturnsEmpty()
        {
            RobotTarget rt = new RobotTarget(Plane.WorldXY);

            Assert.Equal(string.Empty, rt.ToRAPIDDeclaration(null));
        }
        #endregion

        #region ToRAPIDInstruction
        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void ToRAPIDInstruction_ReturnsEmpty()
        {
            RobotTarget rt = new RobotTarget("t1", Plane.WorldXY);

            Assert.Equal(string.Empty, rt.ToRAPIDInstruction(null));
        }
        #endregion

        #region IsValid
        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void IsValid_ValidPlane_ReturnsTrue()
        {
            RobotTarget rt = new RobotTarget("t1", Plane.WorldXY);

            Assert.True(rt.IsValid);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void IsValid_UnsetPlane_ReturnsFalse()
        {
            RobotTarget rt = new RobotTarget();

            Assert.False(rt.IsValid);
        }
        #endregion

        #region Duplicate
        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void Duplicate_ReturnsMatchingValues()
        {
            ConfigurationData cd = new ConfigurationData(1, 2, 3, 4);
            ExternalJointPosition ejp = new ExternalJointPosition(100);
            RobotTarget original = new RobotTarget("t1", Plane.WorldXY, cd, ejp);

            RobotTarget copy = original.Duplicate();

            Assert.Equal(original.Name, copy.Name);
            Assert.Equal(original.ToRAPID(), copy.ToRAPID());
        }
        #endregion

        #region Parse / TryParse
        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void Parse_ValidRapidString_ReturnsCorrectTarget()
        {
            RobotTarget rt = RobotTarget.Parse("VAR robtarget t1 := [[100, 200, 300], [1, 0, 0, 0], [0, 0, 0, 0], [9E9, 9E9, 9E9, 9E9, 9E9, 9E9]];");

            Assert.Equal("t1", rt.Name);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void TryParse_InvalidString_ReturnsFalse()
        {
            bool result = RobotTarget.TryParse("garbage", out RobotTarget rt);

            Assert.False(result);
        }
        #endregion
    }
}
