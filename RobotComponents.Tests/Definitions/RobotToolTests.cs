// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components
// Project: https://github.com/RobotComponents/RobotComponents
//
// For license details, see the LICENSE file in the project root.

// System Libs
using System;
// Rhino Libs
using Rhino.Geometry;
// Xunit Libs
using Xunit;
// Robot Components Libs
using RobotComponents.ABB.Definitions;
using RobotComponents.ABB.Enumerations;

namespace RobotComponents.Tests.Definitions
{
    public class RobotToolTests
    {
        #region Constructor
        [Fact]
        public void Constructor_Default_CreatesTool0()
        {
            RobotTool tool = new RobotTool();

            Assert.Equal("tool0", tool.Name);
            Assert.True(tool.RobotHold);
            Assert.Equal(Plane.WorldXY, tool.AttachmentPlane);
            Assert.Equal(Plane.WorldXY, tool.ToolPlane);
            Assert.True(tool.IsValid);
        }

        [Fact]
        public void Constructor_CustomPlanes_SetsProperties()
        {
            Plane toolPlane = new Plane(new Point3d(100, 0, 200), Vector3d.XAxis, Vector3d.YAxis);
            RobotTool tool = new RobotTool("myTool", new Mesh(), Plane.WorldXY, toolPlane);

            Assert.Equal("myTool", tool.Name);
            Assert.True(tool.RobotHold);
            Assert.Equal(Plane.WorldXY, tool.AttachmentPlane);
            Assert.True(tool.IsValid);
        }

        [Fact]
        public void Constructor_WithLoadData_SetsLoadData()
        {
            LoadData load = new LoadData("load1", 5.0,
                new Point3d(50, 0, 50),
                new Quaternion(1, 0, 0, 0),
                new Vector3d(0, 0, 0));
            Plane toolPlane = new Plane(new Point3d(100, 0, 200), Vector3d.XAxis, Vector3d.YAxis);
            RobotTool tool = new RobotTool("myTool", new Mesh(), Plane.WorldXY, toolPlane, load);

            Assert.Equal("myTool", tool.Name);
            Assert.Equal(5.0, tool.LoadData.Mass);
            Assert.True(tool.IsValid);
        }
        #endregion

        #region ToRAPID
        [Fact]
        public void ToRAPID_DefaultTool0_ProducesCorrectFormat()
        {
            RobotTool tool = new RobotTool();
            string rapid = tool.ToRAPID();

            // tool0: WorldXY attachment and tool planes → position (0,0,0), identity quaternion
            Assert.StartsWith("[TRUE, [[0, 0, 0], [1, 0, 0, 0]], ", rapid);
            Assert.EndsWith("]]", rapid);
            Assert.Contains("[0.001, [0, 0, 0.001], [1, 0, 0, 0], 0, 0, 0]", rapid);
        }

        [Fact]
        public void ToRAPID_CustomToolAtOriginSameOrientation_CorrectPosition()
        {
            Plane toolPlane = new Plane(new Point3d(100, 0, 200), Vector3d.XAxis, Vector3d.YAxis);
            RobotTool tool = new RobotTool("myTool", new Mesh(), Plane.WorldXY, toolPlane);
            string rapid = tool.ToRAPID();

            Assert.StartsWith("[TRUE, [[100, 0, 200], [1, 0, 0, 0]], ", rapid);
        }

        [Fact]
        public void ToRAPID_RobotHoldFalse_StartsWithFalse()
        {
            RobotTool tool = new RobotTool();
            tool.RobotHold = false;
            string rapid = tool.ToRAPID();

            Assert.StartsWith("[FALSE, ", rapid);
        }
        #endregion

        #region ToRAPIDDeclaration
        [Fact]
        public void ToRAPIDDeclaration_DefaultScope_ProducesPersTooldata()
        {
            RobotTool tool = new RobotTool();
            string decl = tool.ToRAPIDDeclaration();

            Assert.StartsWith("PERS tooldata tool0 := ", decl);
            Assert.EndsWith(";", decl);
        }

        [Fact]
        public void ToRAPIDDeclaration_LocalScope_IncludesLocalPrefix()
        {
            RobotTool tool = new RobotTool("myTool", new Mesh(), Plane.WorldXY, Plane.WorldXY);
            tool.Scope = Scope.LOCAL;
            string decl = tool.ToRAPIDDeclaration();

            Assert.StartsWith("LOCAL PERS tooldata myTool := ", decl);
        }

        [Fact]
        public void ToRAPIDDeclaration_TaskScope_IncludesTaskPrefix()
        {
            RobotTool tool = new RobotTool("myTool", new Mesh(), Plane.WorldXY, Plane.WorldXY);
            tool.Scope = Scope.TASK;
            string decl = tool.ToRAPIDDeclaration();

            Assert.StartsWith("TASK PERS tooldata myTool := ", decl);
        }

        [Fact]
        public void ToRAPIDDeclaration_WithRobot_Tool0ReturnsEmpty()
        {
            RobotTool tool = new RobotTool();
            string decl = tool.ToRAPIDDeclaration(null);

            Assert.Equal(string.Empty, decl);
        }

