// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components (Modified)
// Original project: https://github.com/RobotComponents/RobotComponents
// Modified project: https://github.com/jpdrude/RobotComponents
//
// Copyright (c) 2026 EDEK Uni Kassel
//
// Author:
//   - Jan Philipp Drude (2026)
//
// For license details, see the LICENSE file in the project root.

// System Libs
using System;
// Grasshopper Libs
using Grasshopper.Kernel;
// RobotComponents Libs
using RobotComponents.ABB.Gh.Goos.Definitions;

namespace RobotComponents.ABB.Gh.Parameters.Definitions
{
    /// <summary>
    /// RAPID Expression parameter. Accepts literal values, RAPID variable references,
    /// function calls, and arithmetic expressions.
    /// </summary>
    public class Param_RAPIDExpression : GH_RobotParam<GH_RAPIDExpression>
    {
        /// <summary>
        /// Initializes a new instance of the Param_RAPIDExpression class.
        /// </summary>
        public Param_RAPIDExpression() : base("RAPID Expression", "RE", "Parameters",
            "Contains a RAPID expression: a literal value, variable reference, function call, or arithmetic expression.")
        {
        }

        /// <inheritdoc/>
        public override string ToString() => "RAPID Expression";

        /// <inheritdoc/>
        public override string Name { get => base.Name; set => base.Name = value; }

        /// <inheritdoc/>
        protected override System.Drawing.Bitmap Icon
        {
            get { return Properties.Resources.CodeLine_Parameter_Icon; }
        }

        /// <inheritdoc/>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.quarternary; }
        }

        /// <inheritdoc/>
        public override Guid ComponentGuid
        {
            get { return new Guid("F3A9E2D1-7B4C-4E8F-A5D2-9C3B1E6F8A7D"); }
        }
    }
}
