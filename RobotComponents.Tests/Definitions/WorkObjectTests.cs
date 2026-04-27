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
    [Collection("RequiresRhino")]
    [Trait("Category", "RequiresRhino")]
    public class WorkObjectTests
    {
        private const double Tolerance = 1e-6;

        #region Constructor
        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void Constructor_Default_CreatesWobj0()
        {
            WorkObject wobj = new WorkObject();

            Assert.Equal("wobj0", wobj.Name);
            Assert.False(wobj.RobotHold);
            Assert.True(wobj.FixedFrame);
            Assert.Equal(Plane.WorldXY, wobj.ObjectFrame);
            Assert.Equal(Plane.WorldXY, wobj.UserFrame);
            Assert.Null(wobj.ExternalAxis);
            Assert.True(wobj.IsValid);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void Constructor_NameAndObjectFrame_SetsPropertiesCorrectly()
        {
            Plane objectFrame = new Plane(new Point3d(500, 200, 100), Vector3d.XAxis, Vector3d.YAxis);
            WorkObject wobj = new WorkObject("myWobj", objectFrame);

            Assert.Equal("myWobj", wobj.Name);
            Assert.Equal(Plane.WorldXY, wobj.UserFrame);
            Assert.True(wobj.FixedFrame);
            Assert.True(wobj.IsValid);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void Constructor_WithUserFrame_SetsUserFrameCorrectly()
        {
            Plane userFrame = new Plane(new Point3d(100, 0, 0), Vector3d.XAxis, Vector3d.YAxis);
            Plane objectFrame = new Plane(new Point3d(500, 200, 100), Vector3d.XAxis, Vector3d.YAxis);
            WorkObject wobj = new WorkObject("myWobj", userFrame, objectFrame);

            Assert.Equal("myWobj", wobj.Name);
            Assert.Equal(userFrame, wobj.UserFrame);
            Assert.True(wobj.FixedFrame);
            Assert.True(wobj.IsValid);
        }
        #endregion

        #region ToRAPID
        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void ToRAPID_DefaultWobj0_ProducesCorrectFormat()
        {
            WorkObject wobj = new WorkObject();
            string rapid = wobj.ToRAPID();

            // wobj0: WorldXY frames, fixed, no external axis, not robot hold
            Assert.StartsWith("[FALSE, TRUE, \"\", ", rapid);
            Assert.Contains("[[0, 0, 0], [1, 0, 0, 0]]", rapid);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void ToRAPID_CustomObjectFrame_IncludesTranslation()
        {
            Plane objectFrame = new Plane(new Point3d(500, 200, 100), Vector3d.XAxis, Vector3d.YAxis);
            WorkObject wobj = new WorkObject("myWobj", objectFrame);
            string rapid = wobj.ToRAPID();

            // Object frame should contain the translated origin
            Assert.Contains("[[500, 200, 100], [1, 0, 0, 0]]", rapid);
            // User frame at origin
            Assert.Contains("[FALSE, TRUE, \"\", [[0, 0, 0], [1, 0, 0, 0]]", rapid);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void ToRAPID_WithUserFrame_IncludesUserFrameData()
        {
            Plane userFrame = new Plane(new Point3d(100, 50, 0), Vector3d.XAxis, Vector3d.YAxis);
            Plane objectFrame = new Plane(new Point3d(500, 200, 100), Vector3d.XAxis, Vector3d.YAxis);
            WorkObject wobj = new WorkObject("myWobj", userFrame, objectFrame);
            string rapid = wobj.ToRAPID();

            Assert.Contains("[[100, 50, 0], [1, 0, 0, 0]]", rapid);
        }
        #endregion

        #region ToRAPIDDeclaration
        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void ToRAPIDDeclaration_DefaultScope_ProducesPersWobjdata()
        {
            WorkObject wobj = new WorkObject();
            string decl = wobj.ToRAPIDDeclaration();

            Assert.StartsWith("PERS wobjdata wobj0 := ", decl);
            Assert.EndsWith(";", decl);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void ToRAPIDDeclaration_LocalScope_IncludesLocalPrefix()
        {
            WorkObject wobj = new WorkObject("myWobj", Plane.WorldXY);
            wobj.Scope = Scope.LOCAL;
            string decl = wobj.ToRAPIDDeclaration();

            Assert.StartsWith("LOCAL PERS wobjdata myWobj := ", decl);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void ToRAPIDDeclaration_WithRobot_Wobj0ReturnsEmpty()
        {
            WorkObject wobj = new WorkObject();
            string decl = wobj.ToRAPIDDeclaration(null);

            Assert.Equal(string.Empty, decl);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void ToRAPIDDeclaration_WithRobot_CustomWobjReturnsDeclaration()
        {
            WorkObject wobj = new WorkObject("myWobj", Plane.WorldXY);
            string decl = wobj.ToRAPIDDeclaration(null);

            Assert.StartsWith("PERS wobjdata myWobj := ", decl);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void ToRAPIDDeclaration_WithRobot_EmptyNameReturnsEmpty()
        {
            WorkObject wobj = new WorkObject();
            wobj.Name = "";
            string decl = wobj.ToRAPIDDeclaration(null);

            Assert.Equal(string.Empty, decl);
        }
        #endregion

        #region IsValid
        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void IsValid_DefaultWobj_ReturnsTrue()
        {
            WorkObject wobj = new WorkObject();

            Assert.True(wobj.IsValid);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void IsValid_EmptyName_ReturnsFalse()
        {
            WorkObject wobj = new WorkObject();
            wobj.Name = "";

            Assert.False(wobj.IsValid);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void IsValid_NullName_ReturnsFalse()
        {
            WorkObject wobj = new WorkObject();
            wobj.Name = null;

            Assert.False(wobj.IsValid);
        }
        #endregion

        #region FixedFrame
        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void FixedFrame_NoExternalAxis_ReturnsTrue()
        {
            WorkObject wobj = new WorkObject("myWobj", Plane.WorldXY);

            Assert.True(wobj.FixedFrame);
        }
        #endregion

        #region GlobalWorkObjectPlane
        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void GlobalWorkObjectPlane_DefaultWobj_EqualsWorldXY()
        {
            WorkObject wobj = new WorkObject();

            Assert.Equal(Plane.WorldXY.Origin, wobj.GlobalWorkObjectPlane.Origin);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void GlobalWorkObjectPlane_WithUserFrame_CombinesFrames()
        {
            Plane userFrame = new Plane(new Point3d(100, 0, 0), Vector3d.XAxis, Vector3d.YAxis);
            Plane objectFrame = new Plane(new Point3d(50, 0, 0), Vector3d.XAxis, Vector3d.YAxis);
            WorkObject wobj = new WorkObject("myWobj", userFrame, objectFrame);

            // Global plane = objectFrame re-oriented by userFrame
            // objectFrame (50,0,0) placed in userFrame (100,0,0) → global (150,0,0)
            Assert.InRange(wobj.GlobalWorkObjectPlane.Origin.X, 150 - Tolerance, 150 + Tolerance);
            Assert.InRange(wobj.GlobalWorkObjectPlane.Origin.Y, -Tolerance, Tolerance);
            Assert.InRange(wobj.GlobalWorkObjectPlane.Origin.Z, -Tolerance, Tolerance);
        }
        #endregion

        #region Duplicate
        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void Duplicate_CreatesIndependentCopy()
        {
            WorkObject original = new WorkObject("myWobj",
                new Plane(new Point3d(500, 200, 100), Vector3d.XAxis, Vector3d.YAxis));

            WorkObject copy = original.Duplicate();
            copy.Name = "changed";

            Assert.Equal("myWobj", original.Name);
            Assert.Equal("changed", copy.Name);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void Duplicate_PreservesAllProperties()
        {
            Plane userFrame = new Plane(new Point3d(100, 0, 0), Vector3d.XAxis, Vector3d.YAxis);
            Plane objectFrame = new Plane(new Point3d(500, 200, 100), Vector3d.XAxis, Vector3d.YAxis);
            WorkObject original = new WorkObject("myWobj", userFrame, objectFrame);
            original.Scope = Scope.LOCAL;

            WorkObject copy = original.Duplicate();

            Assert.Equal("myWobj", copy.Name);
            Assert.Equal(Scope.LOCAL, copy.Scope);
            Assert.Equal(original.ObjectFrame, copy.ObjectFrame);
            Assert.Equal(original.UserFrame, copy.UserFrame);
            Assert.True(copy.FixedFrame);
        }
        #endregion

        #region Properties
        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void Datatype_AlwaysReturnsWobjdata()
        {
            WorkObject wobj = new WorkObject();

            Assert.Equal("wobjdata", wobj.Datatype);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void Scope_DefaultIsGlobal()
        {
            WorkObject wobj = new WorkObject();

            Assert.Equal(Scope.GLOBAL, wobj.Scope);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void VariableType_DefaultIsPers()
        {
            WorkObject wobj = new WorkObject();

            Assert.Equal(VariableType.PERS, wobj.VariableType);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void ToString_ValidWobj_ReturnsNameFormat()
        {
            WorkObject wobj = new WorkObject();

            Assert.Equal("Work Object (wobj0)", wobj.ToString());
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void ToString_InvalidWobj_ReturnsInvalid()
        {
            WorkObject wobj = new WorkObject();
            wobj.Name = "";

            Assert.Equal("Invalid Work Object", wobj.ToString());
        }
        #endregion

        #region Parse
        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void Parse_ValidRapidString_CreatesWobjWithCorrectProperties()
        {
            string rapidData = "PERS wobjdata myWobj := [FALSE, TRUE, \"\", [[0, 0, 0], [1, 0, 0, 0]], [[500, 200, 100], [1, 0, 0, 0]]];";
            WorkObject wobj = WorkObject.Parse(rapidData);

            Assert.Equal("myWobj", wobj.Name);
            Assert.False(wobj.RobotHold);
            Assert.True(wobj.IsValid);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void TryParse_ValidRapidString_ReturnsTrueAndWobj()
        {
            string rapidData = "PERS wobjdata myWobj := [FALSE, TRUE, \"\", [[0, 0, 0], [1, 0, 0, 0]], [[500, 200, 100], [1, 0, 0, 0]]];";
            bool success = WorkObject.TryParse(rapidData, out WorkObject wobj);

            Assert.True(success);
            Assert.Equal("myWobj", wobj.Name);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void TryParse_InvalidRapidString_ReturnsFalseAndDefaultWobj()
        {
            bool success = WorkObject.TryParse("invalid data", out WorkObject wobj);

            Assert.False(success);
            Assert.Equal("wobj0", wobj.Name);
        }

        // No RequiresRhino: Parse throws before reaching any Rhino native call
        [Fact]
        public void Parse_InvalidRapidString_ThrowsException()
        {
            Assert.Throws<InvalidCastException>(() => WorkObject.Parse("PERS wobjdata w := [FALSE, TRUE]];"));
        }
        #endregion
    }
}
