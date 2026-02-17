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
    public class WaitGITests
    {
        [Fact]
        public void Basic_ProducesWaitGIInstruction()
        {
            WaitGI wgi = new WaitGI("gi1", 42);

            Assert.Equal("WaitGI gi1, 42;", wgi.ToRAPIDInstruction(null));
        }

        [Fact]
        public void WithMaxTime_IncludesMaxTimeParam()
        {
            WaitGI wgi = new WaitGI("gi1", 42, 5.0);

            string result = wgi.ToRAPIDInstruction(null);

            Assert.Contains("\\MaxTime:=5", result);
        }

        [Fact]
        public void IsValid_TrueWithName()
        {
            WaitGI wgi = new WaitGI("gi1", 42);

            Assert.True(wgi.IsValid);
        }

        [Fact]
        public void IsValid_FalseWithInjectionPayload()
        {
            WaitGI wgi = new WaitGI("gi1, 0;\nMoveJ target", 42);

            Assert.False(wgi.IsValid);
        }

        [Fact]
        public void ToRAPIDInstruction_ThrowsOnInvalidName()
        {
            WaitGI wgi = new WaitGI("gi1;inject", 42);

            Assert.Throws<InvalidOperationException>(() => wgi.ToRAPIDInstruction(null));
        }
    }

    public class WaitGOTests
    {
        [Fact]
        public void Basic_ProducesWaitGOInstruction()
        {
            WaitGO wgo = new WaitGO("go1", 42);

            Assert.Equal("WaitGO go1, 42;", wgo.ToRAPIDInstruction(null));
        }

        [Fact]
        public void WithMaxTime_IncludesMaxTimeParam()
        {
            WaitGO wgo = new WaitGO("go1", 42, 5.0);

            string result = wgo.ToRAPIDInstruction(null);

            Assert.Contains("\\MaxTime:=5", result);
        }

        [Fact]
        public void IsValid_FalseWithInjectionPayload()
        {
            WaitGO wgo = new WaitGO("go1, 0;\nMoveJ target", 42);

            Assert.False(wgo.IsValid);
        }

        [Fact]
        public void ToRAPIDInstruction_ThrowsOnInvalidName()
        {
            WaitGO wgo = new WaitGO("go1;inject", 42);

            Assert.Throws<InvalidOperationException>(() => wgo.ToRAPIDInstruction(null));
        }
    }
}
