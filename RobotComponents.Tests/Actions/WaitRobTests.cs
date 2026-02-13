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
    public class WaitRobTests
    {
        [Fact]
        public void InPosition_ProducesInPosInstruction()
        {
            WaitRob wr = new WaitRob(false);

            Assert.Equal("WaitRob \\InPos;", wr.ToRAPIDInstruction(null));
        }

        [Fact]
        public void ZeroSpeed_ProducesZeroSpeedInstruction()
        {
            WaitRob wr = new WaitRob(true);

            Assert.Equal("WaitRob \\ZeroSpeed;", wr.ToRAPIDInstruction(null));
        }

        [Fact]
        public void Default_IsNotValid()
        {
            WaitRob wr = new WaitRob();

            Assert.False(wr.IsValid);
        }

        [Fact]
        public void InPosition_IsValid()
        {
            WaitRob wr = new WaitRob(false);

            Assert.True(wr.IsValid);
            Assert.True(wr.InPosition);
            Assert.False(wr.ZeroSpeed);
        }

        [Fact]
        public void ZeroSpeed_IsValid()
        {
            WaitRob wr = new WaitRob(true);

            Assert.True(wr.IsValid);
            Assert.False(wr.InPosition);
            Assert.True(wr.ZeroSpeed);
        }

        [Fact]
        public void BothTrue_IsNotValid()
        {
            WaitRob wr = new WaitRob();
            wr.InPosition = true;
            wr.ZeroSpeed = true;

            Assert.False(wr.IsValid);
        }

        [Fact]
        public void ToRAPIDDeclaration_ReturnsEmpty()
        {
            WaitRob wr = new WaitRob(false);

            Assert.Equal(string.Empty, wr.ToRAPIDDeclaration(null));
        }
    }
}
