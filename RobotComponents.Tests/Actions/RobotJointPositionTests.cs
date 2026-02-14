// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components
// Project: https://github.com/RobotComponents/RobotComponents
//
// For license details, see the LICENSE file in the project root.

// System Libs
using System;
using System.Collections.Generic;
// Xunit Libs
using Xunit;
// Robot Components Libs
using RobotComponents.ABB.Actions.Declarations;
using RobotComponents.ABB.Enumerations;

namespace RobotComponents.Tests.Actions
{
    public class RobotJointPositionTests
    {
        #region Constructor
        [Fact]
        public void Constructor_Default_AllZero()
        {
            RobotJointPosition rjp = new RobotJointPosition();

            for (int i = 0; i < 6; i++)
            {
                Assert.Equal(0, rjp[i]);
            }
        }

        [Fact]
        public void Constructor_SixArgs_SetsValues()
        {
            RobotJointPosition rjp = new RobotJointPosition(10, 20, 30, 40, 50, 60);

            Assert.Equal(10, rjp[0]);
            Assert.Equal(20, rjp[1]);
            Assert.Equal(30, rjp[2]);
            Assert.Equal(40, rjp[3]);
            Assert.Equal(50, rjp[4]);
            Assert.Equal(60, rjp[5]);
        }

        [Fact]
        public void Constructor_Named_SetsNameAndValues()
        {
            RobotJointPosition rjp = new RobotJointPosition("rj1", 10, 20, 30, 40, 50, 60);

            Assert.Equal("rj1", rjp.Name);
            Assert.Equal(10, rjp[0]);
            Assert.Equal(60, rjp[5]);
        }

        [Fact]
        public void Constructor_NaN_ReplacedWithZero()
        {
            RobotJointPosition rjp = new RobotJointPosition(double.NaN);

            Assert.Equal(0, rjp[0]);
        }

        [Fact]
        public void Constructor_ListOfDoubles_SetsValues()
        {
            RobotJointPosition rjp = new RobotJointPosition(new List<double> { 10, 20, 30, 40, 50, 60 });

            Assert.Equal(10, rjp[0]);
            Assert.Equal(20, rjp[1]);
            Assert.Equal(30, rjp[2]);
            Assert.Equal(40, rjp[3]);
            Assert.Equal(50, rjp[4]);
            Assert.Equal(60, rjp[5]);
        }
        #endregion

        #region ToRAPID
        [Fact]
        public void ToRAPID_AllZero_ReturnsBracketFormat()
        {
            RobotJointPosition rjp = new RobotJointPosition();

            Assert.Equal("[0, 0, 0, 0, 0, 0]", rjp.ToRAPID());
        }

        [Fact]
        public void ToRAPID_DecimalValues_FormatsCorrectly()
        {
            RobotJointPosition rjp = new RobotJointPosition(10.5, 20, 30.75, 0, 0, 0);

            Assert.Equal("[10.5, 20, 30.75, 0, 0, 0]", rjp.ToRAPID());
        }
        #endregion

        #region ToRAPIDDeclaration
        [Fact]
        public void ToRAPIDDeclaration_Named_ReturnsRobjointDeclaration()
        {
            RobotJointPosition rjp = new RobotJointPosition("rj1", 10, 20, 30, 40, 50, 60);

            Assert.Equal("VAR robjoint rj1 := [10, 20, 30, 40, 50, 60];", rjp.ToRAPIDDeclaration(null));
        }

        [Fact]
        public void ToRAPIDDeclaration_Unnamed_ReturnsEmpty()
        {
            RobotJointPosition rjp = new RobotJointPosition(10, 20, 30, 40, 50, 60);

            Assert.Equal(string.Empty, rjp.ToRAPIDDeclaration(null));
        }

