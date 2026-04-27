// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components (Modified)
// Original project: https://github.com/RobotComponents/RobotComponents
// Modified project: https://github.com/jpdrude/RobotComponents
//
// Copyright (c) 2021-2026 Arjen Deetman
// Copyright (c) 2026 EDEK Uni Kassel
//
// For license details, see the LICENSE file in the project root.

#pragma warning disable CS1591 // Missing XML comment — obsolete shim, kept for .ghx backwards compatibility.

// System Libs
using System;
// Grasshopper Libs
using Grasshopper.Kernel;
// RobotComponents Libs
using RobotComponents.ABB.Gh.Goos.Actions.Instructions;

namespace RobotComponents.ABB.Gh.Parameters.Actions.Instructions
{
    /// <summary>
    /// Linear Configuration Control parameter.
    /// </summary>
    /// <remarks>
    /// Hidden from the menu since v1.1. The producing component now publishes its
    /// output as Param_Action. Retained so older .ghx files continue to load and
    /// resolve their saved param GUIDs unchanged.
    /// </remarks>
    public class Param_LinearConfigurationControl : GH_RobotParam<GH_LinearConfigurationControl>
    {
        public Param_LinearConfigurationControl() : base("Linear Configuration Control", "LCCP", "Parameters",
                "Contains the data of a Linear Configuration Control instruction.")
        {
        }

        public override string ToString()
        {
            return "Linear Configuration Control";
        }

        public override string Name { get => base.Name; set => base.Name = value; }

        protected override System.Drawing.Bitmap Icon
        {
            get { return Properties.Resources.ConfL_Parameter_Icon; }
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.hidden; }
        }

        public override bool Obsolete
        {
            get { return true; }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("80EDDE7B-30FB-4C70-B062-6FD6810830F1"); }
        }
    }
}
