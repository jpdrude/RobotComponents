// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components
// Project: https://github.com/RobotComponents/RobotComponents
//
// For license details, see the LICENSE file in the project root.

// Xunit Libs
using Xunit;
// Robot Components Libs
using RobotComponents.ABB.Actions.Declarations;
using RobotComponents.ABB.Enumerations;

namespace RobotComponents.Tests.Actions
{
    public class ZoneDataTests
    {
        #region Predefined Constructor
        [Fact]
        public void Constructor_PredefinedFine_SetsFinePointTrue()
        {
            ZoneData zd = new ZoneData(-1);

            Assert.True(zd.FinePoint);
            Assert.Equal("fine", zd.Name);
            Assert.True(zd.IsPreDefined);
            Assert.True(zd.IsExactPredefinedValue);
        }

        [Fact]
        public void Constructor_PredefinedZ10_SetsCorrectValues()
        {
            // z10 is at index 4 in the predefined arrays
            ZoneData zd = new ZoneData(10);

            Assert.Equal("z10", zd.Name);
            Assert.False(zd.FinePoint);
            Assert.Equal(10, zd.PathZoneTCP);
            Assert.Equal(15, zd.PathZoneOrientation);
            Assert.Equal(15, zd.PathZoneExternalAxes);
            Assert.Equal(1.5, zd.ZoneOrientation);
            Assert.Equal(15, zd.ZoneExternalLinearAxes);
            Assert.Equal(1.5, zd.ZoneExternalRotationalAxes);
            Assert.True(zd.IsPreDefined);
            Assert.True(zd.IsExactPredefinedValue);
        }

        [Fact]
        public void Constructor_PredefinedZ0_SetsFlyByPoint()
        {
            ZoneData zd = new ZoneData(0);

            Assert.False(zd.FinePoint);
            Assert.Equal("z0", zd.Name);
            Assert.Equal(0.3, zd.PathZoneTCP);
        }

        [Fact]
        public void Constructor_PredefinedNearest8_SnapsToZ10()
        {
            // |8-5| = 3 > |8-10| = 2 → snaps to z10
            ZoneData zd = new ZoneData(8.0);

            Assert.Equal("z10", zd.Name);
            Assert.False(zd.IsExactPredefinedValue);
        }

        [Fact]
        public void Constructor_IntOverload_MatchesDoubleOverload()
        {
            ZoneData fromInt = new ZoneData((int)10);
            ZoneData fromDouble = new ZoneData(10.0);

            Assert.Equal(fromDouble.Name, fromInt.Name);
            Assert.Equal(fromDouble.PathZoneTCP, fromInt.PathZoneTCP);
        }
        #endregion

        #region Custom Constructor
        [Fact]
        public void Constructor_CustomWithName_SetsValues()
        {
            ZoneData zd = new ZoneData("myZone", false, 5, 10, 10, 1, 10, 1);

            Assert.Equal("myZone", zd.Name);
            Assert.False(zd.FinePoint);
            Assert.False(zd.IsPreDefined);
            Assert.Equal(5, zd.PathZoneTCP);
            Assert.Equal(10, zd.PathZoneOrientation);
            Assert.Equal(10, zd.PathZoneExternalAxes);
            Assert.Equal(1, zd.ZoneOrientation);
            Assert.Equal(10, zd.ZoneExternalLinearAxes);
            Assert.Equal(1, zd.ZoneExternalRotationalAxes);
        }

        [Fact]
        public void Constructor_CustomNoName_SetsValues()
        {
            ZoneData zd = new ZoneData(false, 5, 10, 10, 1, 10, 1);

            Assert.Equal("", zd.Name);
            Assert.False(zd.IsPreDefined);
        }

        [Fact]
        public void Constructor_CustomFinePoint_SetsFinep()
        {
            ZoneData zd = new ZoneData("myFine", true);

            Assert.True(zd.FinePoint);
            Assert.False(zd.IsPreDefined);
        }
        #endregion

        #region ToRAPID
        [Fact]
        public void ToRAPID_FinePoint_ReturnsTrueFirst()
        {
            ZoneData zd = new ZoneData(-1);

            Assert.StartsWith("[TRUE, ", zd.ToRAPID());
        }

        [Fact]
        public void ToRAPID_FlyByPoint_ReturnsFalseFirst()
        {
            ZoneData zd = new ZoneData(10);

            Assert.StartsWith("[FALSE, ", zd.ToRAPID());
        }

        [Fact]
        public void ToRAPID_Fine_Returns7ElementArray()
        {
            ZoneData zd = new ZoneData(-1);

            Assert.Equal("[TRUE, 0, 0, 0, 0, 0, 0]", zd.ToRAPID());
        }

        [Fact]
        public void ToRAPID_Z10_Returns7ElementArray()
        {
            ZoneData zd = new ZoneData(10);

            Assert.Equal("[FALSE, 10, 15, 15, 1.5, 15, 1.5]", zd.ToRAPID());
        }

        [Fact]
        public void ToRAPID_Z0_Returns7ElementArray()
        {
            ZoneData zd = new ZoneData(0);

            Assert.Equal("[FALSE, 0.3, 0.3, 0.3, 0.03, 0.3, 0.03]", zd.ToRAPID());
        }
        #endregion

        #region ToRAPIDDeclaration
        [Fact]
        public void ToRAPIDDeclaration_CustomWithName_ReturnsVarDeclaration()
        {
            ZoneData zd = new ZoneData("myZone", false, 5, 10, 10, 1, 10, 1);

            string result = zd.ToRAPIDDeclaration(null);

            Assert.Equal("VAR zonedata myZone := [FALSE, 5, 10, 10, 1, 10, 1];", result);
        }

