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
    public class AccelerationSetTests
    {
        [Fact]
        public void Basic_ProducesAccSetInstruction()
        {
            AccelerationSet accel = new AccelerationSet(100.0, 100.0);

            Assert.Equal("AccSet 100, 100;", accel.ToRAPIDInstruction(null));
        }

        [Fact]
        public void PartialValues_ProducesCorrectInstruction()
        {
            AccelerationSet accel = new AccelerationSet(50.0, 75.0);

            Assert.Equal("AccSet 50, 75;", accel.ToRAPIDInstruction(null));
        }

        [Fact]
        public void ToRAPIDDeclaration_ReturnsEmpty()
        {
            AccelerationSet accel = new AccelerationSet(100.0, 100.0);

            Assert.Equal(string.Empty, accel.ToRAPIDDeclaration(null));
        }

        [Fact]
        public void IsValid_TrueForValuesInRange()
        {
            AccelerationSet accel = new AccelerationSet(50.0, 50.0);

            Assert.True(accel.IsValid);
        }

        [Fact]
        public void IsValid_FalseWhenAccelerationBelowMin()
        {
            AccelerationSet accel = new AccelerationSet(10.0, 50.0);

            Assert.False(accel.IsValid);
        }

        [Fact]
        public void IsValid_FalseWhenRampBelowMin()
        {
            AccelerationSet accel = new AccelerationSet(50.0, 5.0);

            Assert.False(accel.IsValid);
        }

        [Fact]
        public void IsValid_FalseWhenAccelerationAboveMax()
        {
            AccelerationSet accel = new AccelerationSet(150.0, 50.0);

            Assert.False(accel.IsValid);
        }

        // Expression path
        [Fact]
        public void VariableReference_EmittedVerbatim()
        {
            AccelerationSet accel = new AccelerationSet("accelVar", "rampVar");

            Assert.Equal("AccSet accelVar, rampVar;", accel.ToRAPIDInstruction(null));
        }

        [Fact]
        public void IsValid_TrueForVariableExpressions()
        {
            AccelerationSet accel = new AccelerationSet("accelVar", "rampVar");

            Assert.True(accel.IsValid);
        }

        [Fact]
        public void FunctionCallExpression_EmittedVerbatim()
        {
            AccelerationSet accel = new AccelerationSet("GetAccel()", "GetRamp()");

            Assert.Equal("AccSet GetAccel(), GetRamp();", accel.ToRAPIDInstruction(null));
        }

        [Fact]
        public void BackwardCompatProperty_Acceleration_ReturnsDouble()
        {
            AccelerationSet accel = new AccelerationSet(80.0, 60.0);

            Assert.Equal(80.0, accel.Acceleration, 5);
        }

        [Fact]
        public void BackwardCompatProperty_Ramp_ReturnsDouble()
        {
            AccelerationSet accel = new AccelerationSet(80.0, 60.0);

            Assert.Equal(60.0, accel.Ramp, 5);
        }
    }
}
