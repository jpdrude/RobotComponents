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
using RobotComponents.ABB.Utils;

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

    public class CodeLineSanitizationTests
    {
        [Fact]
        public void Sanitize_NormalCode_NoWarnings()
        {
            RapidCodeLineSanitizer.SanitizeResult result = RapidCodeLineSanitizer.Sanitize("MoveL p1, v100, fine, tool0;");

            Assert.Equal("MoveL p1, v100, fine, tool0;", result.Code);
            Assert.Empty(result.Warnings);
        }

        [Fact]
        public void Sanitize_StripsNewlines()
        {
            RapidCodeLineSanitizer.SanitizeResult result = RapidCodeLineSanitizer.Sanitize("line1\nline2");

            Assert.Equal("line1 line2", result.Code);
            Assert.Single(result.Warnings);
        }

        [Fact]
        public void Sanitize_StripsCarriageReturnNewline()
        {
            RapidCodeLineSanitizer.SanitizeResult result = RapidCodeLineSanitizer.Sanitize("line1\r\nline2");

            Assert.Equal("line1 line2", result.Code);
            Assert.Single(result.Warnings);
        }

        [Fact]
        public void Sanitize_DetectsEndProc()
        {
            RapidCodeLineSanitizer.SanitizeResult result = RapidCodeLineSanitizer.Sanitize("ENDPROC");

            Assert.True(result.HasWarnings);
            Assert.Contains(result.Warnings, w => w.Contains("ENDPROC"));
        }

        [Fact]
        public void Sanitize_DetectsEndModule()
        {
            RapidCodeLineSanitizer.SanitizeResult result = RapidCodeLineSanitizer.Sanitize("ENDMODULE");

            Assert.True(result.HasWarnings);
            Assert.Contains(result.Warnings, w => w.Contains("ENDMODULE"));
        }

        [Fact]
        public void Sanitize_DetectsProc()
        {
            RapidCodeLineSanitizer.SanitizeResult result = RapidCodeLineSanitizer.Sanitize("PROC myproc()");

            Assert.True(result.HasWarnings);
            Assert.Contains(result.Warnings, w => w.Contains("PROC"));
        }

        [Fact]
        public void Sanitize_DetectsCaseInsensitive()
        {
            RapidCodeLineSanitizer.SanitizeResult result = RapidCodeLineSanitizer.Sanitize("endproc");

            Assert.True(result.HasWarnings);
            Assert.Contains(result.Warnings, w => w.Contains("ENDPROC"));
        }

        [Fact]
        public void Sanitize_AllowsProcInContext()
        {
            // "proc" as part of a longer word should NOT trigger a warning
            RapidCodeLineSanitizer.SanitizeResult result = RapidCodeLineSanitizer.Sanitize("MoveL proc_target, v50, fine, tool0;");

            Assert.False(result.HasWarnings);
        }

        [Fact]
        public void Sanitize_MultipleIssues()
        {
            RapidCodeLineSanitizer.SanitizeResult result = RapidCodeLineSanitizer.Sanitize("ENDPROC\nENDMODULE");

            // Should have: newline warning + ENDPROC warning + ENDMODULE warning
            Assert.True(result.Warnings.Count >= 3);
        }

        [Fact]
        public void Sanitize_NullInput_NoWarnings()
        {
            RapidCodeLineSanitizer.SanitizeResult result = RapidCodeLineSanitizer.Sanitize(null);

            Assert.Null(result.Code);
            Assert.Empty(result.Warnings);
        }

        [Fact]
        public void Sanitize_EmptyInput_NoWarnings()
        {
            RapidCodeLineSanitizer.SanitizeResult result = RapidCodeLineSanitizer.Sanitize("");

            Assert.Equal("", result.Code);
            Assert.Empty(result.Warnings);
        }

        [Fact]
        public void Sanitize_DetectsModule()
        {
            RapidCodeLineSanitizer.SanitizeResult result = RapidCodeLineSanitizer.Sanitize("MODULE evil");

            Assert.True(result.HasWarnings);
            Assert.Contains(result.Warnings, w => w.Contains("MODULE"));
        }

        [Fact]
        public void Sanitize_DetectsTrap()
        {
            RapidCodeLineSanitizer.SanitizeResult result = RapidCodeLineSanitizer.Sanitize("TRAP myhandler");

            Assert.True(result.HasWarnings);
            Assert.Contains(result.Warnings, w => w.Contains("TRAP"));
        }

        [Fact]
        public void CodeLine_WithStructuralKeyword_IsInvalid()
        {
            CodeLine cl = new CodeLine("ENDPROC");

            Assert.False(cl.IsValid);
            Assert.NotEmpty(cl.Warnings);
        }

        [Fact]
        public void CodeLine_WithSafeCode_IsValid()
        {
            CodeLine cl = new CodeLine("MoveL p1, v100, fine, tool0;");

            Assert.True(cl.IsValid);
            Assert.Empty(cl.Warnings);
        }

        [Fact]
        public void CodeLine_SetterSanitizes()
        {
            CodeLine cl = new CodeLine("safe code;");
            Assert.True(cl.IsValid);

            cl.Code = "ENDPROC";
            Assert.False(cl.IsValid);
            Assert.NotEmpty(cl.Warnings);
        }

        [Fact]
        public void CodeLine_NewlinesSanitizedInOutput()
        {
            CodeLine cl = new CodeLine("MoveL p1;\nMoveL p2;");

            // Newlines should be replaced with spaces
            Assert.DoesNotContain("\n", cl.Code);
            Assert.Equal("MoveL p1; MoveL p2;", cl.Code);
        }

        [Fact]
        public void CodeLine_VarDeclaration_IsValid()
        {
            CodeLine cl = new CodeLine("VAR num x := 5;", CodeType.Declaration);

            Assert.True(cl.IsValid);
            Assert.Empty(cl.Warnings);
        }

        [Fact]
        public void CodeLine_InjectionWithSemicolon_NoStructuralWarning()
        {
            // Semicolons are normal in RAPID — only structural keywords are flagged
            CodeLine cl = new CodeLine("SetDO signal1, 1;");

            Assert.True(cl.IsValid);
            Assert.Empty(cl.Warnings);
        }

        [Fact]
        public void CodeLine_FlaggedCode_ToRAPIDInstruction_StillReturnsCode()
        {
            // ToRAPIDInstruction returns the raw string (used for preview);
            // the guard is in ToRAPIDGenerator
            CodeLine cl = new CodeLine("ENDPROC");

            Assert.Equal("ENDPROC", cl.ToRAPIDInstruction(null));
        }

        [Fact]
        public void CodeLine_FlaggedCode_ToRAPIDDeclaration_StillReturnsCode()
        {
            CodeLine cl = new CodeLine("ENDMODULE", CodeType.Declaration);

            Assert.Equal("ENDMODULE", cl.ToRAPIDDeclaration(null));
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

        [Fact]
        public void NewlineStripped_SingleArg()
        {
            Comment c = new Comment("line1\nMoveL dangerous;");

            Assert.DoesNotContain("\n", c.Com);
            Assert.Equal("! line1 MoveL dangerous;", c.ToRAPIDInstruction(null));
        }

        [Fact]
        public void NewlineStripped_TwoArg()
        {
            Comment c = new Comment("line1\r\nline2", CodeType.Declaration);

            Assert.DoesNotContain("\r", c.Com);
            Assert.DoesNotContain("\n", c.Com);
            Assert.Equal("! line1 line2", c.ToRAPIDDeclaration(null));
        }

        [Fact]
        public void SetterStripsNewlines()
        {
            Comment c = new Comment("safe");
            c.Com = "text\ninjected;";

            Assert.DoesNotContain("\n", c.Com);
            Assert.Equal("text injected;", c.Com);
        }

        [Fact]
        public void NullComment_NoException()
        {
            Comment c = new Comment();
            c.Com = null;

            Assert.Null(c.Com);
        }

        [Fact]
        public void EmptyComment_NoException()
        {
            Comment c = new Comment("");

            Assert.Equal("", c.Com);
        }

        [Fact]
        public void NoNewline_Unchanged()
        {
            Comment c = new Comment("normal comment text");

            Assert.Equal("normal comment text", c.Com);
            Assert.Equal("! normal comment text", c.ToRAPIDInstruction(null));
        }

        [Fact]
        public void CarriageReturn_Stripped()
        {
            Comment c = new Comment("line1\rline2");

            Assert.DoesNotContain("\r", c.Com);
            Assert.Equal("line1 line2", c.Com);
        }
    }
}
