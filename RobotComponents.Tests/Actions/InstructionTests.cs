// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components
// Project: https://github.com/RobotComponents/RobotComponents
//
// For license details, see the LICENSE file in the project root.

// Xunit Libs
using Xunit;
// Robot Components Libs
using RobotComponents.ABB.Actions.Instructions;
using RobotComponents.ABB.Actions.Dynamic;
using RobotComponents.ABB.Enumerations;

namespace RobotComponents.Tests.Actions
{
    public class InstructionTests
    {
        #region SetDigitalOutput
        [Fact]
        public void SetDigitalOutput_BasicTrue_ProducesSetDO()
        {
            SetDigitalOutput sdo = new SetDigitalOutput("signal", true);

            Assert.Equal("SetDO signal, 1;", sdo.ToRAPIDInstruction(null));
        }

        [Fact]
        public void SetDigitalOutput_BasicFalse_ProducesZero()
        {
            SetDigitalOutput sdo = new SetDigitalOutput("signal", false);

            Assert.Equal("SetDO signal, 0;", sdo.ToRAPIDInstruction(null));
        }

        [Fact]
        public void SetDigitalOutput_WithDelay_IncludesSDelay()
        {
            SetDigitalOutput sdo = new SetDigitalOutput("signal", true);
            sdo.Delay = 0.5;

            Assert.Equal("SetDO \\SDelay:=0.5, signal, 1;", sdo.ToRAPIDInstruction(null));
        }

        [Fact]
        public void SetDigitalOutput_WithSync_IncludesSync()
        {
            SetDigitalOutput sdo = new SetDigitalOutput("signal", true);
            sdo.Sync = true;

            Assert.Equal("SetDO \\Sync, signal, 1;", sdo.ToRAPIDInstruction(null));
        }

        [Fact]
        public void SetDigitalOutput_SyncOverridesDelay_OnlySyncAppears()
        {
            SetDigitalOutput sdo = new SetDigitalOutput("signal", true);
            sdo.Delay = 0.5;
            sdo.Sync = true;

            string result = sdo.ToRAPIDInstruction(null);

            Assert.Contains("\\Sync", result);
            Assert.DoesNotContain("\\SDelay", result);
        }

        [Fact]
        public void SetDigitalOutput_IsValid_TrueWithName()
        {
            SetDigitalOutput sdo = new SetDigitalOutput("sig", true);

            Assert.True(sdo.IsValid);
        }

        [Fact]
        public void SetDigitalOutput_IsValid_FalseWithNullName()
        {
            SetDigitalOutput sdo = new SetDigitalOutput();

            Assert.False(sdo.IsValid);
        }

        [Fact]
        public void SetDigitalOutput_IsValid_FalseWithEmptyName()
        {
            SetDigitalOutput sdo = new SetDigitalOutput("", true);

            Assert.False(sdo.IsValid);
        }

        [Fact]
        public void SetDigitalOutput_ToRAPIDDeclaration_ReturnsEmpty()
        {
            SetDigitalOutput sdo = new SetDigitalOutput("signal", true);

            Assert.Equal(string.Empty, sdo.ToRAPIDDeclaration(null));
        }
        #endregion

        #region WaitTime
        [Fact]
        public void WaitTime_Basic_ProducesWaitTimeInstruction()
        {
            WaitTime wt = new WaitTime(0.5);

            Assert.Equal("WaitTime 0.5;", wt.ToRAPIDInstruction(null));
        }

        [Fact]
        public void WaitTime_IntegerDuration_OmitsDecimalTrailingZeros()
        {
            // Format {_duration:0.###} => 1.0 becomes "1"
            WaitTime wt = new WaitTime(1.0);

            Assert.Equal("WaitTime 1;", wt.ToRAPIDInstruction(null));
        }

        [Fact]
        public void WaitTime_InPosition_IncludesInPosFlag()
        {
            WaitTime wt = new WaitTime(1.0, true);

            Assert.Equal("WaitTime \\InPos, 1;", wt.ToRAPIDInstruction(null));
        }

