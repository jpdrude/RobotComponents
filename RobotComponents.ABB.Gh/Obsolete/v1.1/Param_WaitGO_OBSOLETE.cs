// SPDX-License-Identifier: GPL-3.0-or-later
// This file is part of Robot Components (Modified)
// Original project: https://github.com/RobotComponents/RobotComponents
// Modified project: https://github.com/jpdrude/RobotComponents
//
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
    /// Wait for Group Output parameter.
    /// </summary>
    /// <remarks>
    /// Hidden from the menu since v1.1. The producing component now publishes its
    /// output as Param_Action. Retained so older .ghx files continue to load and
    /// resolve their saved param GUIDs unchanged.
    /// </remarks>
    public class Param_WaitGO : GH_RobotParam<GH_WaitGO>
    {
        public Param_WaitGO() : base("Wait for Group Output", "WGO", "Parameters",
                "Contains the data of a Wait for Group Output instruction.")
        {
        }

        public override string ToString()
        {
            return "Wait for Group Output";
        }

        public override string Name { get => base.Name; set => base.Name = value; }

        protected override System.Drawing.Bitmap Icon
        {
            get { return Properties.Resources.WaitGO_Parameter_Icon; }
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
            get { return new Guid("607182EE-BE1B-4658-A86E-814F6DC07004"); }
        }
    }
}
