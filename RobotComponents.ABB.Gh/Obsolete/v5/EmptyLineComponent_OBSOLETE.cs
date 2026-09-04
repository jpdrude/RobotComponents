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

#pragma warning disable CS1591 // Missing XML comment — obsolete shim, kept for .ghx backwards compatibility.

// System Libs
using System;
// Grasshopper Libs
using Grasshopper.Kernel;
// RobotComponents
using RobotComponents.ABB.Actions.Dynamic;
using RobotComponents.ABB.Enumerations;
using RobotComponents.ABB.Gh.Components;
using RobotComponents.ABB.Gh.Parameters.Actions;

namespace RobotComponents.ABB.Gh.Obsolete
{
    /// <summary>
    /// RobotComponents Empty Line Component.
    /// Outputs a blank RAPID code line (four spaces) to add visual spacing in the generated module.
    /// </summary>
    /// <remarks>
    /// Hidden from the menu since the Type input was added. Retained so older .gh files that
    /// placed this component before that change continue to load and resolve their saved param
    /// GUIDs unchanged; the live component now has a new GUID for its new input shape.
    /// </remarks>
    [Obsolete("This component is OBSOLETE and will be removed in the future. Use Empty Line instead.", false)]
    public class EmptyLineComponent_OBSOLETE : GH_RobotComponent
    {
        public EmptyLineComponent_OBSOLETE() : base("Empty Line", "EL", "Advanced RAPID Features",
            "Outputs a blank line to add visual spacing inside a RAPID routine.")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            // No inputs — this component always produces the same blank line.
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(new Param_Action(), "Empty Line", "EL",
                "Blank RAPID code line for visual spacing.",
                GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DA.SetData(0, new CodeLine("    ", CodeType.Instruction));
        }

        #region properties
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.hidden; }
        }

        public override bool Obsolete
        {
            get { return true; }
        }

        protected override System.Drawing.Bitmap Icon
        {
            get { return Properties.Resources.EmptyLine_Icon; }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it.
        /// It is vital this Guid doesn't change otherwise old ghx files
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("F8C2A4E1-6B37-4D5A-B9F3-2E1C8D5A7B40"); }
        }
        #endregion
    }
}