        [Fact]
        public void WaitTime_IsValid_ZeroDuration_ReturnsTrue()
        {
            WaitTime wt = new WaitTime(0);

            Assert.True(wt.IsValid);
        }

        [Fact]
        public void WaitTime_IsValid_NegativeDuration_ReturnsFalse()
        {
            WaitTime wt = new WaitTime(-1);

            Assert.False(wt.IsValid);
        }

        [Fact]
        public void WaitTime_ToRAPIDDeclaration_ReturnsEmpty()
        {
            WaitTime wt = new WaitTime(1.0);

            Assert.Equal(string.Empty, wt.ToRAPIDDeclaration(null));
        }
        #endregion

        #region WaitDI
        [Fact]
        public void WaitDI_BasicTrue_ProducesWaitDIInstruction()
        {
            WaitDI wdi = new WaitDI("di1", true);

            Assert.Equal("WaitDI di1, 1;", wdi.ToRAPIDInstruction(null));
        }

        [Fact]
        public void WaitDI_BasicFalse_ProducesZero()
        {
            WaitDI wdi = new WaitDI("di1", false);

            Assert.Equal("WaitDI di1, 0;", wdi.ToRAPIDInstruction(null));
        }

        [Fact]
        public void WaitDI_WithMaxTimeAndTimeFlag_IncludesBoth()
        {
            WaitDI wdi = new WaitDI("di1", false, 5.0, true);

            string result = wdi.ToRAPIDInstruction(null);

            Assert.Contains("\\MaxTime:=5", result);
            Assert.Contains("\\TimeFlag:=TRUE", result);
        }

        [Fact]
        public void WaitDI_WithMaxTimeFlagFalse_IncludesFalseFlag()
        {
            WaitDI wdi = new WaitDI("di1", true, 10.0, false);

            string result = wdi.ToRAPIDInstruction(null);

            Assert.Contains("\\TimeFlag:=FALSE", result);
        }

        [Fact]
        public void WaitDI_IsValid_TrueWithName()
        {
            WaitDI wdi = new WaitDI("di1", true);

            Assert.True(wdi.IsValid);
        }

        [Fact]
        public void WaitDI_IsValid_FalseWithNullName()
        {
            WaitDI wdi = new WaitDI();

            Assert.False(wdi.IsValid);
        }
        #endregion

        #region WaitDO
        [Fact]
        public void WaitDO_BasicTrue_ProducesWaitDOInstruction()
        {
            WaitDO wdo = new WaitDO("do1", true);

            Assert.Equal("WaitDO do1, 1;", wdo.ToRAPIDInstruction(null));
        }

        [Fact]
        public void WaitDO_WithMaxTime_IncludesMaxTimeAndTimeFlag()
        {
            WaitDO wdo = new WaitDO("do1", false, 5.0, true);

            string result = wdo.ToRAPIDInstruction(null);

            Assert.Contains("WaitDO", result);
            Assert.Contains("\\MaxTime:=5", result);
            Assert.Contains("\\TimeFlag:=TRUE", result);
        }
        #endregion

        #region WaitAI
        [Fact]
        public void WaitAI_LessThan_ProducesCorrectInstruction()
        {
            WaitAI wai = new WaitAI("ai1", 5.0, InequalitySymbol.LT);

            Assert.Equal("WaitAI ai1, \\LT, 5;", wai.ToRAPIDInstruction(null));
        }

        [Fact]
        public void WaitAI_GreaterThan_ProducesCorrectInstruction()
        {
            WaitAI wai = new WaitAI("ai1", 5.0, InequalitySymbol.GT);

            Assert.Equal("WaitAI ai1, \\GT, 5;", wai.ToRAPIDInstruction(null));
        }

        [Fact]
        public void WaitAI_WithMaxTime_IncludesMaxTimeParam()
        {
            WaitAI wai = new WaitAI("ai1", 5.0, InequalitySymbol.LT, 10);

            string result = wai.ToRAPIDInstruction(null);

            Assert.Contains("\\MaxTime:=10", result);
        }

