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
using RobotComponents.ABB.Actions.Dynamic;
using RobotComponents.ABB.Gh.Goos.Definitions;
using RobotComponents.ABB.Gh.Parameters.Actions;
using RobotComponents.ABB.Gh.Parameters.Definitions;
using RobotComponents.ABB.Gh.Utils;
using RobotComponents.ABB.Enumerations;

namespace RobotComponents.ABB.Gh.Components.CodeGeneration
{
    /// <summary>
    /// RobotComponents Return Component.
    /// Generates a RAPID RETURN instruction that returns a RAPID Variable or RAPID Expression from a function.
    /// </summary>
    public class ReturnComponent : GH_RobotComponent
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public constructor without any arguments.
        /// </summary>
        public ReturnComponent() : base("Return", "RET", "Advanced RAPID Features",
            "Creates a RAPID RETURN instruction to be used inside a function (FUNC). " +
            "Without a return value it emits a plain RETURN; to leave a procedure or trap routine.")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_RAPIDExpression(), "Return Value", "RV",
                "Value to return as a RAPID Variable or RAPID Expression (e.g. counter * 2). " +
                "Accepts a RAPIDExpression, a RAPIDVariable or a plain string.",
                GH_ParamAccess.item);

            pManager[0].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(new Param_Action(), "Return", "R",
                "RAPID RETURN instruction as a code line.",
                GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_RAPIDExpression returnExpr = null;

            if (!DA.GetData(0, ref returnExpr))
            {
                DA.SetData(0, new CodeLine("RETURN;", CodeType.Instruction));
                return;
            }

            string value = HelperMethods.CheckRAPIDExpression(this, returnExpr, "Return Value");

            DA.SetData(0, new CodeLine($"RETURN {value};", CodeType.Instruction));
        }

        #region properties
        /// <summary>
        /// Override the component exposure (makes the tab subcategory).
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
        }

        /// <summary>
        /// Gets whether this object is obsolete.
        /// </summary>
        public override bool Obsolete
        {
            get { return false; }
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get { return Properties.Resources.Return_Icon; }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it.
        /// It is vital this Guid doesn't change otherwise old ghx files
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("E809C207-3DAF-4F8D-80A8-4628E101EC6B"); }
        }
        #endregion
    }
}
