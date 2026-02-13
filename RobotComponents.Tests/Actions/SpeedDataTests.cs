// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components
// Project: https://github.com/RobotComponents/RobotComponents
//
// For license details, see the LICENSE file in the project root.

// System Libs
using System.Collections.Generic;
using System.Linq;
// Xunit Libs
using Xunit;
// Robot Components Libs
using RobotComponents.ABB.Actions.Declarations;
using RobotComponents.ABB.Enumerations;

namespace RobotComponents.Tests.Actions
{
    public class SpeedDataTests
    {
        #region Predefined Constructor
        [Fact]
        public void Constructor_PredefinedExact5_SetsNameV5AndTcp5()
        {
            SpeedData sd = new SpeedData(5);

            Assert.Equal("v5", sd.Name);
            Assert.Equal(5, sd.V_TCP);
            Assert.True(sd.IsPreDefined);
            Assert.True(sd.IsExactPredefinedValue);
        }

        [Fact]
        public void Constructor_PredefinedExact100_SetsCorrectDefaults()
        {
            SpeedData sd = new SpeedData(100);

            Assert.Equal("v100", sd.Name);
            Assert.Equal(100, sd.V_TCP);
            Assert.Equal(500, sd.V_ORI);
            Assert.Equal(5000, sd.V_LEAX);
            Assert.Equal(1000, sd.V_REAX);
        }

        [Fact]
        public void Constructor_PredefinedNearest25_SnapsToV30()
        {
            // |25-20| == |25-30| == 5, tie goes to v30
            SpeedData sd = new SpeedData(25.0);

            Assert.Equal("v30", sd.Name);
            Assert.Equal(30, sd.V_TCP);
            Assert.True(sd.IsPreDefined);
            Assert.False(sd.IsExactPredefinedValue);
        }

        [Fact]
        public void Constructor_PredefinedNearest7_SnapsToV5()
        {
            // |7-5| = 2 < |7-10| = 3
            SpeedData sd = new SpeedData(7.0);

            Assert.Equal("v5", sd.Name);
            Assert.Equal(5, sd.V_TCP);
        }

        [Fact]
        public void Constructor_IntOverload_MatchesDoubleOverload()
        {
            SpeedData fromInt = new SpeedData((int)100);
            SpeedData fromDouble = new SpeedData(100.0);

            Assert.Equal(fromDouble.Name, fromInt.Name);
            Assert.Equal(fromDouble.V_TCP, fromInt.V_TCP);
            Assert.Equal(fromDouble.IsPreDefined, fromInt.IsPreDefined);
        }
        #endregion

        #region Custom Constructor
        [Fact]
        public void Constructor_Custom4Args_SetsCustomValues()
        {
            SpeedData sd = new SpeedData(200.0, 600, 4000, 800);

            Assert.False(sd.IsPreDefined);
            Assert.False(sd.IsExactPredefinedValue);
            Assert.Equal("", sd.Name);
            Assert.Equal(200, sd.V_TCP);
            Assert.Equal(600, sd.V_ORI);
            Assert.Equal(4000, sd.V_LEAX);
            Assert.Equal(800, sd.V_REAX);
        }

        [Fact]
        public void Constructor_Custom4Args_DefaultOptionalArgs()
        {
            SpeedData sd = new SpeedData(200.0, 500, 5000, 1000);

            Assert.Equal(500, sd.V_ORI);
            Assert.Equal(5000, sd.V_LEAX);
            Assert.Equal(1000, sd.V_REAX);
        }

        [Fact]
        public void Constructor_Custom5Args_SetsNameAndValues()
        {
            SpeedData sd = new SpeedData("custom", 200, 500, 5000, 1000);

            Assert.Equal("custom", sd.Name);
            Assert.Equal(200, sd.V_TCP);
            Assert.False(sd.IsPreDefined);
        }
        #endregion

        #region ToRAPID
        [Fact]
        public void ToRAPID_CustomValues_ReturnsCorrectBracketFormat()
        {
            SpeedData sd = new SpeedData("s", 200, 500, 5000, 1000);

            Assert.Equal("[200, 500, 5000, 1000]", sd.ToRAPID());
        }

        [Fact]
        public void ToRAPID_PredefinedV100_ReturnsCorrectBracketFormat()
        {
            SpeedData sd = new SpeedData(100);

            Assert.Equal("[100, 500, 5000, 1000]", sd.ToRAPID());
        }
        #endregion

        #region ToRAPIDDeclaration
        [Fact]
        public void ToRAPIDDeclaration_CustomWithName_ReturnsVarDeclaration()
        {
            SpeedData sd = new SpeedData("custom", 200, 500, 5000, 1000);

            Assert.Equal("VAR speeddata custom := [200, 500, 5000, 1000];", sd.ToRAPIDDeclaration(null));
        }

        [Fact]
        public void ToRAPIDDeclaration_Predefined_ReturnsEmpty()
        {
            SpeedData sd = new SpeedData(100);

            Assert.Equal(string.Empty, sd.ToRAPIDDeclaration(null));
        }

        [Fact]
        public void ToRAPIDDeclaration_CustomNoName_ReturnsEmpty()
        {
            SpeedData sd = new SpeedData(200.0, 500, 5000, 1000);

            Assert.Equal(string.Empty, sd.ToRAPIDDeclaration(null));
        }

        [Fact]
        public void ToRAPIDDeclaration_LocalScope_IncludesScopePrefix()
        {
            SpeedData sd = new SpeedData("mySpeed", 200, 500, 5000, 1000);
            sd.Scope = Scope.LOCAL;

            string result = sd.ToRAPIDDeclaration(null);

            Assert.StartsWith("LOCAL ", result);
            Assert.Contains("VAR speeddata mySpeed", result);
        }

