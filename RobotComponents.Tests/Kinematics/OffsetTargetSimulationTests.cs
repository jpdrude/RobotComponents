// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components (Modified)
// Original project: https://github.com/RobotComponents/RobotComponents
// Modified project: https://github.com/jpdrude/RobotComponents
//
// Copyright (c) 2026 EDEK Uni Kassel
//
// For license details, see the LICENSE file in the project root.

// System Libs
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
using RobotComponents.ABB.Kinematics;
using RobotComponents.Utils;

namespace RobotComponents.Tests.Kinematics
{
    /// <summary>
    /// Tests for simulating movements towards a Reference Target with a design-time value
    /// (the output of the Offset Target component, an Offs(...) RAPID expression).
    /// </summary>
    [Collection("RequiresRhino")]
    [Trait("Category", "RequiresRhino")]
    public class OffsetTargetSimulationTests
    {
        private const double Tolerance = 1e-6;
        private const string OffsExpression = "Offs(p10, 0, 0, -50)";

        // Tool pointing down, in front of an IRB120
        private static Plane TargetPlane(double z) => new Plane(new Point3d(374, 0, z), new Vector3d(-1, 0, 0), new Vector3d(0, 1, 0));

        private static Movement CreateMovement(MovementType type, ITarget target)
        {
            return new Movement(type, target, new SpeedData(100), new ZoneData(0));
        }

        #region resolved target data
        [Fact]
        public void RAPIDExpression_WithResolvedTarget_DuplicateKeepsTarget()
        {
            RAPIDExpression expression = RAPIDExpression.FromString(OffsExpression).WithResolvedTarget(new RobotTarget("p10", TargetPlane(580)));

            RAPIDExpression duplicate = expression.Duplicate();

            Assert.Equal(OffsExpression, duplicate.Expression);
            Assert.NotNull(duplicate.ResolvedTarget);
            Assert.Equal(580, duplicate.ResolvedTarget.Plane.Origin.Z, 6);
        }

        [Fact]
        public void RAPIDExpression_Serialization_RoundTripsResolvedTarget()
        {
            RAPIDExpression expression = RAPIDExpression.FromString(OffsExpression).WithResolvedTarget(new RobotTarget("p10", TargetPlane(580)));

            RAPIDExpression result = (RAPIDExpression)Serialization.ByteArrayToObject(Serialization.ObjectToByteArray(expression));

            Assert.Equal(OffsExpression, result.Expression);
            Assert.NotNull(result.ResolvedTarget);
            Assert.Equal(580, result.ResolvedTarget.Plane.Origin.Z, 6);
        }

        [Fact]
        public void RAPIDExpression_Serialization_WithoutResolvedTarget_StaysNull()
        {
            RAPIDExpression expression = RAPIDExpression.FromString("myTarget");

            RAPIDExpression result = (RAPIDExpression)Serialization.ByteArrayToObject(Serialization.ObjectToByteArray(expression));

            Assert.Equal("myTarget", result.Expression);
            Assert.Null(result.ResolvedTarget);
        }

        [Fact]
        public void ReferenceTarget_DuplicateAndSerialization_KeepResolvedTarget()
        {
            ReferenceTarget target = new ReferenceTarget(OffsExpression, new RobotTarget("p10", TargetPlane(580)));

            ReferenceTarget duplicate = target.Duplicate();
            ReferenceTarget deserialized = (ReferenceTarget)Serialization.ByteArrayToObject(Serialization.ObjectToByteArray(target));

            Assert.Equal(580, duplicate.ResolvedTarget.Plane.Origin.Z, 6);
            Assert.Equal(580, deserialized.ResolvedTarget.Plane.Origin.Z, 6);
            Assert.Equal(OffsExpression, deserialized.ToRAPID());
        }
        #endregion

        #region simulation movement
        [Fact]
        public void GetSimulationMovement_ResolvedReferenceTarget_ReturnsRobotTargetMovement()
        {
            Movement movement = CreateMovement(MovementType.MoveL, new ReferenceTarget(OffsExpression, new RobotTarget("p10", TargetPlane(580))));

            Movement simulation = movement.GetSimulationMovement();

            RobotTarget target = Assert.IsType<RobotTarget>(simulation.Target);
            Assert.Equal(OffsExpression, target.Name);
            Assert.Equal(580, target.Plane.Origin.Z, 6);
            Assert.Equal(MovementType.MoveL, simulation.MovementType);

            // The original movement still emits the expression
            Assert.IsType<ReferenceTarget>(movement.Target);
            Assert.Equal(OffsExpression, movement.Target.ToRAPID());
        }

