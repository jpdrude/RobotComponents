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
using Grasshopper.Kernel.Types;
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
    /// When the target is a Robot Target, the expression also carries an <see cref="OffsetTarget"/>. A Move
    /// using the expression then gets that Offset Target, so the RAPID Generator can set the external axis
    /// values of the source target and the kinematics can simulate the movement.
    /// </summary>
    public class OffsComponent : GH_RobotComponent
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public constructor without any arguments.
        /// </summary>
        public OffsComponent() : base("Offset Target", "Offs", "Advanced RAPID Features",
            "Wraps the RAPID built-in Offs(Target, X, Y, Z) function into a RAPID expression, offsetting " +
            "a Robot Target in the X, Y and Z directions of its work object coordinate system. The result can be " +
            "wired into any RAPID Expression input. Used as the target of a Move, the external axis values of a " +
            "Robot Target are set by the RAPID Generator, and the Move can be simulated if X, Y and Z are numbers.")
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
                "Offset in the X direction of the work object coordinate system, in mm. Accepts a number, RAPID variable, or RAPID expression.",
                GH_ParamAccess.item);

            var yParam = new Param_RAPIDExpression();
            yParam.PersistentData.Append(new GH_RAPIDExpression(RAPIDExpression.FromLiteral(0.0)), new GH_Path(0));
            pManager.AddParameter(yParam, "Y", "Y",
                "Offset in the Y direction of the work object coordinate system, in mm. Accepts a number, RAPID variable, or RAPID expression.",
                GH_ParamAccess.item);

            var zParam = new Param_RAPIDExpression();
            zParam.PersistentData.Append(new GH_RAPIDExpression(RAPIDExpression.FromLiteral(0.0)), new GH_Path(0));
            pManager.AddParameter(zParam, "Z", "Z",
                "Offset in the Z direction of the work object coordinate system, in mm. Accepts a number, RAPID variable, or RAPID expression.",
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

            // Keep a Robot Target (or a nested Offset Target) as an object, so the RAPID Generator and the
            // kinematics can work with the actual target. Its RAPID text is the same as the plain expression's.
            OffsetTarget offsetTarget = CreateOffsetTarget(rawTarget, x, y, z);

            if (offsetTarget != null)
            {
                targetText = offsetTarget.Target is RobotTarget robotTarget && robotTarget.Name != "" ? robotTarget.Name : offsetTarget.Target.ToRAPID();

                if (offsetTarget.GetOffsetRobotTarget() == null)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "A Move to this offset target can't be simulated, " +
                        "since X, Y and Z are not all numbers.");
                }
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "A Move to this offset target can't be simulated, " +
                    "since the target is not a Robot Target.");
            }

            RAPIDExpression expression = RAPIDExpression.FromFunctionCall("Offs", new[]
            {
                RAPIDExpression.FromString(targetText),
                RAPIDExpression.FromString(x),
                RAPIDExpression.FromString(y),
                RAPIDExpression.FromString(z)
            });

            if (offsetTarget != null)
            {
                expression = expression.WithTarget(offsetTarget);
            }

            DA.SetData(0, new GH_RAPIDExpression(expression));
        }

        /// <summary>
        /// Creates the Offset Target for the Target input, if it is a Robot Target or an Offset Target.
        /// </summary>
        /// <remarks>
        /// Anything else (a RAPID Variable, a plain RAPID Expression, ...) is only known by name or text, and is
        /// emitted as such, without an Offset Target.
        /// </remarks>
        /// <param name="rawTarget"> The raw Target input. </param>
        /// <param name="x"> The X offset expression. </param>
        /// <param name="y"> The Y offset expression. </param>
        /// <param name="z"> The Z offset expression. </param>
        /// <returns> The Offset Target, or null. </returns>
        private static OffsetTarget CreateOffsetTarget(object rawTarget, string x, string y, string z)
        {
            object value = rawTarget is IGH_Goo goo ? goo.ScriptVariable() : rawTarget;

            if (value is RAPIDExpression rapidExpression)
            {
                value = rapidExpression.Target;
            }

            if (value is RobotTarget robotTarget && robotTarget.IsValid)
            {
                return new OffsetTarget(robotTarget, x, y, z);
            }

            if (value is OffsetTarget offsetTarget && offsetTarget.IsValid)
            {
                return new OffsetTarget(offsetTarget, x, y, z);
            }

            return null;
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
