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
    public class JointTargetTests
    {
        #region Constructor
        [Fact]
        public void Constructor_WithRobotJointPosition_SetsDefaultExternal()
        {
            RobotJointPosition rjp = new RobotJointPosition(0, 0, 0, 0, 0, 0);
            JointTarget jt = new JointTarget(rjp);

            Assert.Equal(9e9, jt.ExternalJointPosition[0]);
            Assert.Equal(9e9, jt.ExternalJointPosition[5]);
        }

        [Fact]
        public void Constructor_Named_SetsAllFields()
        {
            RobotJointPosition rjp = new RobotJointPosition(10, 20, 30, 40, 50, 60);
            ExternalJointPosition ejp = new ExternalJointPosition(100);
            JointTarget jt = new JointTarget("jt1", rjp, ejp);

            Assert.Equal("jt1", jt.Name);
            Assert.Equal(10, jt.RobotJointPosition[0]);
            Assert.Equal(60, jt.RobotJointPosition[5]);
            Assert.Equal(100, jt.ExternalJointPosition[0]);
        }

        [Fact]
        public void Constructor_NamedWithRjpOnly_SetsDefaultExternal()
        {
            RobotJointPosition rjp = new RobotJointPosition(10, 20, 30, 40, 50, 60);
            JointTarget jt = new JointTarget("jt1", rjp);

            Assert.Equal("jt1", jt.Name);
            Assert.Equal(9e9, jt.ExternalJointPosition[0]);
        }
        #endregion

        #region ToRAPID
        [Fact]
        public void ToRAPID_AllZeroJoints_ProducesNestedArray()
        {
            RobotJointPosition rjp = new RobotJointPosition(0, 0, 0, 0, 0, 0);
            JointTarget jt = new JointTarget(rjp);

            Assert.Equal("[[0, 0, 0, 0, 0, 0], [9E9, 9E9, 9E9, 9E9, 9E9, 9E9]]", jt.ToRAPID());
        }

        [Fact]
        public void ToRAPID_WithJointValues_ProducesCorrectValues()
        {
            RobotJointPosition rjp = new RobotJointPosition(10, 20, 30, 40, 50, 60);
            ExternalJointPosition ejp = new ExternalJointPosition(100);
            JointTarget jt = new JointTarget(rjp, ejp);

            string rapid = jt.ToRAPID();

            Assert.Contains("[10, 20, 30, 40, 50, 60]", rapid);
            Assert.Contains("[100, 9E9, 9E9, 9E9, 9E9, 9E9]", rapid);
        }
        #endregion

        #region ToRAPIDDeclaration
        [Fact]
        public void ToRAPIDDeclaration_Named_ProducesVarJointtarget()
        {
            RobotJointPosition rjp = new RobotJointPosition(0, 0, 0, 0, 0, 0);
            JointTarget jt = new JointTarget("jt1", rjp);

            string result = jt.ToRAPIDDeclaration(null);

            Assert.Equal("VAR jointtarget jt1 := [[0, 0, 0, 0, 0, 0], [9E9, 9E9, 9E9, 9E9, 9E9, 9E9]];", result);
        }

        [Fact]
        public void ToRAPIDDeclaration_Unnamed_ReturnsEmpty()
        {
            RobotJointPosition rjp = new RobotJointPosition(0, 0, 0, 0, 0, 0);
            JointTarget jt = new JointTarget(rjp);

            Assert.Equal(string.Empty, jt.ToRAPIDDeclaration(null));
        }
        #endregion

        #region ToRAPIDInstruction
        [Fact]
        public void ToRAPIDInstruction_ReturnsEmpty()
        {
            RobotJointPosition rjp = new RobotJointPosition(0, 0, 0, 0, 0, 0);
            JointTarget jt = new JointTarget("jt1", rjp);

            Assert.Equal(string.Empty, jt.ToRAPIDInstruction(null));
        }
        #endregion

        #region IsValid
        [Fact]
        public void IsValid_ValidPositions_ReturnsTrue()
        {
            RobotJointPosition rjp = new RobotJointPosition(0, 0, 0, 0, 0, 0);
            JointTarget jt = new JointTarget("jt1", rjp);

            Assert.True(jt.IsValid);
        }

        [Fact]
        public void IsValid_DefaultConstructor_ReturnsFalse()
        {
            JointTarget jt = new JointTarget();

            Assert.False(jt.IsValid);
        }
        #endregion

        #region Duplicate
        [Fact]
        public void Duplicate_ReturnsMatchingValues()
        {
            RobotJointPosition rjp = new RobotJointPosition(10, 20, 30, 40, 50, 60);
            ExternalJointPosition ejp = new ExternalJointPosition(100, 200);
            JointTarget original = new JointTarget("jt1", rjp, ejp);

            JointTarget copy = original.Duplicate();

            Assert.Equal(original.Name, copy.Name);
            Assert.Equal(original.ToRAPID(), copy.ToRAPID());
        }
        #endregion

        #region Parse / TryParse
        [Fact]
        public void Parse_ValidRapidString_ReturnsCorrectPositions()
        {
            JointTarget jt = JointTarget.Parse("VAR jointtarget jt1 := [[10, 20, 30, 40, 50, 60], [100, 9E9, 9E9, 9E9, 9E9, 9E9]];");

            Assert.Equal("jt1", jt.Name);
            Assert.Equal(10, jt.RobotJointPosition[0]);
            Assert.Equal(60, jt.RobotJointPosition[5]);
            Assert.Equal(100, jt.ExternalJointPosition[0]);
            Assert.Equal(9e9, jt.ExternalJointPosition[1]);
        }

        [Fact]
        public void TryParse_InvalidString_ReturnsFalse()
        {
            bool result = JointTarget.TryParse("garbage", out JointTarget jt);

            Assert.False(result);
        }

        [Fact]
        public void TryParse_WrongDatatype_ReturnsFalse()
        {
            bool result = JointTarget.TryParse("VAR speeddata v100 := [100, 500, 5000, 1000];", out JointTarget jt);

            Assert.False(result);
        }

        [Fact]
        public void TryParse_TooFewValues_ReturnsFalse()
        {
            bool result = JointTarget.TryParse("VAR jointtarget jt := [10, 20, 30];", out JointTarget jt);

            Assert.False(result);
        }

        [Fact]
        public void Parse_ConstVariableType_SetsConst()
        {
            JointTarget jt = JointTarget.Parse("CONST jointtarget jt1 := [[0, 0, 0, 0, 0, 0], [9E9, 9E9, 9E9, 9E9, 9E9, 9E9]];");

            Assert.Equal(VariableType.CONST, jt.VariableType);
            Assert.Equal("jt1", jt.Name);
        }

        [Fact]
        public void Parse_TaskScope_SetsTaskScope()
        {
            JointTarget jt = JointTarget.Parse("TASK VAR jointtarget jt1 := [[10, 20, 30, 40, 50, 60], [9E9, 9E9, 9E9, 9E9, 9E9, 9E9]];");

            Assert.Equal(Scope.TASK, jt.Scope);
            Assert.Equal("jt1", jt.Name);
            Assert.Equal(10, jt.RobotJointPosition[0]);
        }
        #endregion
    }
}
