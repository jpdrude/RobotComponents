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
    public class VelocitySetTests
    {
        [Fact]
        public void Basic_ProducesVelSetInstruction()
        {
            VelocitySet vel = new VelocitySet(100.0, 250.0);

            Assert.Equal("VelSet 100, 250;", vel.ToRAPIDInstruction(null));
        }

        [Fact]
        public void PartialOverride_ProducesCorrectInstruction()
        {
            VelocitySet vel = new VelocitySet(50.0, 1000.0);

            Assert.Equal("VelSet 50, 1000;", vel.ToRAPIDInstruction(null));
        }

        [Fact]
        public void ToRAPIDDeclaration_ReturnsEmpty()
        {
            VelocitySet vel = new VelocitySet(100.0, 250.0);

            Assert.Equal(string.Empty, vel.ToRAPIDDeclaration(null));
        }

        [Fact]
        public void IsValid_TrueForValidValues()
        {
            VelocitySet vel = new VelocitySet(80.0, 500.0);

            Assert.True(vel.IsValid);
        }

        [Fact]
        public void IsValid_FalseWhenOverrideAbove100()
        {
            VelocitySet vel = new VelocitySet(120.0, 250.0);

            Assert.False(vel.IsValid);
        }

        [Fact]
        public void IsValid_FalseWhenOverrideNegative()
        {
            VelocitySet vel = new VelocitySet(-10.0, 250.0);

            Assert.False(vel.IsValid);
        }

        [Fact]
        public void IsValid_FalseWhenMaxIsZeroOrNegative()
        {
            VelocitySet vel = new VelocitySet(100.0, 0.0);

            Assert.False(vel.IsValid);
        }

        // Expression path
        [Fact]
        public void VariableReference_EmittedVerbatim()
        {
            VelocitySet vel = new VelocitySet("overrideVar", "maxVar");

            Assert.Equal("VelSet overrideVar, maxVar;", vel.ToRAPIDInstruction(null));
        }

        [Fact]
        public void IsValid_TrueForVariableExpressions()
        {
            VelocitySet vel = new VelocitySet("overrideVar", "maxVar");

            Assert.True(vel.IsValid);
        }

        [Fact]
        public void FunctionCallExpression_EmittedVerbatim()
        {
            VelocitySet vel = new VelocitySet("GetOverride()", "GetMaxSpeed()");

            Assert.Equal("VelSet GetOverride(), GetMaxSpeed();", vel.ToRAPIDInstruction(null));
        }

        [Fact]
        public void BackwardCompatProperty_Override_ReturnsDouble()
        {
            VelocitySet vel = new VelocitySet(75.0, 300.0);

            Assert.Equal(75.0, vel.Override, 5);
        }

        [Fact]
        public void BackwardCompatProperty_Max_ReturnsDouble()
        {
            VelocitySet vel = new VelocitySet(75.0, 300.0);

            Assert.Equal(300.0, vel.Max, 5);
        }
    }
}
