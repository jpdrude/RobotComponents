// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components
// Project: https://github.com/RobotComponents/RobotComponents
//
// For license details, see the LICENSE file in the project root.

// Xunit Libs
using Xunit;
// Robot Components Libs
using RobotComponents.ABB.Actions.Dynamic;
using RobotComponents.ABB.Enumerations;

namespace RobotComponents.Tests.Actions
{
    public class CodeLineTests
    {
        [Fact]
        public void InstructionType_ToRAPIDInstruction_ReturnsCode()
        {
            CodeLine cl = new CodeLine("MoveL p1;");

            Assert.Equal("MoveL p1;", cl.ToRAPIDInstruction(null));
        }

        [Fact]
        public void InstructionType_ToRAPIDDeclaration_ReturnsEmpty()
        {
            CodeLine cl = new CodeLine("MoveL p1;");

            Assert.Equal("", cl.ToRAPIDDeclaration(null));
        }

        [Fact]
        public void DeclarationType_ToRAPIDDeclaration_ReturnsCode()
        {
            CodeLine cl = new CodeLine("VAR num x;", CodeType.Declaration);

            Assert.Equal("VAR num x;", cl.ToRAPIDDeclaration(null));
        }

        [Fact]
        public void DeclarationType_ToRAPIDInstruction_ReturnsEmpty()
        {
            CodeLine cl = new CodeLine("VAR num x;", CodeType.Declaration);

            Assert.Equal("", cl.ToRAPIDInstruction(null));
        }

        [Fact]
        public void DefaultType_IsInstruction()
        {
            CodeLine cl = new CodeLine("code");

            Assert.Equal(CodeType.Instruction, cl.Type);
        }

        [Fact]
        public void IsValid_NonEmpty_ReturnsTrue()
        {
            CodeLine cl = new CodeLine("abc");

            Assert.True(cl.IsValid);
        }

        [Fact]
        public void IsValid_Empty_ReturnsFalse()
        {
            CodeLine cl = new CodeLine("");

            Assert.False(cl.IsValid);
        }

        [Fact]
        public void IsValid_Null_ReturnsFalse()
        {
            CodeLine cl = new CodeLine();

            Assert.False(cl.IsValid);
        }
    }

    public class CommentTests
    {
        [Fact]
        public void InstructionType_ToRAPIDInstruction_ReturnsBangPrefix()
        {
            Comment c = new Comment("hello");

            Assert.Equal("! hello", c.ToRAPIDInstruction(null));
        }

        [Fact]
        public void InstructionType_ToRAPIDDeclaration_ReturnsEmpty()
        {
            Comment c = new Comment("hello");

            Assert.Equal("", c.ToRAPIDDeclaration(null));
        }

        [Fact]
        public void DeclarationType_ToRAPIDDeclaration_ReturnsBangPrefix()
        {
            Comment c = new Comment("hello", CodeType.Declaration);

            Assert.Equal("! hello", c.ToRAPIDDeclaration(null));
        }

        [Fact]
        public void DeclarationType_ToRAPIDInstruction_ReturnsEmpty()
        {
            Comment c = new Comment("hello", CodeType.Declaration);

            Assert.Equal("", c.ToRAPIDInstruction(null));
        }

        [Fact]
        public void DefaultType_IsInstruction()
        {
            Comment c = new Comment("text");

            Assert.Equal(CodeType.Instruction, c.Type);
        }

        [Fact]
        public void IsValid_NonEmpty_ReturnsTrue()
        {
            Comment c = new Comment("text");

            Assert.True(c.IsValid);
        }

        [Fact]
        public void IsValid_Empty_ReturnsFalse()
        {
            Comment c = new Comment("");

            Assert.False(c.IsValid);
        }

        [Fact]
        public void IsValid_Null_ReturnsFalse()
        {
            Comment c = new Comment();

            Assert.False(c.IsValid);
        }
    }
}
