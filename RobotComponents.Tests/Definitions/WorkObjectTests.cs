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
    public class WorkObjectTests
    {
        #region Constructor
        [Fact]
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
        public void ToRAPID_DefaultWobj0_ProducesCorrectFormat()
        {
            WorkObject wobj = new WorkObject();
            string rapid = wobj.ToRAPID();

            // wobj0: WorldXY frames, fixed, no external axis, not robot hold
            Assert.StartsWith("[FALSE, TRUE, \"\", ", rapid);
            Assert.Contains("[[0, 0, 0], [1, 0, 0, 0]]", rapid);
        }

        [Fact]
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
        public void ToRAPIDDeclaration_DefaultScope_ProducesPersWobjdata()
        {
            WorkObject wobj = new WorkObject();
            string decl = wobj.ToRAPIDDeclaration();

            Assert.StartsWith("PERS wobjdata wobj0 := ", decl);
            Assert.EndsWith(";", decl);
        }

        [Fact]
        public void ToRAPIDDeclaration_LocalScope_IncludesLocalPrefix()
        {
            WorkObject wobj = new WorkObject("myWobj", Plane.WorldXY);
            wobj.Scope = Scope.LOCAL;
            string decl = wobj.ToRAPIDDeclaration();

            Assert.StartsWith("LOCAL PERS wobjdata myWobj := ", decl);
        }

        [Fact]
        public void ToRAPIDDeclaration_WithRobot_Wobj0ReturnsEmpty()
        {
            WorkObject wobj = new WorkObject();
            string decl = wobj.ToRAPIDDeclaration(null);

            Assert.Equal(string.Empty, decl);
        }

        [Fact]
        public void ToRAPIDDeclaration_WithRobot_CustomWobjReturnsDeclaration()
        {
            WorkObject wobj = new WorkObject("myWobj", Plane.WorldXY);
            string decl = wobj.ToRAPIDDeclaration(null);

            Assert.StartsWith("PERS wobjdata myWobj := ", decl);
        }

        [Fact]
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
        public void IsValid_DefaultWobj_ReturnsTrue()
        {
            WorkObject wobj = new WorkObject();

            Assert.True(wobj.IsValid);
        }

        [Fact]
        public void IsValid_EmptyName_ReturnsFalse()
        {
            WorkObject wobj = new WorkObject();
            wobj.Name = "";

            Assert.False(wobj.IsValid);
        }

        [Fact]
        public void IsValid_NullName_ReturnsFalse()
        {
            WorkObject wobj = new WorkObject();
            wobj.Name = null;

            Assert.False(wobj.IsValid);
        }
        #endregion

        #region FixedFrame
        [Fact]
        public void FixedFrame_NoExternalAxis_ReturnsTrue()
        {
            WorkObject wobj = new WorkObject("myWobj", Plane.WorldXY);

            Assert.True(wobj.FixedFrame);
        }
        #endregion

        #region GlobalWorkObjectPlane
        [Fact]
        public void GlobalWorkObjectPlane_DefaultWobj_EqualsWorldXY()
        {
            WorkObject wobj = new WorkObject();

            Assert.Equal(Plane.WorldXY.Origin, wobj.GlobalWorkObjectPlane.Origin);
        }

        [Fact]
        public void GlobalWorkObjectPlane_WithUserFrame_CombinesFrames()
        {
            Plane userFrame = new Plane(new Point3d(100, 0, 0), Vector3d.XAxis, Vector3d.YAxis);
            Plane objectFrame = new Plane(new Point3d(50, 0, 0), Vector3d.XAxis, Vector3d.YAxis);
            WorkObject wobj = new WorkObject("myWobj", userFrame, objectFrame);

            // Global plane = objectFrame re-oriented by userFrame
            // objectFrame (50,0,0) placed in userFrame (100,0,0) → global (150,0,0)
            double tolerance = 1e-6;
            Assert.InRange(wobj.GlobalWorkObjectPlane.Origin.X, 150 - tolerance, 150 + tolerance);
            Assert.InRange(wobj.GlobalWorkObjectPlane.Origin.Y, -tolerance, tolerance);
            Assert.InRange(wobj.GlobalWorkObjectPlane.Origin.Z, -tolerance, tolerance);
        }
        #endregion

        #region Duplicate
        [Fact]
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
        public void Datatype_AlwaysReturnsWobjdata()
        {
            WorkObject wobj = new WorkObject();

            Assert.Equal("wobjdata", wobj.Datatype);
        }

        [Fact]
        public void Scope_DefaultIsGlobal()
        {
            WorkObject wobj = new WorkObject();

            Assert.Equal(Scope.GLOBAL, wobj.Scope);
        }

        [Fact]
        public void VariableType_DefaultIsPers()
        {
            WorkObject wobj = new WorkObject();

            Assert.Equal(VariableType.PERS, wobj.VariableType);
        }

        [Fact]
        public void ToString_ValidWobj_ReturnsNameFormat()
        {
            WorkObject wobj = new WorkObject();

            Assert.Equal("Work Object (wobj0)", wobj.ToString());
        }

        [Fact]
        public void ToString_InvalidWobj_ReturnsInvalid()
        {
            WorkObject wobj = new WorkObject();
            wobj.Name = "";

            Assert.Equal("Invalid Work Object", wobj.ToString());
        }
        #endregion

        #region Parse
        [Fact]
        public void Parse_ValidRapidString_CreatesWobjWithCorrectProperties()
        {
            string rapidData = "PERS wobjdata myWobj := [FALSE, TRUE, \"\", [[0, 0, 0], [1, 0, 0, 0]], [[500, 200, 100], [1, 0, 0, 0]]];";
            WorkObject wobj = WorkObject.Parse(rapidData);

            Assert.Equal("myWobj", wobj.Name);
            Assert.False(wobj.RobotHold);
            Assert.True(wobj.IsValid);
        }

        [Fact]
        public void TryParse_ValidRapidString_ReturnsTrueAndWobj()
        {
            string rapidData = "PERS wobjdata myWobj := [FALSE, TRUE, \"\", [[0, 0, 0], [1, 0, 0, 0]], [[500, 200, 100], [1, 0, 0, 0]]];";
            bool success = WorkObject.TryParse(rapidData, out WorkObject wobj);

            Assert.True(success);
            Assert.Equal("myWobj", wobj.Name);
        }

        [Fact]
        public void TryParse_InvalidRapidString_ReturnsFalseAndDefaultWobj()
        {
            bool success = WorkObject.TryParse("invalid data", out WorkObject wobj);

            Assert.False(success);
            Assert.Equal("wobj0", wobj.Name);
        }

        [Fact]
        public void Parse_InvalidRapidString_ThrowsException()
        {
            Assert.Throws<InvalidCastException>(() => WorkObject.Parse("PERS wobjdata w := [FALSE, TRUE]];"));
        }
        #endregion
    }
}
