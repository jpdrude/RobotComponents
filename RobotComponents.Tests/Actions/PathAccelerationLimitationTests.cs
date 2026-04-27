// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components
// Project: https://github.com/RobotComponents/RobotComponents
//
// For license details, see the LICENSE file in the project root.

// Xunit Libs
using Xunit;
// RobotComponents Libs
using RobotComponents.ABB.Actions.Instructions;

namespace RobotComponents.Tests.Actions
{
    public class PathAccelerationLimitationTests
    {
        [Fact]
        public void BothDisabled_ProducesFalseFalse()
        {
            PathAccelerationLimitation pal = new PathAccelerationLimitation(false, 0.0, false, 0.0);

            Assert.Equal("PathAccelerationLimitation FALSE, FALSE;", pal.ToRAPIDInstruction(null));
        }

        [Fact]
        public void AccelerationEnabled_IncludesAccMax()
        {
            PathAccelerationLimitation pal = new PathAccelerationLimitation(true, 2.5, false, 0.0);

            string result = pal.ToRAPIDInstruction(null);

            Assert.Contains("TRUE\\AccMax:=2.5", result);
            Assert.Contains("FALSE", result);
        }

        [Fact]
        public void DecelerationEnabled_IncludesDecelMax()
        {
            PathAccelerationLimitation pal = new PathAccelerationLimitation(false, 0.0, true, 3.0);

            string result = pal.ToRAPIDInstruction(null);

            Assert.Contains("FALSE", result);
            Assert.Contains("TRUE\\DecelMax:=3", result);
        }

        [Fact]
        public void BothEnabled_IncludesBothParams()
        {
            PathAccelerationLimitation pal = new PathAccelerationLimitation(true, 2.0, true, 4.0);

            string result = pal.ToRAPIDInstruction(null);

            Assert.Contains("TRUE\\AccMax:=2", result);
            Assert.Contains("TRUE\\DecelMax:=4", result);
        }

        [Fact]
        public void ToRAPIDDeclaration_ReturnsEmpty()
        {
            PathAccelerationLimitation pal = new PathAccelerationLimitation(false, 0.0, false, 0.0);

            Assert.Equal(string.Empty, pal.ToRAPIDDeclaration(null));
        }

        [Fact]
        public void IsValid_TrueForNonNegativeValues()
        {
            PathAccelerationLimitation pal = new PathAccelerationLimitation(true, 2.0, true, 3.0);

            Assert.True(pal.IsValid);
        }

        [Fact]
        public void IsValid_FalseWhenAccelerationMaxNegative()
        {
            PathAccelerationLimitation pal = new PathAccelerationLimitation(true, -1.0, false, 0.0);

            Assert.False(pal.IsValid);
        }

        [Fact]
        public void IsValid_FalseWhenDecelerationMaxNegative()
        {
            PathAccelerationLimitation pal = new PathAccelerationLimitation(false, 0.0, true, -1.0);

            Assert.False(pal.IsValid);
        }

        // Expression path
        [Fact]
        public void VariableReference_AccMax_EmittedVerbatim()
        {
            PathAccelerationLimitation pal = new PathAccelerationLimitation(true, "accelLimit", false, "0");

            string result = pal.ToRAPIDInstruction(null);

            Assert.Contains("TRUE\\AccMax:=accelLimit", result);
        }

        [Fact]
        public void VariableReference_DecelMax_EmittedVerbatim()
        {
            PathAccelerationLimitation pal = new PathAccelerationLimitation(false, "0", true, "decelLimit");

            string result = pal.ToRAPIDInstruction(null);

            Assert.Contains("TRUE\\DecelMax:=decelLimit", result);
        }

        [Fact]
        public void IsValid_TrueForVariableExpressions()
        {
            PathAccelerationLimitation pal = new PathAccelerationLimitation(true, "accelVar", true, "decelVar");

            Assert.True(pal.IsValid);
        }

        [Fact]
        public void BackwardCompatProperty_AccelerationMax_ReturnsDouble()
        {
            PathAccelerationLimitation pal = new PathAccelerationLimitation(true, 2.5, true, 3.5);

            Assert.Equal(2.5, pal.AccelerationMax, 5);
        }

        [Fact]
        public void BackwardCompatProperty_DecelerationMax_ReturnsDouble()
        {
            PathAccelerationLimitation pal = new PathAccelerationLimitation(true, 2.5, true, 3.5);

            Assert.Equal(3.5, pal.DecelerationMax, 5);
        }
    }
}
