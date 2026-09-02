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
    /// RobotComponents Offs Expression Component.
    /// Wraps the RAPID built-in Offs(Target, X, Y, Z) function into a RAPIDExpression,
    /// so it can be wired into a Move Target, Assign Variable Value, or any other value input.
    /// </summary>
    public class OffsComponent : GH_RobotComponent
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public constructor without any arguments.
        /// </summary>
        public OffsComponent() : base("Offs", "Offs", "Advanced RAPID Features",
            "Wraps the RAPID built-in Offs(Target, X, Y, Z) function into a RAPID expression, offsetting " +
            "a Robot Target along its own X, Y and Z axes. The result can be wired into any RAPID Expression input.")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Target", "T",
                "Robot Target to offset from. Accepts a Robot Target, a RAPID Variable (e.g. an INOUT robtarget " +
                "routine argument), or a RAPID Expression.",
                GH_ParamAccess.item);

            var xParam = new Param_RAPIDExpression();
            xParam.PersistentData.Append(new GH_RAPIDExpression(RAPIDExpression.FromLiteral(0.0)), new GH_Path(0));
            pManager.AddParameter(xParam, "X", "X",
                "Offset along the target's X axis, in mm. Accepts a number, RAPID variable, or RAPID expression.",
                GH_ParamAccess.item);

            var yParam = new Param_RAPIDExpression();
            yParam.PersistentData.Append(new GH_RAPIDExpression(RAPIDExpression.FromLiteral(0.0)), new GH_Path(0));
            pManager.AddParameter(yParam, "Y", "Y",
                "Offset along the target's Y axis, in mm. Accepts a number, RAPID variable, or RAPID expression.",
                GH_ParamAccess.item);

            var zParam = new Param_RAPIDExpression();
            zParam.PersistentData.Append(new GH_RAPIDExpression(RAPIDExpression.FromLiteral(0.0)), new GH_Path(0));
            pManager.AddParameter(zParam, "Z", "Z",
                "Offset along the target's Z axis, in mm. Accepts a number, RAPID variable, or RAPID expression.",
                GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(new Param_RAPIDExpression(), "Expression", "E",
                "The Offs(...) function call as a RAPID expression, usable as a value in other instructions.",
                GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            object rawTarget = null;
            GH_RAPIDExpression xExpr = null;
            GH_RAPIDExpression yExpr = null;
            GH_RAPIDExpression zExpr = null;

            if (!DA.GetData(0, ref rawTarget)) { return; }
            if (!DA.GetData(1, ref xExpr)) { return; }
            if (!DA.GetData(2, ref yExpr)) { return; }
            if (!DA.GetData(3, ref zExpr)) { return; }

            // Resolve the target to its declared name (referencing the existing declaration/variable) or,
            // if it was not given a name (or is a plain expression), to its inline RAPID value.
            string targetText = HelperMethods.ResolveRAPIDValueExpression(rawTarget);
            if (string.IsNullOrWhiteSpace(targetText))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Target does not resolve to a valid RAPID value.");
                return;
            }

            string x = HelperMethods.CheckRAPIDExpression(this, xExpr, "X", "0");
            string y = HelperMethods.CheckRAPIDExpression(this, yExpr, "Y", "0");
            string z = HelperMethods.CheckRAPIDExpression(this, zExpr, "Z", "0");

            RAPIDExpression expression = RAPIDExpression.FromFunctionCall("Offs", new[]
            {
                RAPIDExpression.FromString(targetText),
                RAPIDExpression.FromString(x),
                RAPIDExpression.FromString(y),
                RAPIDExpression.FromString(z)
            });

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
            get { return Properties.Resources.Offs_Icon; }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it.
        /// It is vital this Guid doesn't change otherwise old ghx files
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("C2A8C680-0555-4530-91B4-90CD3C71F9AA"); }
        }
        #endregion
    }
}
