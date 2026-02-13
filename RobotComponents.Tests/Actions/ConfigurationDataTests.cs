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
    public class ConfigurationDataTests
    {
        #region Constructor
        [Fact]
        public void Constructor_FourInts_SetsFields()
        {
            ConfigurationData cd = new ConfigurationData(1, 2, 3, 4);

            Assert.Equal(1, cd.Cf1);
            Assert.Equal(2, cd.Cf4);
            Assert.Equal(3, cd.Cf6);
            Assert.Equal(4, cd.Cfx);
        }

        [Fact]
        public void Constructor_Default_AllZero()
        {
            ConfigurationData cd = new ConfigurationData();

            Assert.Equal(0, cd.Cf1);
            Assert.Equal(0, cd.Cf4);
            Assert.Equal(0, cd.Cf6);
            Assert.Equal(0, cd.Cfx);
        }

        [Fact]
        public void Constructor_Named_SetsNameAndValues()
        {
            ConfigurationData cd = new ConfigurationData("conf1", 1, 2, 3, 4);

            Assert.Equal("conf1", cd.Name);
            Assert.Equal(1, cd.Cf1);
            Assert.Equal(4, cd.Cfx);
        }
        #endregion

        #region ToRAPID
        [Fact]
        public void ToRAPID_ReturnsCorrectBracketFormat()
        {
            ConfigurationData cd = new ConfigurationData(1, 2, 3, 4);

            Assert.Equal("[1, 2, 3, 4]", cd.ToRAPID());
        }

        [Fact]
        public void ToRAPID_Zeros_ReturnsZeroBrackets()
        {
            ConfigurationData cd = new ConfigurationData();

            Assert.Equal("[0, 0, 0, 0]", cd.ToRAPID());
        }

        [Fact]
        public void ToRAPID_NegativeValues_FormatsCorrectly()
        {
            ConfigurationData cd = new ConfigurationData(-1, 0, 1, -2);

            Assert.Equal("[-1, 0, 1, -2]", cd.ToRAPID());
        }
        #endregion

        #region ToRAPIDDeclaration
        [Fact]
        public void ToRAPIDDeclaration_Named_ReturnsConstConfdataDeclaration()
        {
            // Default VariableType is CONST for ConfigurationData
            ConfigurationData cd = new ConfigurationData("conf1", 1, 2, 3, 4);

            Assert.Equal("CONST confdata conf1 := [1, 2, 3, 4];", cd.ToRAPIDDeclaration(null));
        }

        [Fact]
        public void ToRAPIDDeclaration_Unnamed_ReturnsEmpty()
        {
            ConfigurationData cd = new ConfigurationData(1, 2, 3, 4);

            Assert.Equal(string.Empty, cd.ToRAPIDDeclaration(null));
        }

        [Fact]
        public void ToRAPIDDeclaration_VarType_UsesVarKeyword()
        {
            ConfigurationData cd = new ConfigurationData("conf1", 1, 2, 3, 4);
            cd.VariableType = VariableType.VAR;

            Assert.Contains("VAR confdata", cd.ToRAPIDDeclaration(null));
        }
        #endregion

        #region ToRAPIDInstruction
        [Fact]
        public void ToRAPIDInstruction_ReturnsEmpty()
        {
            ConfigurationData cd = new ConfigurationData("conf1", 1, 2, 3, 4);

            Assert.Equal(string.Empty, cd.ToRAPIDInstruction(null));
        }
        #endregion

        #region Duplicate
        [Fact]
        public void Duplicate_ReturnsMatchingValues()
        {
            ConfigurationData original = new ConfigurationData("conf1", 1, 2, 3, 4);
            original.Scope = Scope.LOCAL;
            original.VariableType = VariableType.VAR;

            ConfigurationData copy = original.Duplicate();

            Assert.Equal(original.Name, copy.Name);
            Assert.Equal(original.Cf1, copy.Cf1);
            Assert.Equal(original.Cf4, copy.Cf4);
            Assert.Equal(original.Cf6, copy.Cf6);
            Assert.Equal(original.Cfx, copy.Cfx);
            Assert.Equal(original.Scope, copy.Scope);
            Assert.Equal(original.VariableType, copy.VariableType);
        }
        #endregion

        #region Parse / TryParse
        [Fact]
        public void Parse_ValidRapidString_ReturnsCorrectValues()
        {
            ConfigurationData cd = ConfigurationData.Parse("CONST confdata conf1 := [1, 2, 3, 4];");

            Assert.Equal("conf1", cd.Name);
            Assert.Equal(1, cd.Cf1);
            Assert.Equal(2, cd.Cf4);
            Assert.Equal(3, cd.Cf6);
            Assert.Equal(4, cd.Cfx);
        }

        [Fact]
        public void TryParse_InvalidString_ReturnsFalse()
        {
            bool result = ConfigurationData.TryParse("garbage", out ConfigurationData cd);

            Assert.False(result);
        }
        #endregion
    }
}
