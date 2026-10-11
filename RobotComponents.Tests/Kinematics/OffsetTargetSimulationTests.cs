// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components (Modified)
// Original project: https://github.com/RobotComponents/RobotComponents
// Modified project: https://github.com/jpdrude/RobotComponents
//
// Copyright (c) 2026 EDEK Uni Kassel
//
// For license details, see the LICENSE file in the project root.

// System Libs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
    /// Tests for the Offset Target (RAPID Offs(...) on a Robot Target): its RAPID text, the external axis values
    /// the RAPID Generator writes for it, and simulating movements towards it.
    /// </summary>
    [Collection("RequiresRhino")]
    [Trait("Category", "RequiresRhino")]
    public class OffsetTargetSimulationTests
    {
        // Tool pointing down
        private static Plane TargetPlane(double x, double z) => new Plane(new Point3d(x, 0, z), new Vector3d(-1, 0, 0), new Vector3d(0, 1, 0));

        private static Movement CreateMovement(MovementType type, ITarget target)
        {
            return new Movement(type, target, new SpeedData(100), new ZoneData(0));
        }

        /// <summary>
        /// IRB120 on a linear track along the world X axis (logical axis A) that moves the robot.
        /// </summary>
        private static Robot CreateRobotOnTrack()
        {
            List<Mesh> meshes = Enumerable.Range(0, 7).Select(_ => new Mesh()).ToList();
            ExternalLinearAxis track = new ExternalLinearAxis(Plane.WorldXY, Vector3d.XAxis, new Interval(0, 3000), 0, true);

            return new Robot("TestRobot", meshes, TestHelpers.CreateIRB120Parameters(), TestHelpers.CreateIRB120Limits(),
                Plane.WorldXY, new RobotTool(), new List<IExternalAxis> { track });
        }

        private static string Generate(Robot robot, params IAction[] actions)
        {
            RAPIDGenerator generator = new RAPIDGenerator(robot);
            return string.Join(Environment.NewLine, generator.CreateModule(actions.ToList()));
        }

        private static string MoveLine(string module, string instruction)
        {
            return module.Split(new[] { Environment.NewLine }, StringSplitOptions.None).First(line => line.TrimStart().StartsWith(instruction + " "));
        }

        private static string DeclarationLine(string module, string name)
        {
            return module.Split(new[] { Environment.NewLine }, StringSplitOptions.None).First(line => line.Contains($"robtarget {name} :="));
        }

        // The external axis group of the first robtarget literal in a line: the bracket group followed by "]]"
        private static string ExternalAxisValues(string line)
        {
            return Regex.Match(line, @"\[([^\[\]]*)\]\]").Groups[1].Value;
        }

        #region RAPID text and data
        [Fact]
        public void ToRAPID_UnnamedSource_WritesTargetInline()
        {
            RobotTarget source = new RobotTarget(TargetPlane(374, 580));

            OffsetTarget offsetTarget = new OffsetTarget(source, "0", "0", "50");

            Assert.Equal($"Offs({source.ToRAPID()}, 0, 0, 50)", offsetTarget.ToRAPID());
            Assert.Equal(offsetTarget.ToRAPID(), offsetTarget.Name);
        }

        [Fact]
        public void ToRAPID_NamedSource_ReferencesName()
        {
            OffsetTarget offsetTarget = new OffsetTarget(new RobotTarget("p10", TargetPlane(374, 580)), "0", "0", "50");

            Assert.Equal("Offs(p10, 0, 0, 50)", offsetTarget.ToRAPID());
        }

        [Fact]
        public void ToRAPID_Nested_NestsOffsCalls()
        {
            OffsetTarget inner = new OffsetTarget(new RobotTarget("p10", TargetPlane(374, 580)), "0", "0", "50");

            OffsetTarget outer = new OffsetTarget(inner, "10", "0", "0");

            Assert.Equal("Offs(Offs(p10, 0, 0, 50), 10, 0, 0)", outer.ToRAPID());
        }

        [Fact]
        public void Constructor_JointTarget_Throws()
        {
            Assert.Throws<ArgumentException>(() => new OffsetTarget(new JointTarget(new RobotJointPosition()), "0", "0", "0"));
        }

        [Fact]
        public void GetOffsetRobotTarget_NumericNested_SumsOffsetsInWorkObjectFrame()
        {
            OffsetTarget inner = new OffsetTarget(new RobotTarget("p10", TargetPlane(374, 580)), "0", "0", "-50");
            OffsetTarget outer = new OffsetTarget(inner, "10", "0", "0");

            RobotTarget result = outer.GetOffsetRobotTarget();

            Assert.Equal(384, result.Plane.Origin.X, 6);
            Assert.Equal(530, result.Plane.Origin.Z, 6);
            Assert.Equal(-1, result.Plane.ZAxis.Z, 6); // orientation kept
        }

        [Fact]
        public void GetOffsetRobotTarget_VariableOffset_ReturnsNull()
        {
            OffsetTarget offsetTarget = new OffsetTarget(new RobotTarget("p10", TargetPlane(374, 580)), "0", "0", "myOffset");

            Assert.Null(offsetTarget.GetOffsetRobotTarget());
        }

        [Fact]
        public void OffsetTarget_Serialization_RoundTrips()
        {
            OffsetTarget offsetTarget = new OffsetTarget(new OffsetTarget(new RobotTarget("p10", TargetPlane(374, 580)), "0", "0", "-50"), "10", "0", "0");

            OffsetTarget result = (OffsetTarget)Serialization.ByteArrayToObject(Serialization.ObjectToByteArray(offsetTarget));

            Assert.Equal(offsetTarget.ToRAPID(), result.ToRAPID());
            Assert.Equal(530, result.GetOffsetRobotTarget().Plane.Origin.Z, 6);
        }

        [Fact]
        public void RAPIDExpression_WithTarget_DuplicateAndSerializationKeepTarget()
        {
            OffsetTarget offsetTarget = new OffsetTarget(new RobotTarget("p10", TargetPlane(374, 580)), "0", "0", "50");
            RAPIDExpression expression = RAPIDExpression.FromString(offsetTarget.ToRAPID()).WithTarget(offsetTarget);

            RAPIDExpression duplicate = expression.Duplicate();
            RAPIDExpression deserialized = (RAPIDExpression)Serialization.ByteArrayToObject(Serialization.ObjectToByteArray(expression));

            Assert.IsType<OffsetTarget>(duplicate.Target);
            Assert.Equal("Offs(p10, 0, 0, 50)", deserialized.Target.ToRAPID());
            Assert.Equal("Offs(p10, 0, 0, 50)", deserialized.Expression);
        }

        [Fact]
        public void RAPIDExpression_WithoutTarget_SerializesAsBefore()
        {
            RAPIDExpression expression = RAPIDExpression.FromString("myTarget");

            RAPIDExpression result = (RAPIDExpression)Serialization.ByteArrayToObject(Serialization.ObjectToByteArray(expression));

            Assert.Equal("myTarget", result.Expression);
            Assert.Null(result.Target);
        }
        #endregion

        #region RAPID generator
        [Fact]
        public void Generator_UnnamedSource_GetsExternalAxisValuesOfOffsetPosition()
        {
            Robot robot = CreateRobotOnTrack();
            OffsetTarget offsetTarget = new OffsetTarget(new RobotTarget(TargetPlane(1374, 580)), "200", "0", "0");

            string offsetModule = Generate(robot, CreateMovement(MovementType.MoveL, offsetTarget));
            string directModule = Generate(robot, CreateMovement(MovementType.MoveL, new RobotTarget(TargetPlane(1574, 580))));

            string offsetLine = MoveLine(offsetModule, "MoveL");
            string expected = ExternalAxisValues(MoveLine(directModule, "MoveL"));

            Assert.Contains("MoveL Offs([[1374, 0, 580]", offsetLine);
            Assert.Contains(", 200, 0, 0)", offsetLine);
            Assert.False(expected.StartsWith("9E9"), "The direct move should get a calculated track value.");
            Assert.Equal(expected, ExternalAxisValues(offsetLine));
        }

        [Fact]
        public void Generator_UnnamedSourceVariableOffset_GetsExternalAxisValuesOfSourcePosition()
        {
            Robot robot = CreateRobotOnTrack();
            OffsetTarget offsetTarget = new OffsetTarget(new RobotTarget(TargetPlane(1374, 580)), "myOffset", "0", "0");

            string offsetModule = Generate(robot, CreateMovement(MovementType.MoveL, offsetTarget));
            string directModule = Generate(robot, CreateMovement(MovementType.MoveL, new RobotTarget(TargetPlane(1374, 580))));

            Assert.Equal(ExternalAxisValues(MoveLine(directModule, "MoveL")), ExternalAxisValues(MoveLine(offsetModule, "MoveL")));
        }

        [Fact]
        public void Generator_NamedSource_DeclaresItLikeADirectMove()
        {
            Robot robot = CreateRobotOnTrack();
            RobotTarget source = new RobotTarget("p10", TargetPlane(1374, 580));

            string offsetModule = Generate(robot, CreateMovement(MovementType.MoveL, new OffsetTarget(source, "200", "0", "0")));
            string directModule = Generate(robot, CreateMovement(MovementType.MoveL, source));

            Assert.Contains("MoveL Offs(p10, 200, 0, 0)", MoveLine(offsetModule, "MoveL"));
            Assert.Equal(DeclarationLine(directModule, "p10"), DeclarationLine(offsetModule, "p10"));
        }

        [Fact]
        public void Generator_MoveAbsJNumericOffset_WritesInlineJointTarget()
        {
            Robot robot = TestHelpers.CreateIRB120Robot();
            OffsetTarget offsetTarget = new OffsetTarget(new RobotTarget("p10", TargetPlane(374, 630)), "0", "0", "-50");

            string module = Generate(robot, CreateMovement(MovementType.MoveAbsJ, offsetTarget));

            Assert.StartsWith("MoveAbsJ [[", MoveLine(module, "MoveAbsJ").TrimStart());
            Assert.DoesNotContain("Offs(", module);
        }
        #endregion

        #region simulation
        [Fact]
        public void InverseKinematics_UnnamedSource_MatchesDirectMoveToOffsetPosition()
        {
            Robot robot = CreateRobotOnTrack();

            InverseKinematics direct = new InverseKinematics(robot);
            direct.Calculate(CreateMovement(MovementType.MoveJ, new RobotTarget(TargetPlane(1574, 580))));

            InverseKinematics offset = new InverseKinematics(robot);
            offset.Calculate(CreateMovement(MovementType.MoveJ, new OffsetTarget(new RobotTarget(TargetPlane(1374, 580)), "200", "0", "0")));

            for (int i = 0; i < 6; i++)
            {
                Assert.Equal(direct.RobotJointPosition[i], offset.RobotJointPosition[i], 6);
            }
            Assert.Equal(direct.ExternalJointPosition[0], offset.ExternalJointPosition[0], 6);
            Assert.DoesNotContain(offset.ErrorText, e => e.Contains("unknown in Grasshopper"));
        }

        [Fact]
        public void InverseKinematics_NamedSource_KeepsExternalAxisValuesOfSource()
        {
            Robot robot = CreateRobotOnTrack();
            RobotTarget source = new RobotTarget("p10", TargetPlane(1374, 580));

            InverseKinematics direct = new InverseKinematics(robot);
            direct.Calculate(CreateMovement(MovementType.MoveJ, source));

            InverseKinematics offset = new InverseKinematics(robot);
            offset.Calculate(CreateMovement(MovementType.MoveJ, new OffsetTarget(source, "200", "0", "0")));

            Assert.Equal(direct.ExternalJointPosition[0], offset.ExternalJointPosition[0], 6);
        }

        [Fact]
        public void InverseKinematics_VariableOffset_ReportsError()
        {
            Robot robot = TestHelpers.CreateIRB120Robot();

            InverseKinematics inverseKinematics = new InverseKinematics(robot);
            inverseKinematics.Calculate(CreateMovement(MovementType.MoveJ, new OffsetTarget(new RobotTarget("p10", TargetPlane(374, 580)), "0", "0", "myOffset")));

            Assert.Contains(inverseKinematics.ErrorText, e => e.Contains("Offs(p10, 0, 0, myOffset)") && e.Contains("unknown in Grasshopper"));
        }

        [Fact]
        public void InverseKinematics_ReferenceTarget_ReportsError()
        {
            Robot robot = TestHelpers.CreateIRB120Robot();

            InverseKinematics inverseKinematics = new InverseKinematics(robot);
            inverseKinematics.Calculate(CreateMovement(MovementType.MoveJ, new ReferenceTarget("myTarget")));

            Assert.Contains(inverseKinematics.ErrorText, e => e.Contains("myTarget") && e.Contains("unknown in Grasshopper"));
        }

        [Fact]
        public void PathGenerator_OffsetTarget_IsSimulated()
        {
            Robot robot = TestHelpers.CreateIRB120Robot();
            List<IAction> actions = new List<IAction>
            {
                CreateMovement(MovementType.MoveJ, new RobotTarget("p10", TargetPlane(374, 630))),
                CreateMovement(MovementType.MoveL, new OffsetTarget(new RobotTarget("p10", TargetPlane(374, 630)), "0", "0", "-50")),
            };

            PathGenerator pathGenerator = new PathGenerator(robot);
            pathGenerator.Calculate(actions, 5);

            Assert.Equal(580, pathGenerator.Planes.Last().Origin.Z, 3);
            Assert.DoesNotContain(pathGenerator.ErrorText, e => e.Contains("unknown in Grasshopper"));
        }

        [Fact]
        public void PathGenerator_VariableOffset_ReportsError()
        {
            Robot robot = TestHelpers.CreateIRB120Robot();
            List<IAction> actions = new List<IAction>
            {
                CreateMovement(MovementType.MoveJ, new RobotTarget("p10", TargetPlane(374, 630))),
                CreateMovement(MovementType.MoveL, new OffsetTarget(new RobotTarget("p10", TargetPlane(374, 630)), "0", "0", "myOffset")),
            };

            PathGenerator pathGenerator = new PathGenerator(robot);
            pathGenerator.Calculate(actions, 5);

            Assert.Contains(pathGenerator.ErrorText, e => e.Contains("unknown in Grasshopper"));
        }

        [Fact]
        public void TimedPathGenerator_OffsetTarget_IsSimulated()
        {
            Robot robot = TestHelpers.CreateIRB120Robot();
            List<IAction> actions = new List<IAction>
            {
                CreateMovement(MovementType.MoveJ, new RobotTarget("p10", TargetPlane(374, 630))),
                CreateMovement(MovementType.MoveL, new OffsetTarget(new RobotTarget("p10", TargetPlane(374, 630)), "0", "0", "-50")),
            };

            TimedPathGenerator pathGenerator = new TimedPathGenerator(robot);
            pathGenerator.Calculate(actions, 0.1);

            Assert.Equal(580, pathGenerator.Planes.Last().Origin.Z, 3);
            Assert.DoesNotContain(pathGenerator.ErrorText, e => e.Contains("unknown in Grasshopper"));
        }
        #endregion
    }
}