        [Fact]
        public void GetSimulationMovement_UnresolvedReferenceTarget_ReturnsSameMovement()
        {
            Movement movement = CreateMovement(MovementType.MoveL, new ReferenceTarget("myTarget"));

            Assert.Same(movement, movement.GetSimulationMovement());
        }

        [Fact]
        public void GetSimulationMovement_RobotTarget_ReturnsSameMovement()
        {
            Movement movement = CreateMovement(MovementType.MoveL, new RobotTarget("p10", TargetPlane(580)));

            Assert.Same(movement, movement.GetSimulationMovement());
        }
        #endregion

        #region inverse kinematics
        [Fact]
        public void InverseKinematics_ResolvedReferenceTarget_MatchesRobotTarget()
        {
            Robot robot = TestHelpers.CreateIRB120Robot();

            InverseKinematics direct = new InverseKinematics(robot);
            direct.Calculate(CreateMovement(MovementType.MoveJ, new RobotTarget("p10", TargetPlane(580))));

            InverseKinematics offset = new InverseKinematics(robot);
            offset.Calculate(CreateMovement(MovementType.MoveJ, new ReferenceTarget(OffsExpression, new RobotTarget("p10", TargetPlane(580)))));

            for (int i = 0; i < 6; i++)
            {
                Assert.Equal(direct.RobotJointPosition[i], offset.RobotJointPosition[i], 6);
            }
            Assert.DoesNotContain(offset.ErrorText, e => e.Contains("unknown in Grasshopper"));
        }

        [Fact]
        public void InverseKinematics_UnresolvedReferenceTarget_ReportsError()
        {
            Robot robot = TestHelpers.CreateIRB120Robot();

            InverseKinematics inverseKinematics = new InverseKinematics(robot);
            inverseKinematics.Calculate(CreateMovement(MovementType.MoveJ, new ReferenceTarget("myTarget")));

            Assert.Contains(inverseKinematics.ErrorText, e => e.Contains("myTarget") && e.Contains("unknown in Grasshopper"));
        }
        #endregion

        #region path generators
        [Fact]
        public void PathGenerator_ResolvedReferenceTarget_IsSimulated()
        {
            Robot robot = TestHelpers.CreateIRB120Robot();
            List<IAction> actions = new List<IAction>
            {
                CreateMovement(MovementType.MoveJ, new RobotTarget("p10", TargetPlane(630))),
                CreateMovement(MovementType.MoveL, new ReferenceTarget(OffsExpression, new RobotTarget("p10", TargetPlane(580)))),
            };

            PathGenerator pathGenerator = new PathGenerator(robot);
            pathGenerator.Calculate(actions, 5);

            Assert.Equal(580, pathGenerator.Planes.Last().Origin.Z, 3);
            Assert.DoesNotContain(pathGenerator.ErrorText, e => e.Contains("unknown in Grasshopper"));
        }

        [Fact]
        public void PathGenerator_UnresolvedReferenceTarget_ReportsError()
        {
            Robot robot = TestHelpers.CreateIRB120Robot();
            List<IAction> actions = new List<IAction>
            {
                CreateMovement(MovementType.MoveJ, new RobotTarget("p10", TargetPlane(630))),
                CreateMovement(MovementType.MoveL, new ReferenceTarget("myTarget")),
            };

            PathGenerator pathGenerator = new PathGenerator(robot);
            pathGenerator.Calculate(actions, 5);

            Assert.Contains(pathGenerator.ErrorText, e => e.Contains("myTarget") && e.Contains("unknown in Grasshopper"));
        }

        [Fact]
        public void TimedPathGenerator_ResolvedReferenceTarget_IsSimulated()
        {
            Robot robot = TestHelpers.CreateIRB120Robot();
            List<IAction> actions = new List<IAction>
            {
                CreateMovement(MovementType.MoveJ, new RobotTarget("p10", TargetPlane(630))),
                CreateMovement(MovementType.MoveL, new ReferenceTarget(OffsExpression, new RobotTarget("p10", TargetPlane(580)))),
            };

            TimedPathGenerator pathGenerator = new TimedPathGenerator(robot);
            pathGenerator.Calculate(actions, 0.1);

            Assert.Equal(580, pathGenerator.Planes.Last().Origin.Z, 3);
            Assert.DoesNotContain(pathGenerator.ErrorText, e => e.Contains("unknown in Grasshopper"));
        }
        #endregion
    }
}