        [Fact]
        public void WaitAI_IsValid_TrueWithName()
        {
            WaitAI wai = new WaitAI("ai1", 5.0, InequalitySymbol.LT);

            Assert.True(wai.IsValid);
        }
        #endregion

        #region WaitAO
        [Fact]
        public void WaitAO_LessThan_ProducesCorrectInstruction()
        {
            WaitAO wao = new WaitAO("ao1", 5.0, InequalitySymbol.LT);

            Assert.Equal("WaitAO ao1, \\LT, 5;", wao.ToRAPIDInstruction(null));
        }

        [Fact]
        public void WaitAO_WithMaxTime_IncludesMaxTimeParam()
        {
            WaitAO wao = new WaitAO("ao1", 5.0, InequalitySymbol.GT, 10);

            string result = wao.ToRAPIDInstruction(null);

            Assert.Contains("\\MaxTime:=10", result);
        }
        #endregion

        #region WaitGI
        [Fact]
        public void WaitGI_Basic_ProducesWaitGIInstruction()
        {
            WaitGI wgi = new WaitGI("gi1", 42);

            Assert.Equal("WaitGI gi1, 42;", wgi.ToRAPIDInstruction(null));
        }

        [Fact]
        public void WaitGI_WithMaxTime_IncludesMaxTimeParam()
        {
            WaitGI wgi = new WaitGI("gi1", 42, 5.0);

            string result = wgi.ToRAPIDInstruction(null);

            Assert.Contains("\\MaxTime:=5", result);
        }

        [Fact]
        public void WaitGI_IsValid_TrueWithName()
        {
            WaitGI wgi = new WaitGI("gi1", 42);

            Assert.True(wgi.IsValid);
        }
        #endregion

        #region WaitGO
        [Fact]
        public void WaitGO_Basic_ProducesWaitGOInstruction()
        {
            WaitGO wgo = new WaitGO("go1", 42);

            Assert.Equal("WaitGO go1, 42;", wgo.ToRAPIDInstruction(null));
        }

        [Fact]
        public void WaitGO_WithMaxTime_IncludesMaxTimeParam()
        {
            WaitGO wgo = new WaitGO("go1", 42, 5.0);

            string result = wgo.ToRAPIDInstruction(null);

            Assert.Contains("\\MaxTime:=5", result);
        }
        #endregion

        #region WaitRob
        [Fact]
        public void WaitRob_InPosition_ProducesInPosInstruction()
        {
            WaitRob wr = new WaitRob(false);

            Assert.Equal("WaitRob \\InPos;", wr.ToRAPIDInstruction(null));
        }

        [Fact]
        public void WaitRob_ZeroSpeed_ProducesZeroSpeedInstruction()
        {
            WaitRob wr = new WaitRob(true);

            Assert.Equal("WaitRob \\ZeroSpeed;", wr.ToRAPIDInstruction(null));
        }

        [Fact]
        public void WaitRob_Default_IsNotValid()
        {
            WaitRob wr = new WaitRob();

            Assert.False(wr.IsValid);
        }

        [Fact]
        public void WaitRob_InPosition_IsValid()
        {
            WaitRob wr = new WaitRob(false);

            Assert.True(wr.IsValid);
            Assert.True(wr.InPosition);
            Assert.False(wr.ZeroSpeed);
        }

        [Fact]
        public void WaitRob_ZeroSpeed_IsValid()
        {
            WaitRob wr = new WaitRob(true);

            Assert.True(wr.IsValid);
            Assert.False(wr.InPosition);
            Assert.True(wr.ZeroSpeed);
        }

        [Fact]
        public void WaitRob_BothTrue_IsNotValid()
        {
            WaitRob wr = new WaitRob();
            wr.InPosition = true;
            wr.ZeroSpeed = true;

            Assert.False(wr.IsValid);
        }

