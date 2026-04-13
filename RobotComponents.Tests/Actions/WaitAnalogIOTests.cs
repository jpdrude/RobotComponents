// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components
// Project: https://github.com/RobotComponents/RobotComponents
//
// For license details, see the LICENSE file in the project root.

// System Libs
using System;
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

        [Fact]
        public void IsValid_FalseWithInjectionPayload()
        {
            WaitAI wai = new WaitAI("ai1, \\LT, 0;\nMoveJ target", 5.0, InequalitySymbol.LT);

            Assert.False(wai.IsValid);
        }

        [Fact]
        public void ToRAPIDInstruction_ThrowsOnInvalidName()
        {
            WaitAI wai = new WaitAI("ai1;inject", 5.0, InequalitySymbol.LT);

            Assert.Throws<InvalidOperationException>(() => wai.ToRAPIDInstruction(null));
        }

        // Expression path
        [Fact]
        public void VariableReference_EmittedVerbatim()
        {
            WaitAI wai = new WaitAI("ai1", "threshold", InequalitySymbol.GT);

            Assert.Equal("WaitAI ai1, \\GT, threshold;", wai.ToRAPIDInstruction(null));
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

        [Fact]
        public void IsValid_FalseWithInjectionPayload()
        {
            WaitAO wao = new WaitAO("ao1, \\LT, 0;\nMoveJ target", 5.0, InequalitySymbol.LT);

            Assert.False(wao.IsValid);
        }

        [Fact]
        public void ToRAPIDInstruction_ThrowsOnInvalidName()
        {
            WaitAO wao = new WaitAO("ao1;inject", 5.0, InequalitySymbol.LT);

            Assert.Throws<InvalidOperationException>(() => wao.ToRAPIDInstruction(null));
        }

        // Expression path
        [Fact]
        public void VariableReference_EmittedVerbatim()
        {
            WaitAO wao = new WaitAO("ao1", "threshold", InequalitySymbol.LT);

            Assert.Equal("WaitAO ao1, \\LT, threshold;", wao.ToRAPIDInstruction(null));
        }
    }
}