        [Fact]
        public void ToRAPIDDeclaration_TaskScope_IncludesScopePrefix()
        {
            SpeedData sd = new SpeedData("mySpeed", 200, 500, 5000, 1000);
            sd.Scope = Scope.TASK;

            string result = sd.ToRAPIDDeclaration(null);

            Assert.StartsWith("TASK ", result);
        }

        [Fact]
        public void ToRAPIDDeclaration_PersVariableType_UsesCorrectKeyword()
        {
            SpeedData sd = new SpeedData("mySpeed", 200, 500, 5000, 1000);
            sd.VariableType = VariableType.PERS;

            string result = sd.ToRAPIDDeclaration(null);

            Assert.Contains("PERS speeddata", result);
        }
        #endregion

        #region ToRAPIDInstruction
        [Fact]
        public void ToRAPIDInstruction_ReturnsEmpty()
        {
            SpeedData sd = new SpeedData(100);

            Assert.Equal(string.Empty, sd.ToRAPIDInstruction(null));
        }
        #endregion

        #region IsValid
        [Fact]
        public void IsValid_AllPositive_ReturnsTrue()
        {
            SpeedData sd = new SpeedData("s", 1, 1, 1, 1);

            Assert.True(sd.IsValid);
        }

        [Fact]
        public void IsValid_ZeroTcp_ReturnsFalse()
        {
            SpeedData sd = new SpeedData("s", 0, 1, 1, 1);

            Assert.False(sd.IsValid);
        }

        [Fact]
        public void IsValid_NegativeOri_ReturnsFalse()
        {
            SpeedData sd = new SpeedData("s", 100, 500, 5000, 1000);
            sd.V_ORI = -1;

            Assert.False(sd.IsValid);
        }

        [Fact]
        public void IsValid_ZeroLeax_ReturnsFalse()
        {
            SpeedData sd = new SpeedData("s", 100, 500, 0, 1000);

            Assert.False(sd.IsValid);
        }
        #endregion

        #region Duplicate
        [Fact]
        public void Duplicate_CustomSpeed_ReturnsSameValues()
        {
            SpeedData original = new SpeedData("mySpeed", 300, 600, 4000, 800);
            original.Scope = Scope.LOCAL;
            original.VariableType = VariableType.PERS;

            SpeedData copy = original.Duplicate();

            Assert.Equal(original.Name, copy.Name);
            Assert.Equal(original.V_TCP, copy.V_TCP);
            Assert.Equal(original.V_ORI, copy.V_ORI);
            Assert.Equal(original.V_LEAX, copy.V_LEAX);
            Assert.Equal(original.V_REAX, copy.V_REAX);
            Assert.Equal(original.IsPreDefined, copy.IsPreDefined);
            Assert.Equal(original.Scope, copy.Scope);
            Assert.Equal(original.VariableType, copy.VariableType);
        }
        #endregion

        #region Parse / TryParse
        [Fact]
        public void Parse_ValidRapidString_ReturnsCorrectValues()
        {
            SpeedData sd = SpeedData.Parse("VAR speeddata v100 := [100, 500, 5000, 1000];");

            Assert.Equal("v100", sd.Name);
            Assert.Equal(100, sd.V_TCP);
            Assert.Equal(500, sd.V_ORI);
            Assert.Equal(5000, sd.V_LEAX);
            Assert.Equal(1000, sd.V_REAX);
            Assert.True(sd.IsPreDefined);
        }

        [Fact]
        public void Parse_CustomRapidString_ReturnsCorrectValues()
        {
            SpeedData sd = SpeedData.Parse("VAR speeddata mySpeed := [250, 600, 4000, 800];");

            Assert.Equal("mySpeed", sd.Name);
            Assert.Equal(250, sd.V_TCP);
            Assert.Equal(600, sd.V_ORI);
            Assert.Equal(4000, sd.V_LEAX);
            Assert.Equal(800, sd.V_REAX);
            Assert.False(sd.IsPreDefined);
        }

        [Fact]
        public void TryParse_InvalidString_ReturnsFalse()
        {
            bool result = SpeedData.TryParse("garbage", out SpeedData sd);

            Assert.False(result);
        }
        #endregion

        #region Static Helpers
        [Fact]
        public void ValidPredefinedNames_Contains25Entries()
        {
            Assert.Equal(25, SpeedData.ValidPredefinedNames.Length);
        }

        [Fact]
        public void ValidPredefinedValues_MatchesNames()
        {
            string[] names = SpeedData.ValidPredefinedNames;
            double[] values = SpeedData.ValidPredefinedValues;

            Assert.Equal(names.Length, values.Length);

            for (int i = 0; i < names.Length; i++)
            {
                Assert.Equal($"v{values[i]}", names[i]);
            }
        }

        [Fact]
        public void ValidPredefinedData_MatchesNamesAndValues()
        {
            Dictionary<string, double> data = SpeedData.ValidPredefinedData;

            Assert.Equal(25, data.Count);
            Assert.Equal(100, data["v100"]);
            Assert.Equal(5, data["v5"]);
        }

        [Fact]
        public void GetPredefinedSpeedData_FromEnum_MatchesConstructor()
        {
            SpeedData fromEnum = SpeedData.GetPredefinedSpeedData(PredefinedSpeedData.v100);
            SpeedData fromCtor = new SpeedData(100);

            Assert.Equal(fromCtor.Name, fromEnum.Name);
            Assert.Equal(fromCtor.V_TCP, fromEnum.V_TCP);
        }
        #endregion
    }
}
