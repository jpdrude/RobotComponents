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
    public class WaitDITests
    {
        [Fact]
        public void BasicTrue_ProducesWaitDIInstruction()
        {
            WaitDI wdi = new WaitDI("di1", true);

            Assert.Equal("WaitDI di1, 1;", wdi.ToRAPIDInstruction(null));
        }

        [Fact]
        public void BasicFalse_ProducesZero()
        {
            WaitDI wdi = new WaitDI("di1", false);

            Assert.Equal("WaitDI di1, 0;", wdi.ToRAPIDInstruction(null));
        }

        [Fact]
        public void WithMaxTimeAndTimeFlag_IncludesBoth()
        {
            WaitDI wdi = new WaitDI("di1", false, 5.0, true);

            string result = wdi.ToRAPIDInstruction(null);

            Assert.Contains("\\MaxTime:=5", result);
            Assert.Contains("\\TimeFlag:=TRUE", result);
        }

        [Fact]
        public void WithMaxTimeFlagFalse_IncludesBothParams()
        {
            WaitDI wdi = new WaitDI("di1", true, 10.0, false);

            string result = wdi.ToRAPIDInstruction(null);

            Assert.Contains("\\MaxTime:=10", result);
            Assert.Contains("\\TimeFlag:=FALSE", result);
        }

        [Fact]
        public void IsValid_TrueWithName()
        {
            WaitDI wdi = new WaitDI("di1", true);

            Assert.True(wdi.IsValid);
        }

        [Fact]
        public void IsValid_FalseWithNullName()
        {
            WaitDI wdi = new WaitDI();

            Assert.False(wdi.IsValid);
        }

        [Fact]
        public void IsValid_FalseWithInjectionPayload()
        {
            WaitDI wdi = new WaitDI("di1, 1;\nMoveJ target", true);

            Assert.False(wdi.IsValid);
        }

        [Fact]
        public void ToRAPIDInstruction_ThrowsOnInvalidName()
        {
            WaitDI wdi = new WaitDI("di1;inject", true);

            Assert.Throws<InvalidOperationException>(() => wdi.ToRAPIDInstruction(null));
        }
    }

    public class WaitDOTests
    {
        [Fact]
        public void BasicTrue_ProducesWaitDOInstruction()
        {
            WaitDO wdo = new WaitDO("do1", true);

            Assert.Equal("WaitDO do1, 1;", wdo.ToRAPIDInstruction(null));
        }

        [Fact]
        public void WithMaxTime_IncludesMaxTimeAndTimeFlag()
        {
            WaitDO wdo = new WaitDO("do1", false, 5.0, true);

            string result = wdo.ToRAPIDInstruction(null);

            Assert.Contains("WaitDO", result);
            Assert.Contains("\\MaxTime:=5", result);
            Assert.Contains("\\TimeFlag:=TRUE", result);
        }

        [Fact]
        public void IsValid_FalseWithInjectionPayload()
        {
            WaitDO wdo = new WaitDO("do1, 1;\nMoveJ target", true);

            Assert.False(wdo.IsValid);
        }

        [Fact]
        public void ToRAPIDInstruction_ThrowsOnInvalidName()
        {
            WaitDO wdo = new WaitDO("do1;inject", true);

            Assert.Throws<InvalidOperationException>(() => wdo.ToRAPIDInstruction(null));
        }
    }
}
