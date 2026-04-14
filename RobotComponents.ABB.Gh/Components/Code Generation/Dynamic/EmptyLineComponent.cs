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
// RobotComponents
using RobotComponents.ABB.Actions.Dynamic;
using RobotComponents.ABB.Enumerations;
using RobotComponents.ABB.Gh.Parameters.Actions;

namespace RobotComponents.ABB.Gh.Components.CodeGeneration
{
    /// <summary>
    /// RobotComponents Empty Line Component.
    /// Outputs a blank RAPID code line (four spaces) to add visual spacing in the generated module.
    /// </summary>
    public class EmptyLineComponent : GH_RobotComponent
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public constructor without any arguments.
        /// </summary>
        public EmptyLineComponent() : base("Empty Line", "EL", "Code Generation",
            "Outputs a blank line to add visual spacing inside a RAPID routine.")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            // No inputs — this component always produces the same blank line.
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(new Param_Action(), "Empty Line", "EL",
                "Blank RAPID code line for visual spacing.",
                GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DA.SetData(0, new CodeLine("    ", CodeType.Instruction));
        }

        #region properties
        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override bool Obsolete => false;

        protected override System.Drawing.Bitmap Icon
        {
            get { return Properties.Resources.Comment_Icon; }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("F8C2A4E1-6B37-4D5A-B9F3-2E1C8D5A7B40"); }
        }
        #endregion
    }
}
