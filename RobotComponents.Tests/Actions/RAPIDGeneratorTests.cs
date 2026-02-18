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
using RobotComponents.ABB.Actions.Instructions;
using RobotComponents.ABB.Definitions;
using RobotComponents.ABB.Enumerations;
using RobotComponents.ABB.Presets;
using RobotComponents.ABB.Presets.Enumerations;

namespace RobotComponents.Tests.Actions
{
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
            List<string> module = generator.CreateModule(new List<IAction>());

            string joined = string.Join(Environment.NewLine, module);

            Assert.Contains("MODULE CustomModule", joined);
            Assert.Contains("PROC myProc()", joined);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void CreateModule_VersionComment_IncludesRCVersion()
        {
            Robot robot = CreateTestRobot();
            RAPIDGenerator generator = new RAPIDGenerator(robot);
            List<string> module = generator.CreateModule(new List<IAction>());

            string joined = string.Join(Environment.NewLine, module);

            Assert.Contains("! This RAPID code was generated with RobotComponents", joined);
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
