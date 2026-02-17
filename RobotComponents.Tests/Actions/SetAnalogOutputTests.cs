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
    public class SetAnalogOutputTests
    {
        [Fact]
        public void BasicValue_ProducesSetAO()
        {
            SetAnalogOutput sao = new SetAnalogOutput("signal", 5.5);

            Assert.Equal("SetAO signal, 5.5;", sao.ToRAPIDInstruction(null));
        }

        [Fact]
        public void IsValid_TrueWithValidName()
        {
            SetAnalogOutput sao = new SetAnalogOutput("ao_1", 0);

            Assert.True(sao.IsValid);
        }

        [Fact]
        public void IsValid_FalseWithInjectionPayload()
        {
            SetAnalogOutput sao = new SetAnalogOutput("signal, 1;\nSetAO other", 0);

            Assert.False(sao.IsValid);
        }

        [Fact]
        public void ToRAPIDInstruction_ThrowsOnInvalidName()
        {
            SetAnalogOutput sao = new SetAnalogOutput("signal;inject", 0);

            Assert.Throws<InvalidOperationException>(() => sao.ToRAPIDInstruction(null));
        }
    }
}