        [Fact]
        public void ToRAPIDDeclaration_WithRobot_CustomToolReturnsDeclaration()
        {
            RobotTool tool = new RobotTool("myTool", new Mesh(), Plane.WorldXY, Plane.WorldXY);
            string decl = tool.ToRAPIDDeclaration(null);

            Assert.StartsWith("PERS tooldata myTool := ", decl);
        }

        [Fact]
        public void ToRAPIDDeclaration_WithRobot_EmptyNameReturnsEmpty()
        {
            RobotTool tool = new RobotTool();
            tool.Name = "";
            string decl = tool.ToRAPIDDeclaration(null);

            Assert.Equal(string.Empty, decl);
        }
        #endregion

        #region IsValid
        [Fact]
        public void IsValid_DefaultTool_ReturnsTrue()
        {
            RobotTool tool = new RobotTool();

            Assert.True(tool.IsValid);
        }

        [Fact]
        public void IsValid_EmptyName_ReturnsFalse()
        {
            RobotTool tool = new RobotTool();
            tool.Name = "";

            Assert.False(tool.IsValid);
        }

        [Fact]
        public void IsValid_UnsetAttachmentPlane_ReturnsFalse()
        {
            RobotTool tool = RobotTool.GetEmptyRobotTool();

            Assert.False(tool.IsValid);
        }
        #endregion

        #region Duplicate
        [Fact]
        public void Duplicate_CreatesIndependentCopy()
        {
            RobotTool original = new RobotTool("myTool", new Mesh(), Plane.WorldXY,
                new Plane(new Point3d(100, 0, 200), Vector3d.XAxis, Vector3d.YAxis));

            RobotTool copy = original.Duplicate();
            copy.Name = "changed";

            Assert.Equal("myTool", original.Name);
            Assert.Equal("changed", copy.Name);
        }

        [Fact]
        public void DuplicateWithoutMesh_CreatesEmptyMeshCopy()
        {
            RobotTool original = new RobotTool("myTool", new Mesh(), Plane.WorldXY, Plane.WorldXY);
            RobotTool copy = original.DuplicateWithoutMesh();

            Assert.Equal("myTool", copy.Name);
            Assert.True(copy.IsValid);
        }
        #endregion

        #region Properties
        [Fact]
        public void Datatype_AlwaysReturnsTooldata()
        {
            RobotTool tool = new RobotTool();

            Assert.Equal("tooldata", tool.Datatype);
        }

        [Fact]
        public void Scope_DefaultIsGlobal()
        {
            RobotTool tool = new RobotTool();

            Assert.Equal(Scope.GLOBAL, tool.Scope);
        }

        [Fact]
        public void VariableType_DefaultIsPers()
        {
            RobotTool tool = new RobotTool();

            Assert.Equal(VariableType.PERS, tool.VariableType);
        }

        [Fact]
        public void ToString_ValidTool_ReturnsNameFormat()
        {
            RobotTool tool = new RobotTool();

            Assert.Equal("Robot Tool (tool0)", tool.ToString());
        }

        [Fact]
        public void ToString_InvalidTool_ReturnsInvalid()
        {
            RobotTool tool = RobotTool.GetEmptyRobotTool();

            Assert.Equal("Invalid Robot Tool", tool.ToString());
        }
        #endregion

        #region Parse
        [Fact]
        public void Parse_ValidRapidString_CreatesToolWithCorrectProperties()
        {
            string rapidData = "PERS tooldata myTool := [TRUE, [[100, 0, 200], [1, 0, 0, 0]], [5, [50, 0, 50], [1, 0, 0, 0], 0, 0, 0]];";
            RobotTool tool = RobotTool.Parse(rapidData);

            Assert.Equal("myTool", tool.Name);
            Assert.True(tool.RobotHold);
            Assert.Equal(5.0, tool.LoadData.Mass);
            Assert.True(tool.IsValid);
        }

        [Fact]
        public void TryParse_ValidRapidString_ReturnsTrueAndTool()
        {
            string rapidData = "PERS tooldata myTool := [TRUE, [[100, 0, 200], [1, 0, 0, 0]], [5, [50, 0, 50], [1, 0, 0, 0], 0, 0, 0]];";
            bool success = RobotTool.TryParse(rapidData, out RobotTool tool);

            Assert.True(success);
            Assert.Equal("myTool", tool.Name);
        }

        [Fact]
        public void TryParse_InvalidRapidString_ReturnsFalseAndDefaultTool()
        {
            bool success = RobotTool.TryParse("invalid data", out RobotTool tool);

            Assert.False(success);
            Assert.Equal("tool0", tool.Name);
        }

        [Fact]
        public void Parse_InvalidRapidString_ThrowsException()
        {
            Assert.Throws<InvalidCastException>(() => RobotTool.Parse("PERS tooldata t := [TRUE, [0, 0]];"));
        }
        #endregion
    }
}
