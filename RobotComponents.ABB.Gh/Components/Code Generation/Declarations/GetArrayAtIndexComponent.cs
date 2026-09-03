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
using Grasshopper.Kernel.Data;
// RobotComponents
using RobotComponents.ABB.Actions.Declarations;
using RobotComponents.ABB.Gh.Goos.Definitions;
using RobotComponents.ABB.Gh.Parameters.Definitions;
using RobotComponents.ABB.Gh.Utils;

namespace RobotComponents.ABB.Gh.Components.CodeGeneration
{
    /// <summary>
    /// RobotComponents Get Array At Index Component.
    /// Wraps RAPID array element access (arrayName{index}) into a RAPIDExpression.
    /// </summary>
    public class GetArrayAtIndexComponent : GH_RobotComponent
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public constructor without any arguments.
        /// </summary>
        public GetArrayAtIndexComponent() : base("Get Array At Index", "GAI", "Advanced RAPID Features",
            "Gets the element of an array RAPID Variable at a given index (arrayName{index}) as a RAPID expression. " +
            "The result can be wired into any RAPID Expression input.")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_RAPIDVariable(), "Variable", "V",
                "Array RAPID Variable to index into. Must be a variable declared as an array " +
                "(RAPID Variable component, \"Set Array Size\").",
                GH_ParamAccess.item);

            var indexParam = new Param_RAPIDExpression();
            indexParam.PersistentData.Append(new GH_RAPIDExpression(RAPIDExpression.FromLiteral(1)), new GH_Path(0));
            pManager.AddParameter(indexParam, "Index", "I",
                "Array index. Accepts an integer, a RAPID variable, or a RAPID expression. " +
                "Reminder: RAPID arrays are 1-based, so the first element is at index 1.",
                GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(new Param_RAPIDExpression(), "Expression", "E",
                "The array element access as a RAPID expression (arrayName{index}), usable as a value in other instructions.",
                GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            RAPIDVariable variable = null;
            GH_RAPIDExpression indexExpr = null;

            if (!DA.GetData(0, ref variable)) { return; }
            if (!DA.GetData(1, ref indexExpr)) { return; }

            if (variable == null || string.IsNullOrWhiteSpace(variable.Name))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Variable does not have a name.");
                return;
            }

            string index = HelperMethods.CheckRAPIDExpression(this, indexExpr, "Index", "1");

            RAPIDExpression expression = RAPIDExpression.FromString($"{variable.Name}{{{index}}}");
            DA.SetData(0, new GH_RAPIDExpression(expression));
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
            get { return Properties.Resources.GetArrayAtIndex_Icon; }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it.
        /// It is vital this Guid doesn't change otherwise old ghx files
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("862DBD06-A858-4BFF-95C0-A3AEDBCFA0A3"); }
        }
        #endregion
    }
}