        [Fact]
        public void ToRAPIDDeclaration_LocalScope_IncludesScopePrefix()
        {
            RobotJointPosition rjp = new RobotJointPosition("rj1", 0, 0, 0, 0, 0, 0);
            rjp.Scope = Scope.LOCAL;

            Assert.Equal("LOCAL VAR robjoint rj1 := [0, 0, 0, 0, 0, 0];", rjp.ToRAPIDDeclaration(null));
        }

        [Fact]
        public void ToRAPIDDeclaration_TaskScope_IncludesScopePrefix()
        {
            RobotJointPosition rjp = new RobotJointPosition("rj1", 0, 0, 0, 0, 0, 0);
            rjp.Scope = Scope.TASK;

            Assert.Equal("TASK VAR robjoint rj1 := [0, 0, 0, 0, 0, 0];", rjp.ToRAPIDDeclaration(null));
        }
        #endregion

        #region ToRAPIDInstruction
        [Fact]
        public void ToRAPIDInstruction_ReturnsEmpty()
        {
            RobotJointPosition rjp = new RobotJointPosition("rj1", 10, 20, 30, 40, 50, 60);

            Assert.Equal(string.Empty, rjp.ToRAPIDInstruction(null));
        }
        #endregion

        #region Indexer
        [Fact]
        public void Indexer_SetAndGet_RoundTrips()
        {
            RobotJointPosition rjp = new RobotJointPosition();
            rjp[0] = 42;

            Assert.Equal(42, rjp[0]);
        }

        [Fact]
        public void Indexer_OutOfRange_Throws()
        {
            RobotJointPosition rjp = new RobotJointPosition();

            Assert.Throws<IndexOutOfRangeException>(() => { double _ = rjp[6]; });
        }
        #endregion

        #region IsValid
        [Fact]
        public void IsValid_AllZero_ReturnsTrue()
        {
            RobotJointPosition rjp = new RobotJointPosition();

            Assert.True(rjp.IsValid);
        }

        [Fact]
        public void IsValid_WithValues_ReturnsTrue()
        {
            RobotJointPosition rjp = new RobotJointPosition(10, 20, 30, 40, 50, 60);

            Assert.True(rjp.IsValid);
        }
        #endregion

        #region Duplicate
        [Fact]
        public void Duplicate_ReturnsMatchingValues()
        {
            RobotJointPosition original = new RobotJointPosition("rj1", 10, 20, 30, 40, 50, 60);

            RobotJointPosition copy = original.Duplicate();

            Assert.Equal(original.Name, copy.Name);
            for (int i = 0; i < 6; i++)
            {
                Assert.Equal(original[i], copy[i]);
            }
        }
        #endregion

        #region Parse / TryParse
        [Fact]
        public void Parse_ValidRapidString_ReturnsCorrectPositions()
        {
            RobotJointPosition rjp = RobotJointPosition.Parse("VAR robjoint rj1 := [10, 20, 30, 40, 50, 60];");

            Assert.Equal("rj1", rjp.Name);
            Assert.Equal(10, rjp[0]);
            Assert.Equal(20, rjp[1]);
            Assert.Equal(30, rjp[2]);
            Assert.Equal(40, rjp[3]);
            Assert.Equal(50, rjp[4]);
            Assert.Equal(60, rjp[5]);
        }

        [Fact]
        public void TryParse_InvalidString_ReturnsFalse()
        {
            bool result = RobotJointPosition.TryParse("garbage", out RobotJointPosition rjp);

            Assert.False(result);
        }

        [Fact]
        public void TryParse_WrongDatatype_ReturnsFalse()
        {
            bool result = RobotJointPosition.TryParse("VAR speeddata v100 := [100, 500, 5000, 1000];", out RobotJointPosition rjp);

            Assert.False(result);
        }

        [Fact]
        public void Parse_ConstVariableType_SetsConst()
        {
            RobotJointPosition rjp = RobotJointPosition.Parse("CONST robjoint rj1 := [0, 0, 0, 0, 0, 0];");

            Assert.Equal(VariableType.CONST, rjp.VariableType);
            Assert.Equal("rj1", rjp.Name);
        }
        #endregion
    }
}
