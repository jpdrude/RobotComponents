// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components (Modified)
// Original project: https://github.com/RobotComponents/RobotComponents
// Modified project: https://github.com/jpdrude/RobotComponents
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
    public class InvertDigitalOutputTests
    {
        [Fact]
        public void Basic_ProducesInvertDO()
        {
            InvertDigitalOutput ido = new InvertDigitalOutput("signal");

            Assert.Equal("InvertDO signal;", ido.ToRAPIDInstruction(null));
        }

        [Fact]
        public void IsValid_TrueWithName()
        {
            InvertDigitalOutput ido = new InvertDigitalOutput("sig");

            Assert.True(ido.IsValid);
        }

        [Fact]
        public void IsValid_FalseWithNullName()
        {
            InvertDigitalOutput ido = new InvertDigitalOutput();

            Assert.False(ido.IsValid);
        }

        [Fact]
        public void IsValid_FalseWithEmptyName()
        {
            InvertDigitalOutput ido = new InvertDigitalOutput("");

            Assert.False(ido.IsValid);
        }

        [Fact]
        public void ToRAPIDDeclaration_ReturnsEmpty()
        {
            InvertDigitalOutput ido = new InvertDigitalOutput("signal");

            Assert.Equal(string.Empty, ido.ToRAPIDDeclaration(null));
        }

        [Fact]
        public void IsValid_FalseWithInjectionPayload()
        {
            InvertDigitalOutput ido = new InvertDigitalOutput("signal;\nInvertDO other");

            Assert.False(ido.IsValid);
        }

        [Fact]
        public void IsValid_FalseWithNameStartingWithDigit()
        {
            InvertDigitalOutput ido = new InvertDigitalOutput("1signal");

            Assert.False(ido.IsValid);
        }

        [Fact]
        public void ToRAPIDInstruction_ThrowsOnInvalidName()
        {
            InvertDigitalOutput ido = new InvertDigitalOutput("signal;\nInvertDO other");

            Assert.Throws<InvalidOperationException>(() => ido.ToRAPIDInstruction(null));
        }

        [Fact]
        public void Duplicate_ProducesEqualCopy()
        {
            InvertDigitalOutput ido = new InvertDigitalOutput("signal");
            InvertDigitalOutput copy = ido.Duplicate();

            Assert.Equal(ido.Name, copy.Name);
            Assert.Equal(ido.ToRAPIDInstruction(null), copy.ToRAPIDInstruction(null));
        }
    }
}
