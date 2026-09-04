// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components
// Project: https://github.com/RobotComponents/RobotComponents
//
// For license details, see the LICENSE file in the project root.

// System Libs
using System;
using System.Collections.Generic;
using System.Linq;
// Rhino Libs
using Rhino.Geometry;
// Xunit Libs
using Xunit;
// Robot Components Libs
using RobotComponents.ABB.Actions;
using RobotComponents.ABB.Actions.Declarations;
using RobotComponents.ABB.Actions.Dynamic;
using RobotComponents.ABB.Actions.Instructions;
using RobotComponents.ABB.Definitions;
using RobotComponents.ABB.Enumerations;
using RobotComponents.ABB.Presets;
using RobotComponents.ABB.Presets.Enumerations;

namespace RobotComponents.Tests.Actions
{
    [Collection("RequiresRhino")]
    [Trait("Category", "RequiresRhino")]
    public class RAPIDGeneratorTests
    {
        private Robot CreateTestRobot()
        {
            return Factory.GetRobotPreset(RobotPreset.IRB120_3_058, Plane.WorldXY);
        }

        #region Module Structure
        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void CreateModule_EmptyActions_ProducesModuleStructure()
        {
            Robot robot = CreateTestRobot();
            RAPIDGenerator generator = new RAPIDGenerator(robot);
            List<string> module = generator.CreateModule(new List<IAction>());

            string joined = string.Join(Environment.NewLine, module);

            Assert.Contains("MODULE MainModule", joined);
            Assert.Contains("ENDMODULE", joined);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void CreateModule_WithAction_ProducesProcAndEndproc()
        {
            Robot robot = CreateTestRobot();
            RAPIDGenerator generator = new RAPIDGenerator(robot);

            RobotJointPosition rjp = new RobotJointPosition(0, 0, 0, 0, 0, 0);
            JointTarget target = new JointTarget("jt1", rjp);
            Movement move = new Movement(MovementType.MoveAbsJ, target, new SpeedData(100), new ZoneData(10));

            List<string> module = generator.CreateModule(new List<IAction> { move });
            string joined = string.Join(Environment.NewLine, module);

            Assert.Contains("PROC main()", joined);
            Assert.Contains("ENDPROC", joined);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void CreateModule_CustomModuleName_UsesCustomName()
        {
            Robot robot = CreateTestRobot();
            RAPIDGenerator generator = new RAPIDGenerator(robot, "CustomModule", "myProc");

            RobotJointPosition rjp = new RobotJointPosition(0, 0, 0, 0, 0, 0);
            JointTarget target = new JointTarget("jt1", rjp);
            Movement move = new Movement(MovementType.MoveAbsJ, target, new SpeedData(100), new ZoneData(10));

            List<string> module = generator.CreateModule(new List<IAction> { move });
            string joined = string.Join(Environment.NewLine, module);

            Assert.Contains("MODULE CustomModule", joined);
            Assert.Contains("PROC myProc()", joined);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void CreateModule_EmptyActions_OmitsProcBlock()
        {
            Robot robot = CreateTestRobot();
            RAPIDGenerator generator = new RAPIDGenerator(robot, "CustomModule", "myProc");
            List<string> module = generator.CreateModule(new List<IAction>());

            string joined = string.Join(Environment.NewLine, module);

            Assert.Contains("MODULE CustomModule", joined);
            Assert.DoesNotContain("PROC", joined);
            Assert.DoesNotContain("ENDPROC", joined);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void CreateModule_VersionComment_IncludesRCVersion()
        {
            Robot robot = CreateTestRobot();
            RAPIDGenerator generator = new RAPIDGenerator(robot);
            List<string> module = generator.CreateModule(new List<IAction>());

            string joined = string.Join(Environment.NewLine, module);

            Assert.Contains("! This RAPID code was generated with a modified version of RobotComponents", joined);
        }
        #endregion

        #region Deduplication
        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void CreateModule_SpeedDataDeduplication_OnlyOneDeclaration()
        {
            Robot robot = CreateTestRobot();
            RAPIDGenerator generator = new RAPIDGenerator(robot);

            SpeedData customSpeed = new SpeedData("mySpeed", 250, 500, 5000, 1000);
            RobotJointPosition rjp = new RobotJointPosition(0, 0, 0, 0, 0, 0);
            JointTarget jt1 = new JointTarget("jt1", rjp);
            JointTarget jt2 = new JointTarget("jt2", rjp);

            Movement move1 = new Movement(MovementType.MoveAbsJ, jt1, customSpeed, new ZoneData(10));
            Movement move2 = new Movement(MovementType.MoveAbsJ, jt2, customSpeed, new ZoneData(10));

            List<string> module = generator.CreateModule(new List<IAction> { move1, move2 });

            // Count occurrences of the declaration
            int count = module.Count(line => line.Contains("VAR speeddata mySpeed"));
            Assert.Equal(1, count);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void CreateModule_ZoneDataDeduplication_OnlyOneDeclaration()
        {
            Robot robot = CreateTestRobot();
            RAPIDGenerator generator = new RAPIDGenerator(robot);

            ZoneData customZone = new ZoneData("myZone", false, 5, 10, 10, 1, 10, 1);
            RobotJointPosition rjp = new RobotJointPosition(0, 0, 0, 0, 0, 0);
            JointTarget jt1 = new JointTarget("jt1", rjp);
            JointTarget jt2 = new JointTarget("jt2", rjp);

            Movement move1 = new Movement(MovementType.MoveAbsJ, jt1, new SpeedData(100), customZone);
            Movement move2 = new Movement(MovementType.MoveAbsJ, jt2, new SpeedData(100), customZone);

            List<string> module = generator.CreateModule(new List<IAction> { move1, move2 });

            int count = module.Count(line => line.Contains("VAR zonedata myZone"));
            Assert.Equal(1, count);
        }
        #endregion

        #region Instruction Order
        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void CreateModule_InstructionOrder_MatchesInputOrder()
        {
            Robot robot = CreateTestRobot();
            RAPIDGenerator generator = new RAPIDGenerator(robot);

            RobotJointPosition rjp = new RobotJointPosition(0, 0, 0, 0, 0, 0);
            JointTarget jt1 = new JointTarget("first", rjp);
            JointTarget jt2 = new JointTarget("second", rjp);
            JointTarget jt3 = new JointTarget("third", rjp);

            Movement move1 = new Movement(MovementType.MoveAbsJ, jt1, new SpeedData(100), new ZoneData(10));
            Movement move2 = new Movement(MovementType.MoveAbsJ, jt2, new SpeedData(100), new ZoneData(10));
            Movement move3 = new Movement(MovementType.MoveAbsJ, jt3, new SpeedData(100), new ZoneData(10));

            List<string> module = generator.CreateModule(new List<IAction> { move1, move2, move3 });

            // Find lines containing each target name in instruction section
            int idx1 = module.FindIndex(l => l.Contains("MoveAbsJ") && l.Contains("first"));
            int idx2 = module.FindIndex(l => l.Contains("MoveAbsJ") && l.Contains("second"));
            int idx3 = module.FindIndex(l => l.Contains("MoveAbsJ") && l.Contains("third"));

            Assert.True(idx1 < idx2, "first should come before second");
            Assert.True(idx2 < idx3, "second should come before third");
        }
        #endregion

        #region Declaration Sorting
        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void CreateModule_DeclarationsSorted_Alphabetically()
        {
            Robot robot = CreateTestRobot();
            RAPIDGenerator generator = new RAPIDGenerator(robot);

            RobotJointPosition rjp = new RobotJointPosition(0, 0, 0, 0, 0, 0);
            JointTarget jtZ = new JointTarget("zzz", rjp);
            JointTarget jtA = new JointTarget("aaa", rjp);
            JointTarget jtM = new JointTarget("mmm", rjp);

            Movement moveZ = new Movement(MovementType.MoveAbsJ, jtZ, new SpeedData(100), new ZoneData(10));
            Movement moveA = new Movement(MovementType.MoveAbsJ, jtA, new SpeedData(100), new ZoneData(10));
            Movement moveM = new Movement(MovementType.MoveAbsJ, jtM, new SpeedData(100), new ZoneData(10));

            List<string> module = generator.CreateModule(new List<IAction> { moveZ, moveA, moveM });

            // Find declaration lines (VAR jointtarget)
            List<int> declIndices = new List<int>();
            for (int i = 0; i < module.Count; i++)
            {
                if (module[i].Contains("VAR jointtarget"))
                {
                    declIndices.Add(i);
                }
            }

            // There should be at least 3 declarations (zzz_jt, aaa_jt, mmm_jt)
            Assert.True(declIndices.Count >= 3, $"Expected at least 3 jointtarget declarations, found {declIndices.Count}");

            // Verify alphabetical order of declaration lines
            for (int i = 1; i < declIndices.Count; i++)
            {
                string prev = module[declIndices[i - 1]].Trim();
                string curr = module[declIndices[i]].Trim();
                Assert.True(string.Compare(prev, curr, StringComparison.Ordinal) <= 0,
                    $"Declarations not sorted: '{prev}' should come before '{curr}'");
            }
        }
        #endregion

        #region Declaration Comments
        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void CreateModule_DeclarationComments_InterleaveWithCustomCodeLineDeclarations()
        {
            // A declaration-type Comment must land in the same section, in the same relative
            // order, as declaration-type CodeLines (e.g. a RAPID Variable's own declaration output,
            // or CodeLineComponent's custom code lines) — both are user-authored declaration text,
            // as opposed to the implicit declarations Movement/Target objects generate on their own.
            Robot robot = CreateTestRobot();
            RAPIDGenerator generator = new RAPIDGenerator(robot);

            CodeLine declA = new CodeLine("VAR num a := 1;", CodeType.Declaration);
            Comment comment = new Comment("comment between a and b", CodeType.Declaration);
            CodeLine declB = new CodeLine("VAR num b := 2;", CodeType.Declaration);

            List<string> module = generator.CreateModule(new List<IAction> { declA, comment, declB });

            int idxA = module.FindIndex(l => l.Contains("VAR num a"));
            int idxComment = module.FindIndex(l => l.Contains("comment between a and b"));
            int idxB = module.FindIndex(l => l.Contains("VAR num b"));

            Assert.True(idxA >= 0, "Declaration 'a' should be present in the module.");
            Assert.True(idxComment >= 0, "The comment should be present in the module.");
            Assert.True(idxB >= 0, "Declaration 'b' should be present in the module.");

            Assert.True(idxA < idxComment, "Declaration 'a' should come before the comment.");
            Assert.True(idxComment < idxB, "The comment should come before declaration 'b'.");
        }
        #endregion

        #region Optional Sections
        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void CreateModule_AddTooldataFalse_OmitsTooldata()
        {
            Robot robot = CreateTestRobot();
            RAPIDGenerator generator = new RAPIDGenerator(robot);
            List<string> module = generator.CreateModule(new List<IAction>(), addTooldata: false);

            string joined = string.Join(Environment.NewLine, module);

            Assert.DoesNotContain("! User defined tooldata", joined);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void CreateModule_AddWobjdataFalse_OmitsWobjdata()
        {
            Robot robot = CreateTestRobot();
            RAPIDGenerator generator = new RAPIDGenerator(robot);
            List<string> module = generator.CreateModule(new List<IAction>(), addWobjdata: false);

            string joined = string.Join(Environment.NewLine, module);

            Assert.DoesNotContain("! User defined wobjdata", joined);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void CreateModule_AddLoaddataFalse_OmitsLoaddata()
        {
            Robot robot = CreateTestRobot();
            RAPIDGenerator generator = new RAPIDGenerator(robot);
            List<string> module = generator.CreateModule(new List<IAction>(), addLoaddata: false);

            string joined = string.Join(Environment.NewLine, module);

            Assert.DoesNotContain("! User defined loaddata", joined);
        }
        #endregion

        #region Mixed Actions
        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void CreateModule_MixedActions_CorrectSectionLayout()
        {
            Robot robot = CreateTestRobot();
            RAPIDGenerator generator = new RAPIDGenerator(robot);

            RobotJointPosition rjp = new RobotJointPosition(0, 0, 0, 0, 0, 0);
            JointTarget target = new JointTarget("jt1", rjp);
            SpeedData customSpeed = new SpeedData("mySpeed", 250, 500, 5000, 1000);
            Movement move = new Movement(MovementType.MoveAbsJ, target, customSpeed, new ZoneData(10));

            List<string> module = generator.CreateModule(new List<IAction> { move });

            // Find key sections
            int moduleStart = module.FindIndex(l => l.Contains("MODULE"));
            int declLine = module.FindIndex(l => l.Contains("VAR speeddata"));
            int procLine = module.FindIndex(l => l.Contains("PROC main()"));
            int instrLine = module.FindIndex(l => l.Contains("MoveAbsJ"));
            int endProcLine = module.FindIndex(l => l.Contains("ENDPROC"));
            int endModuleLine = module.FindIndex(l => l.Contains("ENDMODULE"));

            // Verify ordering: MODULE → declarations → PROC → instructions → ENDPROC → ENDMODULE
            Assert.True(moduleStart < declLine, "MODULE should come before declarations");
            Assert.True(declLine < procLine, "Declarations should come before PROC");
            Assert.True(procLine < instrLine, "PROC should come before instructions");
            Assert.True(instrLine < endProcLine, "Instructions should come before ENDPROC");
            Assert.True(endProcLine < endModuleLine, "ENDPROC should come before ENDMODULE");
        }
        #endregion

        #region Additional Routines
        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void CreateModule_WithAdditionalProc_ContainsProcSignature()
        {
            Robot robot = CreateTestRobot();
            var routine = new Routine(new List<IAction>(), RoutineType.PROC, "myRoutine");
            var generator = new RAPIDGenerator(robot, "MainModule", "main", Scope.GLOBAL, null,
                new List<Routine> { routine });

            string joined = string.Join(Environment.NewLine, generator.CreateModule(new List<IAction>()));

            Assert.Contains("PROC myRoutine()", joined);
            Assert.Contains("ENDPROC", joined);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void CreateModule_WithAdditionalProc_InstructionsAppearedInBody()
        {
            Robot robot = CreateTestRobot();
            var actions = new List<IAction> { new CodeLine("x := 1;", CodeType.Instruction) };
            var routine = new Routine(actions, RoutineType.PROC, "myRoutine");
            var generator = new RAPIDGenerator(robot, "MainModule", "main", Scope.GLOBAL, null,
                new List<Routine> { routine });

            List<string> module = generator.CreateModule(new List<IAction>());
            int procLine    = module.FindIndex(l => l.Contains("PROC myRoutine()"));
            int endProcLine = module.FindLastIndex(l => l.Contains("ENDPROC"));
            int instrLine   = module.FindIndex(l => l.Contains("x := 1;"));

            Assert.True(procLine >= 0,  "PROC myRoutine() not found");
            Assert.True(instrLine > procLine && instrLine < endProcLine,
                "Instruction should appear inside the routine body");
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void CreateModule_WithAdditionalProc_DeclarationsHoisted()
        {
            Robot robot = CreateTestRobot();
            var actions = new List<IAction> { new CodeLine("VAR num myVar := 0;", CodeType.Declaration) };
            var routine = new Routine(actions, RoutineType.PROC, "myRoutine");
            var generator = new RAPIDGenerator(robot, "MainModule", "main", Scope.GLOBAL, null,
                new List<Routine> { routine });

            List<string> module = generator.CreateModule(new List<IAction>());
            int declLine = module.FindIndex(l => l.Contains("myVar"));
            int procLine = module.FindIndex(l => l.Contains("PROC myRoutine()"));

            Assert.True(declLine >= 0, "Declaration 'myVar' not found in module");
            Assert.True(declLine < procLine, "Declaration should be hoisted before the PROC block");
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void CreateModule_WithAdditionalFunc_ContainsFuncSignature()
        {
            Robot robot = CreateTestRobot();
            var actions = new List<IAction> { new CodeLine("RETURN 42;", CodeType.Instruction) };
            var routine = new Routine(actions, RoutineType.FUNC, "num", "getVal");
            var generator = new RAPIDGenerator(robot, "MainModule", "main", Scope.GLOBAL, null,
                new List<Routine> { routine });

            string joined = string.Join(Environment.NewLine, generator.CreateModule(new List<IAction>()));

            Assert.Contains("FUNC num getVal()", joined);
            Assert.Contains("ENDFUNC", joined);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void CreateModule_WithAdditionalTrap_ContainsTrapSignature()
        {
            Robot robot = CreateTestRobot();
            var actions = new List<IAction> { new CodeLine("x := 1;", CodeType.Instruction) };
            var routine = new Routine(actions, RoutineType.TRAP, "myTrap");
            var generator = new RAPIDGenerator(robot, "MainModule", "main", Scope.GLOBAL, null,
                new List<Routine> { routine });

            string joined = string.Join(Environment.NewLine, generator.CreateModule(new List<IAction>()));

            Assert.Contains("TRAP myTrap", joined);
            Assert.Contains("ENDTRAP", joined);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void CreateModule_AdditionalProcWithArguments_ContainsArgsInSignature()
        {
            Robot robot = CreateTestRobot();
            var args = new List<RoutineArgument>
            {
                new RoutineArgument("num", "x"),
                new RoutineArgument("string", "msg")
            };
            var routine = new Routine(new List<IAction>(), RoutineType.PROC, "myRoutine", Scope.GLOBAL, args);
            var generator = new RAPIDGenerator(robot, "MainModule", "main", Scope.GLOBAL, null,
                new List<Routine> { routine });

            string joined = string.Join(Environment.NewLine, generator.CreateModule(new List<IAction>()));

            Assert.Contains("PROC myRoutine(num x, string msg)", joined);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void CreateModule_AdditionalRoutineAfterMainProc_OrderCorrect()
        {
            Robot robot = CreateTestRobot();
            RobotJointPosition rjp  = new RobotJointPosition(0, 0, 0, 0, 0, 0);
            Movement move = new Movement(MovementType.MoveAbsJ,
                new JointTarget("jt1", rjp), new SpeedData(100), new ZoneData(10));

            var routine = new Routine(
                new List<IAction> { new CodeLine("x := 1;", CodeType.Instruction) },
                RoutineType.PROC, "helper");
            var generator = new RAPIDGenerator(robot, "MainModule", "main", Scope.GLOBAL, null,
                new List<Routine> { routine });

            List<string> module = generator.CreateModule(new List<IAction> { move });
            int mainEndProc  = module.FindIndex(l => l.Contains("ENDPROC"));
            int helperProc   = module.FindIndex(l => l.Contains("PROC helper()"));

            Assert.True(mainEndProc >= 0, "Main ENDPROC not found");
            Assert.True(helperProc  >= 0, "PROC helper() not found");
            Assert.True(helperProc > mainEndProc, "Additional routine should appear after main ENDPROC");
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void CreateModule_TwoAdditionalRoutinesSameDeclaration_OnlyDeclaredOnce()
        {
            Robot robot = CreateTestRobot();
            var decl = new CodeLine("VAR num sharedVar := 0;", CodeType.Declaration);
            var routineA = new Routine(new List<IAction> { decl }, RoutineType.PROC, "routineA");
            var routineB = new Routine(new List<IAction> { decl }, RoutineType.PROC, "routineB");
            var generator = new RAPIDGenerator(robot, "MainModule", "main", Scope.GLOBAL, null,
                new List<Routine> { routineA, routineB });

            List<string> module = generator.CreateModule(new List<IAction>());
            int count = module.Count(l => l.Contains("VAR num sharedVar"));

            Assert.Equal(1, count);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void CreateModule_MainAndAdditionalRoutineSameDeclaration_OnlyDeclaredOnce()
        {
            Robot robot = CreateTestRobot();
            var decl = new CodeLine("VAR num sharedVar := 0;", CodeType.Declaration);
            var routine = new Routine(new List<IAction> { decl }, RoutineType.PROC, "helper");
            var generator = new RAPIDGenerator(robot, "MainModule", "main", Scope.GLOBAL, null,
                new List<Routine> { routine });

            // Same declaration also in the main actions
            List<string> module = generator.CreateModule(new List<IAction> { decl });
            int count = module.Count(l => l.Contains("VAR num sharedVar"));

            Assert.Equal(1, count);
        }
        #endregion

        #region Axis Limit Enforcement
        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void CreateModule_AxisViolation_EnforceTrue_ReturnsEmptyModule()
        {
            Robot robot = CreateTestRobot();
            RAPIDGenerator generator = new RAPIDGenerator(robot);

            // Axis 1 limit for IRB120 is [-165, 165]; 999 is well out of range
            RobotJointPosition rjp = new RobotJointPosition(999, 0, 0, 0, 0, 0);
            JointTarget target = new JointTarget("jt_bad", rjp);
            Movement move = new Movement(MovementType.MoveAbsJ, target, new SpeedData(100), new ZoneData(10));

            List<string> module = generator.CreateModule(new List<IAction> { move });

            Assert.Empty(module);
            Assert.NotEmpty(generator.ErrorText);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void CreateModule_AxisViolation_EnforceFalse_ReturnsFullModule()
        {
            Robot robot = CreateTestRobot();
            RAPIDGenerator generator = new RAPIDGenerator(robot);
            generator.EnforceAxisLimits = false;

            RobotJointPosition rjp = new RobotJointPosition(999, 0, 0, 0, 0, 0);
            JointTarget target = new JointTarget("jt_bad", rjp);
            Movement move = new Movement(MovementType.MoveAbsJ, target, new SpeedData(100), new ZoneData(10));

            List<string> module = generator.CreateModule(new List<IAction> { move });
            string joined = string.Join(Environment.NewLine, module);

            Assert.Contains("MODULE MainModule", joined);
            Assert.Contains("ENDMODULE", joined);
            Assert.NotEmpty(generator.ErrorText);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void CreateModule_NoViolation_EnforceTrue_ReturnsFullModule()
        {
            Robot robot = CreateTestRobot();
            RAPIDGenerator generator = new RAPIDGenerator(robot);

            RobotJointPosition rjp = new RobotJointPosition(0, 0, 0, 0, 0, 0);
            JointTarget target = new JointTarget("jt_ok", rjp);
            Movement move = new Movement(MovementType.MoveAbsJ, target, new SpeedData(100), new ZoneData(10));

            List<string> module = generator.CreateModule(new List<IAction> { move });
            string joined = string.Join(Environment.NewLine, module);

            Assert.Contains("MODULE MainModule", joined);
            Assert.Contains("ENDMODULE", joined);
            Assert.Empty(generator.ErrorText);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void Duplicate_PreservesEnforceAxisLimits_True()
        {
            Robot robot = CreateTestRobot();
            RAPIDGenerator generator = new RAPIDGenerator(robot);
            generator.EnforceAxisLimits = true;

            RAPIDGenerator copy = generator.Duplicate();

            Assert.True(copy.EnforceAxisLimits);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void Duplicate_PreservesEnforceAxisLimits_False()
        {
            Robot robot = CreateTestRobot();
            RAPIDGenerator generator = new RAPIDGenerator(robot);
            generator.EnforceAxisLimits = false;

            RAPIDGenerator copy = generator.Duplicate();

            Assert.False(copy.EnforceAxisLimits);
        }
        #endregion
    }
}
