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
using RobotComponents.ABB.Actions;
using RobotComponents.ABB.Actions.Declarations;
using RobotComponents.ABB.Definitions;
using RobotComponents.ABB.Enumerations;
using RobotComponents.ABB.Utils;

namespace RobotComponents.Tests
{
    public class HelperMethodTests
    {
        #region constants
        private const double Tolerance = 1e-6;
        #endregion

        #region Quaternion / Plane round-trip
        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void PlaneToQuaternion_ThenQuaternionToPlane_ReturnsOriginalPlane()
        {
            Plane original = new Plane(new Point3d(10, 20, 30), new Vector3d(1, 0, 0), new Vector3d(0, 1, 0));

            Quaternion quat = HelperMethods.PlaneToQuaternion(original);
            Plane result = HelperMethods.QuaternionToPlane(original.Origin, quat);

            Assert.True(original.Origin.DistanceTo(result.Origin) < Tolerance,
                $"Origins differ: expected {original.Origin}, got {result.Origin}");
            Assert.True(Math.Abs(original.XAxis * result.XAxis - 1.0) < Tolerance,
                $"XAxis differs: expected {original.XAxis}, got {result.XAxis}");
            Assert.True(Math.Abs(original.YAxis * result.YAxis - 1.0) < Tolerance,
                $"YAxis differs: expected {original.YAxis}, got {result.YAxis}");
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void PlaneToQuaternion_ThenQuaternionToPlane_RotatedPlane_ReturnsOriginalPlane()
        {
            // A rotated plane (45 degrees around Z)
            double angle = Math.PI / 4;
            Vector3d xAxis = new Vector3d(Math.Cos(angle), Math.Sin(angle), 0);
            Vector3d yAxis = new Vector3d(-Math.Sin(angle), Math.Cos(angle), 0);
            Plane original = new Plane(new Point3d(5, -3, 7), xAxis, yAxis);

            Quaternion quat = HelperMethods.PlaneToQuaternion(original);
            Plane result = HelperMethods.QuaternionToPlane(original.Origin, quat);

            Assert.True(original.Origin.DistanceTo(result.Origin) < Tolerance,
                "Origins differ after round-trip");
            Assert.True(Math.Abs(original.XAxis * result.XAxis - 1.0) < Tolerance,
                "XAxis differs after round-trip");
            Assert.True(Math.Abs(original.YAxis * result.YAxis - 1.0) < Tolerance,
                "YAxis differs after round-trip");
        }
        #endregion

        #region Known quaternion values
        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void IdentityQuaternion_ProducesWorldXYPlane()
        {
            Quaternion identity = new Quaternion(1, 0, 0, 0);
            Point3d origin = Point3d.Origin;

            Plane result = HelperMethods.QuaternionToPlane(origin, identity);

            Assert.True(origin.DistanceTo(result.Origin) < Tolerance,
                "Origin should be at world origin");
            Assert.True(Math.Abs(result.XAxis * Vector3d.XAxis - 1.0) < Tolerance,
                $"XAxis should be (1,0,0), got {result.XAxis}");
            Assert.True(Math.Abs(result.YAxis * Vector3d.YAxis - 1.0) < Tolerance,
                $"YAxis should be (0,1,0), got {result.YAxis}");
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void QuaternionToPlane_WithComponentOverload_MatchesQuaternionOverload()
        {
            Point3d origin = new Point3d(1, 2, 3);
            double a = 0.7071068, b = 0, c = 0, d = 0.7071068;

            Plane result1 = HelperMethods.QuaternionToPlane(origin, a, b, c, d);
            Plane result2 = HelperMethods.QuaternionToPlane(origin, new Quaternion(a, b, c, d));

            Assert.True(result1.Origin.DistanceTo(result2.Origin) < Tolerance, "Origins should match");
            Assert.True(Math.Abs(result1.XAxis * result2.XAxis - 1.0) < Tolerance, "XAxis should match");
            Assert.True(Math.Abs(result1.YAxis * result2.YAxis - 1.0) < Tolerance, "YAxis should match");
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void QuaternionToPlane_XYZOverload_SetsCorrectOrigin()
        {
            Plane result = HelperMethods.QuaternionToPlane(10.0, 20.0, 30.0, 1, 0, 0, 0);

            Assert.True(Math.Abs(result.Origin.X - 10.0) < Tolerance, "X should be 10");
            Assert.True(Math.Abs(result.Origin.Y - 20.0) < Tolerance, "Y should be 20");
            Assert.True(Math.Abs(result.Origin.Z - 30.0) < Tolerance, "Z should be 30");
        }
        #endregion

        #region FlipPlaneX / FlipPlaneY
        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void FlipPlaneX_NegatesXAxis_OriginUnchanged()
        {
            Plane original = new Plane(new Point3d(1, 2, 3), Vector3d.XAxis, Vector3d.YAxis);

            Plane flipped = HelperMethods.FlipPlaneX(original);

            Assert.True(original.Origin.DistanceTo(flipped.Origin) < Tolerance,
                "Origin should be unchanged");
            Assert.True(Math.Abs(flipped.XAxis * (-Vector3d.XAxis) - 1.0) < Tolerance,
                $"XAxis should be negated, got {flipped.XAxis}");
            Assert.True(Math.Abs(flipped.YAxis * Vector3d.YAxis - 1.0) < Tolerance,
                "YAxis should be unchanged");
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void FlipPlaneY_NegatesYAxis_OriginUnchanged()
        {
            Plane original = new Plane(new Point3d(1, 2, 3), Vector3d.XAxis, Vector3d.YAxis);

            Plane flipped = HelperMethods.FlipPlaneY(original);

            Assert.True(original.Origin.DistanceTo(flipped.Origin) < Tolerance,
                "Origin should be unchanged");
            Assert.True(Math.Abs(flipped.XAxis * Vector3d.XAxis - 1.0) < Tolerance,
                "XAxis should be unchanged");
            Assert.True(Math.Abs(flipped.YAxis * (-Vector3d.YAxis) - 1.0) < Tolerance,
                $"YAxis should be negated, got {flipped.YAxis}");
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void FlipPlaneX_FlipsNormal()
        {
            Plane original = Plane.WorldXY;
            Plane flipped = HelperMethods.FlipPlaneX(original);

            // Flipping X negates the normal (Z = X cross Y, so -X cross Y = -Z)
            Assert.True(Math.Abs(flipped.Normal * (-original.Normal) - 1.0) < Tolerance,
                "Normal should be flipped");
        }
        #endregion

        #region Slerp
        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void Slerp_AtZero_ReturnsFirstQuaternion()
        {
            Quaternion q1 = new Quaternion(1, 0, 0, 0);
            Quaternion q2 = new Quaternion(0, 1, 0, 0);

            Quaternion result = HelperMethods.Slerp(q1, q2, 0.0);
            result.Unitize();

            Assert.True(Math.Abs(result.A - q1.A) < Tolerance &&
                        Math.Abs(result.B - q1.B) < Tolerance &&
                        Math.Abs(result.C - q1.C) < Tolerance &&
                        Math.Abs(result.D - q1.D) < Tolerance,
                $"Slerp(t=0) should return q1. Got ({result.A}, {result.B}, {result.C}, {result.D})");
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void Slerp_AtOne_ReturnsSecondQuaternion()
        {
            Quaternion q1 = new Quaternion(1, 0, 0, 0);
            Quaternion q2 = new Quaternion(0, 1, 0, 0);

            Quaternion result = HelperMethods.Slerp(q1, q2, 1.0);
            result.Unitize();

            // The result may be q2 or -q2 (both represent the same rotation)
            double dot = Math.Abs(result.A * q2.A + result.B * q2.B + result.C * q2.C + result.D * q2.D);
            Assert.True(Math.Abs(dot - 1.0) < Tolerance,
                $"Slerp(t=1) should return q2 (or -q2). Got ({result.A}, {result.B}, {result.C}, {result.D})");
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void Slerp_AtHalf_ReturnsMidpointRotation()
        {
            Quaternion q1 = new Quaternion(1, 0, 0, 0);
            // 90 degree rotation around Z
            double halfAngle = Math.PI / 4;
            Quaternion q2 = new Quaternion(Math.Cos(halfAngle), 0, 0, Math.Sin(halfAngle));

            Quaternion mid = HelperMethods.Slerp(q1, q2, 0.5);
            mid.Unitize();

            // Midpoint should be a 45 degree rotation around Z
            double expectedHalf = Math.PI / 8;
            Quaternion expected = new Quaternion(Math.Cos(expectedHalf), 0, 0, Math.Sin(expectedHalf));
            expected.Unitize();

            double dot = Math.Abs(mid.A * expected.A + mid.B * expected.B + mid.C * expected.C + mid.D * expected.D);
            Assert.True(Math.Abs(dot - 1.0) < Tolerance,
                $"Slerp(t=0.5) should be midpoint rotation. Got ({mid.A}, {mid.B}, {mid.C}, {mid.D})");
        }

        [Fact]
        public void Slerp_ClampsNegativeT_ToZero()
        {
            Quaternion q1 = new Quaternion(1, 0, 0, 0);
            Quaternion q2 = new Quaternion(0, 1, 0, 0);

            Quaternion atNeg = HelperMethods.Slerp(q1, q2, -0.5);
            Quaternion atZero = HelperMethods.Slerp(q1, q2, 0.0);

            Assert.True(Math.Abs(atNeg.A - atZero.A) < Tolerance &&
                        Math.Abs(atNeg.B - atZero.B) < Tolerance &&
                        Math.Abs(atNeg.C - atZero.C) < Tolerance &&
                        Math.Abs(atNeg.D - atZero.D) < Tolerance,
                "Slerp(t<0) should clamp to t=0");
        }

        [Fact]
        public void Slerp_ClampsAboveOneT_ToOne()
        {
            Quaternion q1 = new Quaternion(1, 0, 0, 0);
            Quaternion q2 = new Quaternion(0, 1, 0, 0);

            Quaternion atAbove = HelperMethods.Slerp(q1, q2, 1.5);
            Quaternion atOne = HelperMethods.Slerp(q1, q2, 1.0);

            Assert.True(Math.Abs(atAbove.A - atOne.A) < Tolerance &&
                        Math.Abs(atAbove.B - atOne.B) < Tolerance &&
                        Math.Abs(atAbove.C - atOne.C) < Tolerance &&
                        Math.Abs(atAbove.D - atOne.D) < Tolerance,
                "Slerp(t>1) should clamp to t=1");
        }
        #endregion

        #region Lerp
        [Fact]
        public void Lerp_AtZero_ReturnsFirstQuaternion()
        {
            Quaternion q1 = new Quaternion(1, 0, 0, 0);
            Quaternion q2 = new Quaternion(0, 1, 0, 0);

            Quaternion result = HelperMethods.Lerp(q1, q2, 0.0);

            Assert.True(Math.Abs(result.A - q1.A) < Tolerance &&
                        Math.Abs(result.B - q1.B) < Tolerance &&
                        Math.Abs(result.C - q1.C) < Tolerance &&
                        Math.Abs(result.D - q1.D) < Tolerance,
                $"Lerp(t=0) should return q1. Got ({result.A}, {result.B}, {result.C}, {result.D})");
        }

        [Fact]
        public void Lerp_AtOne_ReturnsSecondQuaternion()
        {
            Quaternion q1 = new Quaternion(1, 0, 0, 0);
            Quaternion q2 = new Quaternion(0, 1, 0, 0);

            Quaternion result = HelperMethods.Lerp(q1, q2, 1.0);

            // The result may be q2 or -q2 (sign alignment)
            double dot = Math.Abs(result.A * q2.A + result.B * q2.B + result.C * q2.C + result.D * q2.D);
            Assert.True(Math.Abs(dot - 1.0) < Tolerance,
                $"Lerp(t=1) should return q2 (or -q2). Got ({result.A}, {result.B}, {result.C}, {result.D})");
        }

        [Fact]
        public void Lerp_ClampsNegativeT_ToZero()
        {
            Quaternion q1 = new Quaternion(1, 0, 0, 0);
            Quaternion q2 = new Quaternion(0, 1, 0, 0);

            Quaternion atNeg = HelperMethods.Lerp(q1, q2, -0.5);
            Quaternion atZero = HelperMethods.Lerp(q1, q2, 0.0);

            Assert.True(Math.Abs(atNeg.A - atZero.A) < Tolerance &&
                        Math.Abs(atNeg.B - atZero.B) < Tolerance &&
                        Math.Abs(atNeg.C - atZero.C) < Tolerance &&
                        Math.Abs(atNeg.D - atZero.D) < Tolerance,
                "Lerp(t<0) should clamp to t=0");
        }

        [Fact]
        public void Lerp_ClampsAboveOneT_ToOne()
        {
            Quaternion q1 = new Quaternion(1, 0, 0, 0);
            Quaternion q2 = new Quaternion(0, 1, 0, 0);

            Quaternion atAbove = HelperMethods.Lerp(q1, q2, 1.5);
            Quaternion atOne = HelperMethods.Lerp(q1, q2, 1.0);

            Assert.True(Math.Abs(atAbove.A - atOne.A) < Tolerance &&
                        Math.Abs(atAbove.B - atOne.B) < Tolerance &&
                        Math.Abs(atAbove.C - atOne.C) < Tolerance &&
                        Math.Abs(atAbove.D - atOne.D) < Tolerance,
                "Lerp(t>1) should clamp to t=1");
        }
        #endregion

        #region DotProduct
        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void DotProduct_IdenticalQuaternions_ReturnsOne()
        {
            Quaternion q = new Quaternion(1, 0, 0, 0);

            double dot = q.DotProduct(q);

            Assert.True(Math.Abs(dot - 1.0) < Tolerance,
                $"Dot product of identical unit quaternions should be 1, got {dot}");
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void DotProduct_OrthogonalQuaternions_ReturnsZero()
        {
            // Two orthogonal unit quaternions
            Quaternion q1 = new Quaternion(1, 0, 0, 0);
            Quaternion q2 = new Quaternion(0, 1, 0, 0);

            double dot = q1.DotProduct(q2);

            Assert.True(Math.Abs(dot) < Tolerance,
                $"Dot product of orthogonal quaternions should be 0, got {dot}");
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void DotProduct_OppositeQuaternions_ReturnsNegativeOne()
        {
            Quaternion q1 = new Quaternion(1, 0, 0, 0);
            Quaternion q2 = new Quaternion(-1, 0, 0, 0);

            double dot = q1.DotProduct(q2);

            Assert.True(Math.Abs(dot + 1.0) < Tolerance,
                $"Dot product of opposite quaternions should be -1, got {dot}");
        }
        #endregion

        #region ReplaceFirst
        [Fact]
        public void ReplaceFirst_ReplacesOnlyFirstOccurrence()
        {
            string input = "abc abc abc";

            string result = input.ReplaceFirst("abc", "xyz");

            Assert.Equal("xyz abc abc", result);
        }

        [Fact]
        public void ReplaceFirst_NoMatch_ReturnsOriginal()
        {
            string input = "hello world";

            string result = input.ReplaceFirst("xyz", "replaced");

            Assert.Equal("hello world", result);
        }

        [Fact]
        public void ReplaceFirst_EmptySearch_ReplacesAtStart()
        {
            string input = "hello";

            string result = input.ReplaceFirst("", "prefix");

            Assert.Equal("prefixhello", result);
        }

        [Fact]
        public void ReplaceFirst_ReplaceWithEmpty_RemovesFirstOccurrence()
        {
            string input = "VAR speeddata v100";

            string result = input.ReplaceFirst("VAR", "");

            Assert.Equal(" speeddata v100", result);
        }
        #endregion

        #region SetRapidDataFromString
        [Fact]
        public void SetRapidDataFromString_ParsesVarSpeedData()
        {
            var stub = new DeclarationStub("speeddata");

            HelperMethods.SetRapidDataFromString(stub, "VAR speeddata v100 := [100, 500, 5000, 1000];", out string[] values);

            Assert.Equal(Scope.GLOBAL, stub.Scope);
            Assert.Equal(VariableType.VAR, stub.VariableType);
            Assert.Equal("v100", stub.Name);
            Assert.Equal(4, values.Length);
            Assert.Equal("100", values[0]);
            Assert.Equal("500", values[1]);
            Assert.Equal("5000", values[2]);
            Assert.Equal("1000", values[3]);
        }

        [Fact]
        public void SetRapidDataFromString_ParsesLocalScope()
        {
            var stub = new DeclarationStub("speeddata");

            HelperMethods.SetRapidDataFromString(stub, "LOCAL VAR speeddata mySpeed := [50, 250, 2500, 500];", out string[] values);

            Assert.Equal(Scope.LOCAL, stub.Scope);
            Assert.Equal(VariableType.VAR, stub.VariableType);
            Assert.Equal("mySpeed", stub.Name);
        }

        [Fact]
        public void SetRapidDataFromString_ParsesTaskScope()
        {
            var stub = new DeclarationStub("speeddata");

            HelperMethods.SetRapidDataFromString(stub, "TASK PERS speeddata taskSpeed := [100, 500, 5000, 1000];", out string[] values);

            Assert.Equal(Scope.TASK, stub.Scope);
            Assert.Equal(VariableType.PERS, stub.VariableType);
            Assert.Equal("taskSpeed", stub.Name);
        }

        [Fact]
        public void SetRapidDataFromString_ParsesConstVariableType()
        {
            var stub = new DeclarationStub("speeddata");

            HelperMethods.SetRapidDataFromString(stub, "CONST speeddata constSpeed := [100, 500, 5000, 1000];", out string[] values);

            Assert.Equal(Scope.GLOBAL, stub.Scope);
            Assert.Equal(VariableType.CONST, stub.VariableType);
            Assert.Equal("constSpeed", stub.Name);
        }

        [Fact]
        public void SetRapidDataFromString_MultipleEqualSigns_ThrowsInvalidCastException()
        {
            var stub = new DeclarationStub("speeddata");

            Assert.Throws<InvalidCastException>(() =>
                HelperMethods.SetRapidDataFromString(stub, "VAR speeddata v := a := b;", out string[] _));
        }

        [Fact]
        public void SetRapidDataFromString_WrongDatatype_ThrowsInvalidCastException()
        {
            var stub = new DeclarationStub("speeddata");

            Assert.Throws<InvalidCastException>(() =>
                HelperMethods.SetRapidDataFromString(stub, "VAR zonedata z10 := [FALSE, 10, 15, 15, 1.5, 15, 1.5];", out string[] _));
        }
        #endregion

        #region test helpers
        /// <summary>
        /// Minimal IDeclaration stub for testing SetRapidDataFromString.
        /// </summary>
        private class DeclarationStub : IDeclaration
        {
            private readonly string _datatype;

            public DeclarationStub(string datatype)
            {
                _datatype = datatype;
            }

            public bool IsValid => true;
            public Scope Scope { get; set; }
            public VariableType VariableType { get; set; }
            public string Datatype => _datatype;
            public string Name { get; set; }

            public IDeclaration DuplicateDeclaration() => new DeclarationStub(_datatype);
            public string ToRAPID() => "";
            public string ToRAPIDDeclaration(Robot robot) => "";
            public void ToRAPIDGenerator(RAPIDGenerator RAPIDGenerator) { }
        }
        #endregion
    }
}
