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
    public class MovementTests
    {
        /// <summary>
        /// Creates a test robot and generates a RAPID module from the given actions.
        /// </summary>
        private List<string> GenerateModule(IList<IAction> actions)
        {
            Robot robot = Factory.GetRobotPreset(RobotPreset.IRB120_3_058, Plane.WorldXY);
            RAPIDGenerator generator = new RAPIDGenerator(robot);
            return generator.CreateModule(actions);
        }

        #region MoveAbsJ
        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void MoveAbsJ_JointTarget_ProducesCorrectInstruction()
        {
            RobotJointPosition rjp = new RobotJointPosition(0, 0, 0, 0, 0, 0);
            JointTarget target = new JointTarget("jt1", rjp);
            SpeedData speed = new SpeedData(100);
            ZoneData zone = new ZoneData(10);
            Movement move = new Movement(MovementType.MoveAbsJ, target, speed, zone);

            List<string> module = GenerateModule(new List<IAction> { move });
            string joined = string.Join(Environment.NewLine, module);

            Assert.Contains("MoveAbsJ", joined);
            Assert.Contains("v100", joined);
            Assert.Contains("z10", joined);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void MoveAbsJ_NamedTarget_UsesVariableName()
        {
            RobotJointPosition rjp = new RobotJointPosition(10, 20, 30, 40, 50, 60);
            JointTarget target = new JointTarget("myTarget", rjp);
            Movement move = new Movement(MovementType.MoveAbsJ, target, new SpeedData(100), new ZoneData(10));

            List<string> module = GenerateModule(new List<IAction> { move });
            string joined = string.Join(Environment.NewLine, module);

            Assert.Contains("myTarget", joined);
        }
        #endregion

        #region MoveL
        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void MoveL_RobotTarget_ProducesCorrectInstruction()
        {
            RobotTarget target = new RobotTarget("rt1", new Plane(new Point3d(300, 0, 500), Vector3d.ZAxis));
            Movement move = new Movement(MovementType.MoveL, target, new SpeedData(100), new ZoneData(10));

            List<string> module = GenerateModule(new List<IAction> { move });
            string joined = string.Join(Environment.NewLine, module);

            Assert.Contains("MoveL", joined);
            Assert.Contains("v100", joined);
        }
        #endregion

        #region MoveJ
        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void MoveJ_RobotTarget_ProducesCorrectInstruction()
        {
            RobotTarget target = new RobotTarget("rt1", new Plane(new Point3d(300, 0, 500), Vector3d.ZAxis));
            Movement move = new Movement(MovementType.MoveJ, target, new SpeedData(100), new ZoneData(10));

            List<string> module = GenerateModule(new List<IAction> { move });
            string joined = string.Join(Environment.NewLine, module);

            Assert.Contains("MoveJ", joined);
        }
        #endregion

        #region Speed / Zone naming
        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void MoveAbsJ_PredefinedSpeed_UsesSpeedName()
        {
            RobotJointPosition rjp = new RobotJointPosition(0, 0, 0, 0, 0, 0);
            JointTarget target = new JointTarget("jt1", rjp);
            Movement move = new Movement(MovementType.MoveAbsJ, target, new SpeedData(100), new ZoneData(10));

            List<string> module = GenerateModule(new List<IAction> { move });
            // Predefined speed should use name "v100" not inline [100, 500, 5000, 1000]
            string instructionLine = module.FirstOrDefault(l => l.Contains("MoveAbsJ"));

            Assert.NotNull(instructionLine);
            Assert.Contains("v100", instructionLine);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void MoveAbsJ_CustomNamedSpeed_ProducesDeclaration()
        {
            RobotJointPosition rjp = new RobotJointPosition(0, 0, 0, 0, 0, 0);
            JointTarget target = new JointTarget("jt1", rjp);
            SpeedData customSpeed = new SpeedData("mySpeed", 250, 500, 5000, 1000);
            Movement move = new Movement(MovementType.MoveAbsJ, target, customSpeed, new ZoneData(10));

            List<string> module = GenerateModule(new List<IAction> { move });
            string joined = string.Join(Environment.NewLine, module);

            // Custom named speed: instruction uses name, declaration appears
            Assert.Contains("mySpeed", joined);
            Assert.Contains("VAR speeddata mySpeed", joined);
        }
        #endregion

        #region Invalid combinations
        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void JointTarget_WithMoveL_ThrowsInvalidOperationException()
        {
            RobotJointPosition rjp = new RobotJointPosition(0, 0, 0, 0, 0, 0);
            JointTarget target = new JointTarget("jt1", rjp);

            Assert.Throws<InvalidOperationException>(() =>
                new Movement(MovementType.MoveL, target, new SpeedData(100), new ZoneData(10)));
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void JointTarget_WithMoveJ_ThrowsInvalidOperationException()
        {
            RobotJointPosition rjp = new RobotJointPosition(0, 0, 0, 0, 0, 0);
            JointTarget target = new JointTarget("jt1", rjp);

            Assert.Throws<InvalidOperationException>(() =>
                new Movement(MovementType.MoveJ, target, new SpeedData(100), new ZoneData(10)));
        }
        #endregion

        #region MoveC
        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void MoveC_RobotTarget_ProducesCorrectInstruction()
        {
            RobotTarget cirPoint = new RobotTarget("via1", new Plane(new Point3d(200, 100, 400), Vector3d.ZAxis));
            RobotTarget target = new RobotTarget("rt1", new Plane(new Point3d(300, 0, 500), Vector3d.ZAxis));
            Movement move = new Movement(MovementType.MoveC, target, new SpeedData(100), new ZoneData(10));
            move.CircularPoint = cirPoint;

            List<string> module = GenerateModule(new List<IAction> { move });
            string joined = string.Join(Environment.NewLine, module);

            Assert.Contains("MoveC", joined);
            Assert.Contains("via1", joined);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void MoveC_UnsetCircularPoint_ThrowsException()
        {
            RobotTarget target = new RobotTarget("rt1", new Plane(new Point3d(300, 0, 500), Vector3d.ZAxis));
            Movement move = new Movement(MovementType.MoveC, target, new SpeedData(100), new ZoneData(10));

            Assert.Throws<Exception>(() => GenerateModule(new List<IAction> { move }));
        }
        #endregion

        #region MoveLDO / MoveJDO
        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void MoveL_WithDigitalOutput_ProducesMoveLDO()
        {
            RobotTarget target = new RobotTarget("rt1", new Plane(new Point3d(300, 0, 500), Vector3d.ZAxis));
            SetDigitalOutput sdo = new SetDigitalOutput("DO_1", true);
            Movement move = new Movement(MovementType.MoveL, target, new SpeedData(100), new ZoneData(10), sdo);

            List<string> module = GenerateModule(new List<IAction> { move });
            string joined = string.Join(Environment.NewLine, module);

            Assert.Contains("MoveLDO", joined);
            Assert.Contains("DO_1", joined);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void MoveJ_WithDigitalOutput_ProducesMoveJDO()
        {
            RobotTarget target = new RobotTarget("rt1", new Plane(new Point3d(300, 0, 500), Vector3d.ZAxis));
            SetDigitalOutput sdo = new SetDigitalOutput("DO_1", false);
            Movement move = new Movement(MovementType.MoveJ, target, new SpeedData(100), new ZoneData(10), sdo);

            List<string> module = GenerateModule(new List<IAction> { move });
            string joined = string.Join(Environment.NewLine, module);

            Assert.Contains("MoveJDO", joined);
            Assert.Contains("DO_1", joined);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void MoveAbsJ_WithDigitalOutput_ProducesSeparateSetDO()
        {
            RobotJointPosition rjp = new RobotJointPosition(0, 0, 0, 0, 0, 0);
            JointTarget target = new JointTarget("jt1", rjp);
            SetDigitalOutput sdo = new SetDigitalOutput("DO_1", true);
            Movement move = new Movement(MovementType.MoveAbsJ, target, new SpeedData(100), new ZoneData(10), sdo);

            List<string> module = GenerateModule(new List<IAction> { move });
            string joined = string.Join(Environment.NewLine, module);

            Assert.Contains("MoveAbsJ", joined);
            Assert.Contains("SetDO", joined);
            Assert.Contains("DO_1", joined);
        }
        #endregion

        #region IsValid
        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void IsValid_ValidMovement_ReturnsTrue()
        {
            RobotJointPosition rjp = new RobotJointPosition(0, 0, 0, 0, 0, 0);
            JointTarget target = new JointTarget("jt1", rjp);
            Movement move = new Movement(MovementType.MoveAbsJ, target, new SpeedData(100), new ZoneData(10));

            Assert.True(move.IsValid);
        }

        [Fact]
        [Trait("Category", "RequiresRhino")]
        public void IsValid_NullTarget_ReturnsFalse()
        {
            Movement move = new Movement();

            Assert.False(move.IsValid);
        }
        #endregion
    }
}
