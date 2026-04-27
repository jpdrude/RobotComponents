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
    public class WaitTimeTests
    {
        [Fact]
        public void Basic_ProducesWaitTimeInstruction()
        {
            WaitTime wt = new WaitTime(0.5);

            Assert.Equal("WaitTime 0.5;", wt.ToRAPIDInstruction(null));
        }

        [Fact]
        public void IntegerDuration_OmitsDecimalTrailingZeros()
        {
            // Format {_duration:0.###} => 1.0 becomes "1"
            WaitTime wt = new WaitTime(1.0);

            Assert.Equal("WaitTime 1;", wt.ToRAPIDInstruction(null));
        }

        [Fact]
        public void InPosition_IncludesInPosFlag()
        {
            WaitTime wt = new WaitTime(1.0, true);

            Assert.Equal("WaitTime \\InPos, 1;", wt.ToRAPIDInstruction(null));
        }

        [Fact]
        public void IsValid_ZeroDuration_ReturnsTrue()
        {
            WaitTime wt = new WaitTime(0);

            Assert.True(wt.IsValid);
        }

        [Fact]
        public void IsValid_NegativeDuration_ReturnsFalse()
        {
            WaitTime wt = new WaitTime(-1);

            Assert.False(wt.IsValid);
        }

        [Fact]
        public void ToRAPIDDeclaration_ReturnsEmpty()
        {
            WaitTime wt = new WaitTime(1.0);

            Assert.Equal(string.Empty, wt.ToRAPIDDeclaration(null));
        }

        // Expression path
        [Fact]
        public void VariableExpression_EmittedVerbatim()
        {
            WaitTime wt = new WaitTime("myDelay");

            Assert.Equal("WaitTime myDelay;", wt.ToRAPIDInstruction(null));
        }

        [Fact]
        public void FunctionCallExpression_EmittedVerbatim()
        {
            WaitTime wt = new WaitTime("GetDelay()");

            Assert.Equal("WaitTime GetDelay();", wt.ToRAPIDInstruction(null));
        }

        [Fact]
        public void IsValid_TrueForVariableString()
        {
            WaitTime wt = new WaitTime("myDelay");

            Assert.True(wt.IsValid);
        }

        [Fact]
        public void IsValid_FalseForNegativeLiteralString()
        {
            WaitTime wt = new WaitTime("-2.0");

            Assert.False(wt.IsValid);
        }

        [Fact]
        public void IsValid_FalseForEmptyString()
        {
            WaitTime wt = new WaitTime("");

            Assert.False(wt.IsValid);
        }

        [Fact]
        public void InPosition_WithExpression_IncludesInPosFlag()
        {
            WaitTime wt = new WaitTime("myDelay", true);

            Assert.Equal("WaitTime \\InPos, myDelay;", wt.ToRAPIDInstruction(null));
        }
    }
}
