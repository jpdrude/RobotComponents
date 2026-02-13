// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components
// Project: https://github.com/RobotComponents/RobotComponents
//
// For license details, see the LICENSE file in the project root.

// Xunit Libs
using Xunit;
// Robot Components Libs
using RobotComponents.ABB.Actions.Instructions;
using RobotComponents.ABB.Enumerations;

namespace RobotComponents.Tests.Actions
{
    public class WaitAITests
    {
        [Fact]
        public void LessThan_ProducesCorrectInstruction()
        {
            WaitAI wai = new WaitAI("ai1", 5.0, InequalitySymbol.LT);

            Assert.Equal("WaitAI ai1, \\LT, 5;", wai.ToRAPIDInstruction(null));
        }

        [Fact]
        public void GreaterThan_ProducesCorrectInstruction()
        {
            WaitAI wai = new WaitAI("ai1", 5.0, InequalitySymbol.GT);

            Assert.Equal("WaitAI ai1, \\GT, 5;", wai.ToRAPIDInstruction(null));
        }

        [Fact]
        public void WithMaxTime_IncludesMaxTimeParam()
        {
            WaitAI wai = new WaitAI("ai1", 5.0, InequalitySymbol.LT, 10);

            string result = wai.ToRAPIDInstruction(null);

            Assert.Contains("\\MaxTime:=10", result);
        }

        [Fact]
        public void IsValid_TrueWithName()
        {
            WaitAI wai = new WaitAI("ai1", 5.0, InequalitySymbol.LT);

            Assert.True(wai.IsValid);
        }
    }

    public class WaitAOTests
    {
        [Fact]
        public void LessThan_ProducesCorrectInstruction()
        {
            WaitAO wao = new WaitAO("ao1", 5.0, InequalitySymbol.LT);

            Assert.Equal("WaitAO ao1, \\LT, 5;", wao.ToRAPIDInstruction(null));
        }

        [Fact]
        public void WithMaxTime_IncludesMaxTimeParam()
        {
            WaitAO wao = new WaitAO("ao1", 5.0, InequalitySymbol.GT, 10);

            string result = wao.ToRAPIDInstruction(null);

            Assert.Contains("\\MaxTime:=10", result);
        }
    }
}