        [Fact]
        public void ToRAPIDDeclaration_Predefined_ReturnsEmpty()
        {
            ZoneData zd = new ZoneData(10);

            Assert.Equal(string.Empty, zd.ToRAPIDDeclaration(null));
        }

        [Fact]
        public void ToRAPIDDeclaration_CustomNoName_ReturnsEmpty()
        {
            ZoneData zd = new ZoneData(false, 5, 10, 10, 1, 10, 1);

            Assert.Equal(string.Empty, zd.ToRAPIDDeclaration(null));
        }

        [Fact]
        public void ToRAPIDDeclaration_LocalScope_IncludesScopePrefix()
        {
            ZoneData zd = new ZoneData("myZone", false, 5, 10, 10, 1, 10, 1);
            zd.Scope = Scope.LOCAL;

            string result = zd.ToRAPIDDeclaration(null);

            Assert.StartsWith("LOCAL ", result);
        }
        #endregion

        #region ToRAPIDInstruction
        [Fact]
        public void ToRAPIDInstruction_ReturnsEmpty()
        {
            ZoneData zd = new ZoneData(10);

            Assert.Equal(string.Empty, zd.ToRAPIDInstruction(null));
        }
        #endregion

        #region IsValid
        [Fact]
        public void IsValid_PredefinedZ10_ReturnsTrue()
        {
            ZoneData zd = new ZoneData(10);

            Assert.True(zd.IsValid);
        }

        [Fact]
        public void IsValid_FinePoint_ReturnsTrue()
        {
            ZoneData zd = new ZoneData(-1);

            Assert.True(zd.IsValid);
        }

        [Fact]
        public void IsValid_ZeroPathZone_ReturnsTrue()
        {
            ZoneData zd = new ZoneData("z", false, 0, 0, 0, 0, 0, 0);

            Assert.True(zd.IsValid);
        }

        [Fact]
        public void IsValid_NegativePathZone_ReturnsFalse()
        {
            ZoneData zd = new ZoneData("z", false, -1, 0, 0, 0, 0, 0);

            Assert.False(zd.IsValid);
        }
        #endregion

        #region Duplicate
        [Fact]
        public void Duplicate_ReturnsMatchingValues()
        {
            ZoneData original = new ZoneData("myZone", false, 5, 10, 10, 1, 10, 1);
            original.Scope = Scope.LOCAL;

            ZoneData copy = original.Duplicate();

            Assert.Equal(original.Name, copy.Name);
            Assert.Equal(original.FinePoint, copy.FinePoint);
            Assert.Equal(original.PathZoneTCP, copy.PathZoneTCP);
            Assert.Equal(original.PathZoneOrientation, copy.PathZoneOrientation);
            Assert.Equal(original.ZoneOrientation, copy.ZoneOrientation);
            Assert.Equal(original.Scope, copy.Scope);
        }
        #endregion

        #region Parse / TryParse
        [Fact]
        public void Parse_ValidRapidString_ReturnsCorrectValues()
        {
            ZoneData zd = ZoneData.Parse("VAR zonedata myZone := [FALSE, 5, 10, 10, 1, 10, 1];");

            Assert.Equal("myZone", zd.Name);
            Assert.False(zd.FinePoint);
            Assert.Equal(5, zd.PathZoneTCP);
            Assert.Equal(10, zd.PathZoneOrientation);
        }

        [Fact]
        public void Parse_FineZoneString_ReturnsFinePoint()
        {
            ZoneData zd = ZoneData.Parse("VAR zonedata fine := [TRUE, 0, 0, 0, 0, 0, 0];");

            Assert.True(zd.FinePoint);
            Assert.True(zd.IsPreDefined);
        }

        [Fact]
        public void TryParse_InvalidString_ReturnsFalse()
        {
            bool result = ZoneData.TryParse("garbage", out ZoneData zd);

            Assert.False(result);
        }

        [Fact]
        public void TryParse_WrongDatatype_ReturnsFalse()
        {
            bool result = ZoneData.TryParse("VAR speeddata v100 := [100, 500, 5000, 1000];", out ZoneData zd);

            Assert.False(result);
        }

        [Fact]
        public void TryParse_TooFewValues_ReturnsFalse()
        {
            bool result = ZoneData.TryParse("VAR zonedata z := [FALSE, 5, 10];", out ZoneData zd);

            Assert.False(result);
        }

        [Fact]
        public void Parse_ConstVariableType_SetsConst()
        {
            ZoneData zd = ZoneData.Parse("CONST zonedata myZone := [FALSE, 5, 10, 10, 1, 10, 1];");

            Assert.Equal(VariableType.CONST, zd.VariableType);
            Assert.Equal("myZone", zd.Name);
        }

        [Fact]
        public void Parse_LocalScope_SetsLocalScope()
        {
            ZoneData zd = ZoneData.Parse("LOCAL VAR zonedata myZone := [FALSE, 5, 10, 10, 1, 10, 1];");

            Assert.Equal(Scope.LOCAL, zd.Scope);
            Assert.Equal("myZone", zd.Name);
        }
        #endregion

        #region Static Helpers
        [Fact]
        public void ValidPredefinedNames_Contains15Entries()
        {
            Assert.Equal(15, ZoneData.ValidPredefinedNames.Length);
        }

        [Fact]
        public void GetPredefinedZoneData_FromEnum_MatchesConstructor()
        {
            ZoneData fromEnum = ZoneData.GetPredefinedZoneData(PredefinedZoneData.z10);
            ZoneData fromCtor = new ZoneData(10);

            Assert.Equal(fromCtor.Name, fromEnum.Name);
            Assert.Equal(fromCtor.PathZoneTCP, fromEnum.PathZoneTCP);
        }
        #endregion
    }
}
