// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components
// Project: https://github.com/RobotComponents/RobotComponents
//
// For license details, see the LICENSE file in the project root.

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
    }
}