        [Fact]
        public void WaitRob_ToRAPIDDeclaration_ReturnsEmpty()
        {
            WaitRob wr = new WaitRob(false);

            Assert.Equal(string.Empty, wr.ToRAPIDDeclaration(null));
        }
        #endregion

        #region CodeLine
        [Fact]
        public void CodeLine_InstructionType_ToRAPIDInstruction_ReturnsCode()
        {
            CodeLine cl = new CodeLine("MoveL p1;");

            Assert.Equal("MoveL p1;", cl.ToRAPIDInstruction(null));
        }

        [Fact]
        public void CodeLine_InstructionType_ToRAPIDDeclaration_ReturnsEmpty()
        {
            CodeLine cl = new CodeLine("MoveL p1;");

            Assert.Equal("", cl.ToRAPIDDeclaration(null));
        }

        [Fact]
        public void CodeLine_DeclarationType_ToRAPIDDeclaration_ReturnsCode()
        {
            CodeLine cl = new CodeLine("VAR num x;", CodeType.Declaration);

            Assert.Equal("VAR num x;", cl.ToRAPIDDeclaration(null));
        }

        [Fact]
        public void CodeLine_DeclarationType_ToRAPIDInstruction_ReturnsEmpty()
        {
            CodeLine cl = new CodeLine("VAR num x;", CodeType.Declaration);

            Assert.Equal("", cl.ToRAPIDInstruction(null));
        }

        [Fact]
        public void CodeLine_DefaultType_IsInstruction()
        {
            CodeLine cl = new CodeLine("code");

            Assert.Equal(CodeType.Instruction, cl.Type);
        }

        [Fact]
        public void CodeLine_IsValid_NonEmpty_ReturnsTrue()
        {
            CodeLine cl = new CodeLine("abc");

            Assert.True(cl.IsValid);
        }

        [Fact]
        public void CodeLine_IsValid_Empty_ReturnsFalse()
        {
            CodeLine cl = new CodeLine("");

            Assert.False(cl.IsValid);
        }

        [Fact]
        public void CodeLine_IsValid_Null_ReturnsFalse()
        {
            CodeLine cl = new CodeLine();

            Assert.False(cl.IsValid);
        }
        #endregion

        #region Comment
        [Fact]
        public void Comment_InstructionType_ToRAPIDInstruction_ReturnsBangPrefix()
        {
            Comment c = new Comment("hello");

            Assert.Equal("! hello", c.ToRAPIDInstruction(null));
        }

        [Fact]
        public void Comment_InstructionType_ToRAPIDDeclaration_ReturnsEmpty()
        {
            Comment c = new Comment("hello");

            Assert.Equal("", c.ToRAPIDDeclaration(null));
        }

        [Fact]
        public void Comment_DeclarationType_ToRAPIDDeclaration_ReturnsBangPrefix()
        {
            Comment c = new Comment("hello", CodeType.Declaration);

            Assert.Equal("! hello", c.ToRAPIDDeclaration(null));
        }

        [Fact]
        public void Comment_DeclarationType_ToRAPIDInstruction_ReturnsEmpty()
        {
            Comment c = new Comment("hello", CodeType.Declaration);

            Assert.Equal("", c.ToRAPIDInstruction(null));
        }

        [Fact]
        public void Comment_DefaultType_IsInstruction()
        {
            Comment c = new Comment("text");

            Assert.Equal(CodeType.Instruction, c.Type);
        }

        [Fact]
        public void Comment_IsValid_NonEmpty_ReturnsTrue()
        {
            Comment c = new Comment("text");

            Assert.True(c.IsValid);
        }

        [Fact]
        public void Comment_IsValid_Empty_ReturnsFalse()
        {
            Comment c = new Comment("");

            Assert.False(c.IsValid);
        }

        [Fact]
        public void Comment_IsValid_Null_ReturnsFalse()
        {
            Comment c = new Comment();

            Assert.False(c.IsValid);
        }
        #endregion
    }
}
