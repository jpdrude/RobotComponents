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

namespace RobotComponents.Tests.Actions
{
    public class PulseDigitalOutputTests
    {
        [Fact]
        public void Basic_ProducesPulseDO()
        {
            PulseDigitalOutput pdo = new PulseDigitalOutput(false, 0.5, "signal");

            Assert.Equal("PulseDO \\PLength:=0.5, signal;", pdo.ToRAPIDInstruction(null));
        }

        [Fact]
        public void WithHigh_IncludesHigh()
        {
            PulseDigitalOutput pdo = new PulseDigitalOutput(true, 0.5, "signal");

            Assert.Equal("PulseDO \\High, \\PLength:=0.5, signal;", pdo.ToRAPIDInstruction(null));
        }

        [Fact]
        public void IsValid_TrueWithValidNameAndLength()
        {
            PulseDigitalOutput pdo = new PulseDigitalOutput(false, 0.2, "do_1");

            Assert.True(pdo.IsValid);
        }

        [Fact]
        public void IsValid_FalseWithInjectionPayload()
        {
            PulseDigitalOutput pdo = new PulseDigitalOutput(false, 0.2, "signal;\nMoveJ");

            Assert.False(pdo.IsValid);
        }

        [Fact]
        public void ToRAPIDInstruction_ThrowsOnInvalidName()
        {
            PulseDigitalOutput pdo = new PulseDigitalOutput(false, 0.2, "signal;inject");

            Assert.Throws<InvalidOperationException>(() => pdo.ToRAPIDInstruction(null));
        }

        // Expression path
        [Fact]
        public void StringExpression_Length_EmittedVerbatim()
        {
            PulseDigitalOutput pdo = new PulseDigitalOutput(false, "myPulseLen", "do1");

            Assert.Equal("PulseDO \\PLength:=myPulseLen, do1;", pdo.ToRAPIDInstruction(null));
        }

        [Fact]
        public void IsValid_TrueWithVariableExpression()
        {
            PulseDigitalOutput pdo = new PulseDigitalOutput(false, "myVar", "do1");

            Assert.True(pdo.IsValid);
        }
    }
}
