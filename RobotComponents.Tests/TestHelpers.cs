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
// Robot Components Libs
using RobotComponents.Generic.Kinematics;
using RobotComponents.ABB.Definitions;

namespace RobotComponents.Tests
{
    /// <summary>
    /// Shared test helpers for creating test fixtures.
    /// </summary>
    internal static class TestHelpers
    {
        /// <summary>
        /// Creates a test robot with IRB120-3/0.58 kinematic parameters and empty meshes.
        /// Requires Rhino native library (new Mesh(), new RobotTool()).
        /// </summary>
        internal static Robot CreateIRB120Robot(Plane basePlane = default, RobotTool tool = null)
        {
            if (basePlane == default) basePlane = Plane.WorldXY;
            if (tool == null) tool = new RobotTool();

            List<Mesh> meshes = Enumerable.Range(0, 7).Select(_ => new Mesh()).ToList();
            RobotKinematicParameters parameters = CreateIRB120Parameters();
            Interval[] limits = CreateIRB120Limits();

            return new Robot("TestRobot", meshes, parameters, limits, basePlane, tool);
        }

        /// <summary>
        /// Creates IRB120-3/0.58 kinematic parameters.
        /// </summary>
        internal static RobotKinematicParameters CreateIRB120Parameters()
        {
            return new RobotKinematicParameters(0, -70, 0, 0, 290, 270, 302, 72);
        }

        /// <summary>
        /// Creates IRB120 axis limits (degrees).
        /// </summary>
        internal static Interval[] CreateIRB120Limits()
        {
            return new Interval[]
            {
                new Interval(-165, 165),
                new Interval(-110, 110),
                new Interval(-110, 70),
                new Interval(-160, 160),
                new Interval(-120, 120),
                new Interval(-400, 400)
            };
        }

        /// <summary>
        /// Creates an OPW solver configured with IRB120-3/0.58 kinematic parameters.
        /// IMPORTANT: Property assignment order matters — A2 and C3 each trigger
        /// UpdateRobotParameters(), so A2 must be set before C3 to ensure correct
        /// derived values (psi3, k, k2).
        /// </summary>
        internal static OPWKinematics CreateIRB120OPW()
        {
            OPWKinematics opw = new OPWKinematics();
            opw.A1 = 0;
            opw.B = 0;
            opw.C1 = 290;
            opw.C2 = 270;
            opw.C4 = 72;
            opw.A2 = -70;   // triggers UpdateRobotParameters
            opw.C3 = 302;   // triggers UpdateRobotParameters with correct A2
            opw.Offsets = new double[] { 0, 0, -Math.PI / 2, 0, 0, 0 };
            opw.Signs = new int[] { 1, 1, 1, 1, 1, 1 };
            return opw;
        }

        /// <summary>
        /// Asserts that at least one of the 8 OPW inverse solutions matches the expected
        /// joint angles within the given tolerance.
        /// </summary>
        internal static void AssertAnySolutionMatches(double[][] solutions, double[] expected, double tolerance)
        {
            for (int i = 0; i < solutions.Length; i++)
            {
                bool allMatch = true;

                for (int j = 0; j < 6; j++)
                {
                    if (Math.Abs(solutions[i][j] - expected[j]) > tolerance)
                    {
                        allMatch = false;
                        break;
                    }
                }

                if (allMatch) return;
            }

            throw new Xunit.Sdk.XunitException(
                "No OPW inverse solution matched the expected joint angles within tolerance");
        }
    }
}
