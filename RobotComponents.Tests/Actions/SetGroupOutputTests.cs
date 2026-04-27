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
    public class SetGroupOutputTests
    {
        [Fact]
        public void BasicValue_ProducesSetGO()
        {
            SetGroupOutput sgo = new SetGroupOutput("group1", 3);

            Assert.Equal("SetGO group1, 3;", sgo.ToRAPIDInstruction(null));
        }

        [Fact]
        public void IsValid_TrueWithValidName()
        {
            SetGroupOutput sgo = new SetGroupOutput("go_1", 0);

            Assert.True(sgo.IsValid);
        }

        [Fact]
        public void IsValid_FalseWithInjectionPayload()
        {
            SetGroupOutput sgo = new SetGroupOutput("signal, 1;\nSetGO other", 0);

            Assert.False(sgo.IsValid);
        }

        [Fact]
        public void ToRAPIDInstruction_ThrowsOnInvalidName()
        {
            SetGroupOutput sgo = new SetGroupOutput("signal;inject", 0);

            Assert.Throws<InvalidOperationException>(() => sgo.ToRAPIDInstruction(null));
        }

        // Expression path
        [Fact]
        public void VariableReference_EmittedVerbatim()
        {
            SetGroupOutput sgo = new SetGroupOutput("go1", "myGroupVar");

            Assert.Equal("SetGO go1, myGroupVar;", sgo.ToRAPIDInstruction(null));
        }
    }
}
