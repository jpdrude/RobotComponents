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
    public class SetDigitalOutputTests
    {
        [Fact]
        public void BasicTrue_ProducesSetDO()
        {
            SetDigitalOutput sdo = new SetDigitalOutput("signal", true);

            Assert.Equal("SetDO signal, 1;", sdo.ToRAPIDInstruction(null));
        }

        [Fact]
        public void BasicFalse_ProducesZero()
        {
            SetDigitalOutput sdo = new SetDigitalOutput("signal", false);

            Assert.Equal("SetDO signal, 0;", sdo.ToRAPIDInstruction(null));
        }

        [Fact]
        public void WithDelay_IncludesSDelay()
        {
            SetDigitalOutput sdo = new SetDigitalOutput("signal", true);
            sdo.Delay = 0.5;

            Assert.Equal("SetDO \\SDelay:=0.5, signal, 1;", sdo.ToRAPIDInstruction(null));
        }

        [Fact]
        public void WithSync_IncludesSync()
        {
            SetDigitalOutput sdo = new SetDigitalOutput("signal", true);
            sdo.Sync = true;

            Assert.Equal("SetDO \\Sync, signal, 1;", sdo.ToRAPIDInstruction(null));
        }

        [Fact]
        public void SyncOverridesDelay_OnlySyncAppears()
        {
            SetDigitalOutput sdo = new SetDigitalOutput("signal", true);
            sdo.Delay = 0.5;
            sdo.Sync = true;

            string result = sdo.ToRAPIDInstruction(null);

            Assert.Contains("\\Sync", result);
            Assert.DoesNotContain("\\SDelay", result);
        }

        [Fact]
        public void IsValid_TrueWithName()
        {
            SetDigitalOutput sdo = new SetDigitalOutput("sig", true);

            Assert.True(sdo.IsValid);
        }

        [Fact]
        public void IsValid_FalseWithNullName()
        {
            SetDigitalOutput sdo = new SetDigitalOutput();

            Assert.False(sdo.IsValid);
        }

        [Fact]
        public void IsValid_FalseWithEmptyName()
        {
            SetDigitalOutput sdo = new SetDigitalOutput("", true);

            Assert.False(sdo.IsValid);
        }

        [Fact]
        public void ToRAPIDDeclaration_ReturnsEmpty()
        {
            SetDigitalOutput sdo = new SetDigitalOutput("signal", true);

            Assert.Equal(string.Empty, sdo.ToRAPIDDeclaration(null));
        }

        [Fact]
        public void IsValid_FalseWithInjectionPayload()
        {
            SetDigitalOutput sdo = new SetDigitalOutput("signal, 1;\nSetDO other", true);

            Assert.False(sdo.IsValid);
        }

        [Fact]
        public void IsValid_FalseWithNameStartingWithDigit()
        {
            SetDigitalOutput sdo = new SetDigitalOutput("1signal", true);

            Assert.False(sdo.IsValid);
        }

        [Fact]
        public void ToRAPIDInstruction_ThrowsOnInvalidName()
        {
            SetDigitalOutput sdo = new SetDigitalOutput("signal, 1;\nSetDO other", true);

            Assert.Throws<InvalidOperationException>(() => sdo.ToRAPIDInstruction(null));
        }

        // Expression path
        [Fact]
        public void VariableReference_EmittedVerbatim()
        {
            SetDigitalOutput sdo = new SetDigitalOutput("do1", "myVar");

            Assert.Equal("SetDO do1, myVar;", sdo.ToRAPIDInstruction(null));
        }

        [Fact]
        public void ArithmeticExpression_EmittedVerbatim()
        {
            SetDigitalOutput sdo = new SetDigitalOutput("do1", "counter + 1");

            Assert.Equal("SetDO do1, counter + 1;", sdo.ToRAPIDInstruction(null));
        }

        [Fact]
        public void IsValid_TrueWithExpression()
        {
            SetDigitalOutput sdo = new SetDigitalOutput("do1", "myVar");

            Assert.True(sdo.IsValid);
        }

        [Fact]
        public void IsValid_FalseWithEmptyExpression()
        {
            SetDigitalOutput sdo = new SetDigitalOutput("do1", "");

            Assert.False(sdo.IsValid);
        }
    }
}
